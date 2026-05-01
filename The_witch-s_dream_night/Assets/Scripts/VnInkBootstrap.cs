using UnityEngine;

namespace VN
{
    /// <summary>
    /// 게임 시작 시 필요한 컴포넌트들을 조립(Wiring)하고 첫 스토리를 실행합니다.
    /// </summary>
    public sealed class VnInkBootstrap : MonoBehaviour
    {
        [Header("Required Components")]
        [SerializeField] private VNDirector director;
        [SerializeField] private TextAsset compiledInkJson;

        [Header("Configuration")]
        [SerializeField] private string startKnot = "start";

        private readonly InkStoryEngine engine = new();

        private void Start()
        {
            if (director == null)
            {
                // [자가 복구] 씬에서 VNDirector를 찾아 할당 시도
                director = FindFirstObjectByType<VNDirector>();
                if (director == null)
                {
                    Debug.LogError("[VNInkBootstrap] VNDirector is missing in the scene.");
                    return;
                }
            }

            if (compiledInkJson == null)
            {
                Debug.LogError("[VNInkBootstrap] compiledInkJson is missing.");
                return;
            }

            try
            {
                // 1. 데이터 엔진 초기화
                engine.Initialize(compiledInkJson);

                // 2. 디렉터 초기화 및 실행 권한 위임
                director.Initialize(engine);

                // 타이틀에서 '이어하기'를 눌렀는지 체크
                if (PlayerPrefs.HasKey("LoadOnStart_Slot"))
                {
                    int slot = PlayerPrefs.GetInt("LoadOnStart_Slot");
                    PlayerPrefs.DeleteKey("LoadOnStart_Slot"); // 사용 후 삭제
                    director.Load(slot);
                }
                else
                {
                    director.Play(startKnot);
                }

                Debug.Log("[VNInkBootstrap] Story started via VNDirector.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VNInkBootstrap] Bootstrap failed: {e}");
            }
        }
    }
}
