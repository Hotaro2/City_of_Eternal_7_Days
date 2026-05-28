using System;
using System.Collections.Generic;
using UnityEngine;

namespace VN.MiniGames
{
    public sealed class MiniGameManager : MonoBehaviour
    {
        [Header("Definitions")]
        [SerializeField] private List<MiniGameDefinition> definitions = new();

        private readonly Dictionary<string, IMiniGameModule> modulesByType = new();
        private bool isRunning;

        public bool IsRunning => isRunning;
        public MiniGameResult LastResult { get; private set; }

        private void Awake()
        {
            RegisterChildModules();
            HideAllModules();
        }

        public bool StartMiniGame(string miniGameId, Action<MiniGameResult> onComplete)
        {
            if (isRunning || string.IsNullOrWhiteSpace(miniGameId)) return false;

            MiniGameDefinition definition = FindDefinition(miniGameId);
            if (definition == null)
            {
                Debug.LogError($"[MiniGameManager] MiniGameDefinition not found: {miniGameId}");
                return false;
            }

            if (!modulesByType.TryGetValue(definition.MiniGameType, out IMiniGameModule module) || module == null)
            {
                Debug.LogError($"[MiniGameManager] MiniGame module not found for type: {definition.MiniGameType}");
                return false;
            }

            isRunning = true;
            module.StartGame(definition, result =>
            {
                isRunning = false;
                LastResult = result;
                onComplete?.Invoke(result);
            });

            return true;
        }

        public void StopCurrentMiniGame()
        {
            foreach (IMiniGameModule module in modulesByType.Values)
            {
                if (module != null && module.IsRunning)
                    module.StopGame();
            }

            isRunning = false;
        }

        private void RegisterChildModules()
        {
            modulesByType.Clear();
            var behaviours = GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is not IMiniGameModule module) continue;
                if (string.IsNullOrWhiteSpace(module.MiniGameType)) continue;

                modulesByType[module.MiniGameType] = module;
            }
        }

        private void HideAllModules()
        {
            foreach (IMiniGameModule module in modulesByType.Values)
                module?.StopGame();
        }

        private MiniGameDefinition FindDefinition(string miniGameId)
        {
            for (int i = 0; i < definitions.Count; i++)
            {
                MiniGameDefinition definition = definitions[i];
                if (definition == null) continue;
                if (string.Equals(definition.MiniGameId, miniGameId, StringComparison.OrdinalIgnoreCase))
                    return definition;
            }

            return null;
        }

    }
}
