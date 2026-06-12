using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VN.MiniGames
{
    public sealed class MiniGameTestLauncher : MonoBehaviour
    {
        [SerializeField] private MiniGameManager manager;
        [SerializeField] private string playOnStartMiniGameId = "qte_sequence_001";
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private KeyCode singleQteKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode sequenceQteKey = KeyCode.Alpha2;
        [SerializeField] private string returnSceneName = "LobbyScene";
        [SerializeField] private TMP_Text statusText;

        private void Awake()
        {
            ResolveManager();
        }

        private IEnumerator Start()
        {
            yield return null;

            if (playOnStart && !string.IsNullOrWhiteSpace(playOnStartMiniGameId))
                StartMiniGame(playOnStartMiniGameId);
        }

        private void Update()
        {
            if (Input.GetKeyDown(singleQteKey))
                StartMiniGame("qte_defense_001");

            if (Input.GetKeyDown(sequenceQteKey))
                StartMiniGame("qte_sequence_001");
        }

        private void StartMiniGame(string miniGameId)
        {
            ResolveManager();
            if (manager == null)
            {
                SetStatus("MiniGameManager missing.");
                return;
            }

            bool started = manager.StartMiniGame(miniGameId, result =>
            {
                SetStatus(result == null
                    ? "Result: null"
                    : $"{result.miniGameId}: {(result.isSuccess ? "Success" : "Fail")} / {result.rank} / {result.score}");

                if (!string.IsNullOrWhiteSpace(returnSceneName))
                    SceneManager.LoadScene(returnSceneName);
            });

            if (started) SetStatus($"Running {miniGameId}");
        }

        private void ResolveManager()
        {
            if (manager != null) return;
            manager = FindFirstObjectByType<MiniGameManager>(FindObjectsInactive.Include);
        }

        private void SetStatus(string message)
        {
            if (statusText != null) statusText.text = message;
            Debug.Log($"[MiniGameTestLauncher] {message}");
        }
    }
}
