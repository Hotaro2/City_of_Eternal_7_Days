using UnityEngine;

namespace VN
{
    [System.Serializable]
    public class VNSettingsData
    {
        public float masterVolume = 1.0f;
        public float bgmVolume = 0.8f;
        public float sfxVolume = 0.8f;
        
        public float textSpeed = 0.5f; // 0: 아주 느림, 1: 즉시 출력
        public float autoWaitMultiplier = 1.0f; // 0.5 ~ 2.0 (낮을수록 빠름)
        public int resolutionWidth;
        public int resolutionHeight;
        public bool fullscreen;
        public bool hasDisplaySettings;

        public void Save()
        {
            PlayerPrefs.SetString("VNSettings", JsonUtility.ToJson(this));
            PlayerPrefs.Save();
        }

        public void ApplyDisplaySettings()
        {
            if (!hasDisplaySettings) return;
            if (resolutionWidth <= 0 || resolutionHeight <= 0) return;

            if (Screen.width == resolutionWidth &&
                Screen.height == resolutionHeight &&
                Screen.fullScreen == fullscreen)
            {
                return;
            }

            Screen.SetResolution(resolutionWidth, resolutionHeight, fullscreen);
        }

        public static VNSettingsData Load()
        {
            if (PlayerPrefs.HasKey("VNSettings"))
            {
                string json = PlayerPrefs.GetString("VNSettings");
                var data = JsonUtility.FromJson<VNSettingsData>(json);
                if (data != null) return data;
            }
            return new VNSettingsData();
        }
    }
}
