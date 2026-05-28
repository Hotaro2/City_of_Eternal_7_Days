using System;
using System.Collections;
using UnityEngine;

namespace VN.MiniGames
{
    public sealed class QTEMiniGame : MonoBehaviour, IMiniGameModule
    {
        [SerializeField] private QTEView view;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private float resultHoldSeconds = 0.45f;

        private QTEMiniGameDefinition currentDefinition;
        private Action<MiniGameResult> onComplete;
        private Coroutine routine;

        public string MiniGameType => QTEMiniGameDefinition.TypeId;
        public bool IsRunning => routine != null;

        private void Awake()
        {
            ResolveView();
            view?.SetVisible(false);
        }

        public void StartGame(MiniGameDefinition definition, Action<MiniGameResult> completeCallback)
        {
            StopGame();

            currentDefinition = definition as QTEMiniGameDefinition;
            if (currentDefinition == null)
            {
                Debug.LogError("[QTEMiniGame] Definition is not a QTEMiniGameDefinition.");
                completeCallback?.Invoke(CreateResult(false));
                return;
            }

            ResolveView();
            if (view == null)
            {
                Debug.LogError("[QTEMiniGame] QTEView is missing. Run VN Tools > MiniGames > Install Overlay In Active Scene in the scene that plays this Ink script.");
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
            bool success = false;

            while (remaining > 0f)
            {
                if (Input.GetKeyDown(currentDefinition.RequiredKey))
                {
                    success = true;
                    break;
                }

                remaining -= Time.unscaledDeltaTime;
                view?.UpdateTimer(remaining, currentDefinition.TimeLimitSeconds);
                yield return null;
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
                    ? new System.Collections.Generic.List<MiniGameInkVariableChange>(currentDefinition.GetVariableChanges(success))
                    : new System.Collections.Generic.List<MiniGameInkVariableChange>()
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
            view = GetComponentInChildren<QTEView>(true);
            if (view == null) view = FindFirstObjectByType<QTEView>(FindObjectsInactive.Include);
        }
    }
}
