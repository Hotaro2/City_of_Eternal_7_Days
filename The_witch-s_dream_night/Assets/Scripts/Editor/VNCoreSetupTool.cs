using UnityEditor;
using UnityEngine;
using VN;

namespace VN.Editor
{
    public static class VNCoreSetupTool
    {
        [MenuItem("VN Tools/Setup Core System (Director & Bootstrap)")]
        public static void SetupCore()
        {
            // 1. VNUIController 찾기 (모든 연결의 중심)
            var ui = Object.FindFirstObjectByType<VNUIController>();
            if (ui == null)
            {
                Debug.LogError("[VN Tools] Scene에 VNUIController가 없습니다. 먼저 UI를 생성하세요.");
                return;
            }

            // 2. VNDirector 설정
            var director = Object.FindFirstObjectByType<VNDirector>();
            if (director == null)
            {
                GameObject directorObj = new GameObject("VNDirector");
                director = directorObj.AddComponent<VNDirector>();
                Undo.RegisterCreatedObjectUndo(directorObj, "Create VNDirector");
                Debug.Log("[VN Tools] VNDirector 오브젝트를 생성했습니다.");
            }

            // VNDirector에 UI 연결 (SerializedObject를 사용하여 private 필드 강제 할당)
            SerializedObject soDirector = new SerializedObject(director);
            var uiProp = soDirector.FindProperty("ui");
            if (uiProp != null)
            {
                uiProp.objectReferenceValue = ui;
                soDirector.ApplyModifiedProperties();
                Debug.Log("[VN Tools] VNDirector에 VNUIController를 연결했습니다.");
            }

            // 3. VnInkBootstrap 설정
            var bootstrap = Object.FindFirstObjectByType<VnInkBootstrap>();
            if (bootstrap == null)
            {
                GameObject bootstrapObj = new GameObject("VNBootstrap");
                bootstrap = bootstrapObj.AddComponent<VnInkBootstrap>();
                Undo.RegisterCreatedObjectUndo(bootstrapObj, "Create VNBootstrap");
                Debug.Log("[VN Tools] VnInkBootstrap 오브젝트를 생성했습니다.");
            }

            // Bootstrap에 Director 연결
            SerializedObject soBootstrap = new SerializedObject(bootstrap);
            var directorProp = soBootstrap.FindProperty("director");
            if (directorProp != null)
            {
                directorProp.objectReferenceValue = director;
                soBootstrap.ApplyModifiedProperties();
                Debug.Log("[VN Tools] VnInkBootstrap에 VNDirector를 연결했습니다.");
            }

            // 변경사항 저장
            EditorUtility.SetDirty(director);
            EditorUtility.SetDirty(bootstrap);
            
            Debug.Log("<color=green>[VN Tools] Core System 설정이 완료되었습니다!</color>");
            Selection.activeObject = bootstrap.gameObject;
        }
    }
}
