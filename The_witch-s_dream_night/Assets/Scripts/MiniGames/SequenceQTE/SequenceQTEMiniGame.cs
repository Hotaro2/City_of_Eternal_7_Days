using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VN.MiniGames
{
    public sealed class SequenceQTEMiniGame : MonoBehaviour, IMiniGameModule
    {
        [SerializeField] private SequenceQTEView view;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private float wrongFlashSeconds = 0.12f;
        [SerializeField] private float roundPauseSeconds = 0.2f;
        [SerializeField] private float resultHoldSeconds = 0.55f;

        private SequenceQTEMiniGameDefinition currentDefinition;
        private Action<MiniGameResult> onComplete;
        private Coroutine routine;

        public string MiniGameType => SequenceQTEMiniGameDefinition.TypeId;
        public bool IsRunning => routine != null;

        private void Awake()
        {
            ResolveView();
            view?.SetVisible(false);
        }

        public void StartGame(MiniGameDefinition definition, Action<MiniGameResult> completeCallback)
        {
            StopGame();

            currentDefinition = definition as SequenceQTEMiniGameDefinition;
            if (currentDefinition == null)
            {
                Debug.LogError("[SequenceQTEMiniGame] Definition is not a SequenceQTEMiniGameDefinition.");
                completeCallback?.Invoke(CreateResult(false));
                return;
            }

            ResolveView();
            if (view == null)
            {
                Debug.LogError("[SequenceQTEMiniGame] SequenceQTEView is missing. Run VN Tools > MiniGames > Install Overlay In Active Scene in the scene that plays this Ink script.");
                completeCallback?.Invoke(CreateResult(false));
                return;
            }

            onComplete = completeCallback;
            routine = StartCoroutine(RunRoutine());
        }

        public void StopGame()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }

            view?.SetVisible(false);
            onComplete = null;
            currentDefinition = null;
        }

        private IEnumerator RunRoutine()
        {
            view?.ShowReady(currentDefinition);

            float remaining = currentDefinition.TimeLimitSeconds;
            int mistakes = 0;
            bool success = true;

            for (int round = 0; round < currentDefinition.RoundCount; round++)
            {
                List<KeyCode> sequence = CreateSequence(currentDefinition);
                int currentIndex = 0;
                view?.ShowSequence(sequence, currentIndex, round, currentDefinition.RoundCount, mistakes, currentDefinition.MistakeLimit);

                while (currentIndex < sequence.Count)
                {
                    remaining -= Time.unscaledDeltaTime;
                    view?.UpdateTimer(remaining, currentDefinition.TimeLimitSeconds);

                    if (remaining <= 0f)
                    {
                        success = false;
                        break;
                    }

                    if (TryReadAllowedKey(currentDefinition, out KeyCode pressedKey))
                    {
                        if (pressedKey == sequence[currentIndex])
                        {
                            currentIndex++;
                            PlaySfx(currentDefinition.CorrectSfx);
                            view?.ShowSequence(sequence, currentIndex, round, currentDefinition.RoundCount, mistakes, currentDefinition.MistakeLimit);
                        }
                        else
                        {
                            mistakes++;
                            PlaySfx(currentDefinition.WrongSfx);
                            view?.ShowMistake(sequence, currentIndex);
                            yield return new WaitForSecondsRealtime(Mathf.Max(0f, wrongFlashSeconds));
                            view?.ShowSequence(sequence, currentIndex, round, currentDefinition.RoundCount, mistakes, currentDefinition.MistakeLimit);

                            if (mistakes > currentDefinition.MistakeLimit)
                            {
                                success = false;
                                break;
                            }
                        }
                    }

                    yield return null;
                }

                if (!success) break;
                if (round < currentDefinition.RoundCount - 1)
                    yield return new WaitForSecondsRealtime(Mathf.Max(0f, roundPauseSeconds));
            }

            MiniGameResult result = CreateResult(success);
            PlaySfx(success ? currentDefinition.SuccessSfx : currentDefinition.FailSfx);
            view?.ShowResult(success, result.rank);

            yield return new WaitForSecondsRealtime(Mathf.Max(0f, resultHoldSeconds));

            routine = null;
            view?.SetVisible(false);
            Action<MiniGameResult> callback = onComplete;
            onComplete = null;
            currentDefinition = null;
            callback?.Invoke(result);
        }

        private static List<KeyCode> CreateSequence(SequenceQTEMiniGameDefinition definition)
        {
            var pool = new List<KeyCode>();
            for (int i = 0; i < definition.InputPool.Count; i++)
            {
                KeyCode key = definition.InputPool[i];
                if (key != KeyCode.None) pool.Add(key);
            }

            if (pool.Count == 0) pool.Add(KeyCode.Space);

            var sequence = new List<KeyCode>(definition.SequenceLength);
            for (int i = 0; i < definition.SequenceLength; i++)
                sequence.Add(pool[UnityEngine.Random.Range(0, pool.Count)]);

            return sequence;
        }

        private static bool TryReadAllowedKey(SequenceQTEMiniGameDefinition definition, out KeyCode pressedKey)
        {
            for (int i = 0; i < definition.InputPool.Count; i++)
            {
                KeyCode key = definition.InputPool[i];
                if (key == KeyCode.None) continue;
                if (!Input.GetKeyDown(key)) continue;

                pressedKey = key;
                return true;
            }

            pressedKey = KeyCode.None;
            return false;
        }

        private MiniGameResult CreateResult(bool success)
        {
            string miniGameId = currentDefinition != null ? currentDefinition.MiniGameId : string.Empty;
            string rank = currentDefinition != null ? (success ? currentDefinition.SuccessRank : currentDefinition.FailRank) : string.Empty;
            string flag = currentDefinition != null ? (success ? currentDefinition.SuccessFlag : currentDefinition.FailFlag) : string.Empty;
            int score = currentDefinition != null ? (success ? currentDefinition.SuccessScore : currentDefinition.FailScore) : 0;
            string variableName = currentDefinition != null ? currentDefinition.ResultVariableName : string.Empty;

            return new MiniGameResult
            {
                miniGameId = miniGameId,
                isSuccess = success,
                score = score,
                rank = rank,
                gainedFlag = flag,
                resultVariableName = variableName,
                variableChanges = currentDefinition != null
                    ? new List<MiniGameInkVariableChange>(currentDefinition.GetVariableChanges(success))
                    : new List<MiniGameInkVariableChange>()
            };
        }

        private void PlaySfx(AudioClip clip)
        {
            if (clip == null) return;
            if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
            if (sfxSource != null) sfxSource.PlayOneShot(clip);
        }

        private void ResolveView()
        {
            if (view != null) return;
            view = GetComponentInChildren<SequenceQTEView>(true);
            if (view == null) view = FindFirstObjectByType<SequenceQTEView>(FindObjectsInactive.Include);
        }
    }
}
