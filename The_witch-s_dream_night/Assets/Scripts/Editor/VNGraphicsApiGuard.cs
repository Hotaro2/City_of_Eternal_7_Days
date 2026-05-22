#if UNITY_EDITOR_WIN
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace VN.Editor
{
    [InitializeOnLoad]
    public static class VNGraphicsApiGuard
    {
        private const string WarnedD3D12SessionKey = "VNGraphicsApiGuard.WarnedD3D12";

        static VNGraphicsApiGuard()
        {
            EditorApplication.delayCall += EnsureDirect3D11;
        }

        private static void EnsureDirect3D11()
        {
            WarnIfEditorStillUsesD3D12();

            GraphicsDeviceType[] currentApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
            bool shouldUpdate = PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64)
                || currentApis.Length != 1
                || currentApis[0] != GraphicsDeviceType.Direct3D11;

            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
            PlayerSettings.SetGraphicsAPIs(
                BuildTarget.StandaloneWindows64,
                new[] { GraphicsDeviceType.Direct3D11 });

            if (!shouldUpdate) return;

            AssetDatabase.SaveAssets();
            Debug.Log("[VNGraphicsApiGuard] Windows player graphics API was set to Direct3D11.");
        }

        private static void WarnIfEditorStillUsesD3D12()
        {
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Direct3D12
                || SessionState.GetBool(WarnedD3D12SessionKey, false))
            {
                return;
            }

            SessionState.SetBool(WarnedD3D12SessionKey, true);
            Debug.LogWarning("[VNGraphicsApiGuard] This Unity editor session is still running on Direct3D12. If the editor exits with a D3D12 device error again, restart Unity with -force-d3d11.");
        }
    }
}
#endif
