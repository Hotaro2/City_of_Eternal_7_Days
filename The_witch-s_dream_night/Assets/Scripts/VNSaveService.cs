using System.IO;
using UnityEngine;

namespace VN
{
    public static class VNSaveService
    {
        private static string SavePath(int slot) => Path.Combine(Application.persistentDataPath, $"save_{slot}.json");

        public static void Save(int slot, VNSaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath(slot), json);
                Debug.Log($"[VNSave] Slot {slot} saved: {SavePath(slot)}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VNSave] Slot {slot} save failed: {e.Message}");
            }
        }

        public static VNSaveData Load(int slot)
        {
            string path = SavePath(slot);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[VNSave] Slot {slot} file not found.");
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);
                VNSaveData data = JsonUtility.FromJson<VNSaveData>(json);
                if (data != null && string.IsNullOrWhiteSpace(data.saveDate))
                    data.saveDate = File.GetLastWriteTime(path).ToString("yyyy-MM-dd HH:mm");

                Debug.Log($"[VNSave] Slot {slot} loaded.");
                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VNSave] Slot {slot} load failed: {e.Message}");
                return null;
            }
        }

        public static bool Exists(int slot) => File.Exists(SavePath(slot));

        public static void Delete(int slot)
        {
            if (Exists(slot)) File.Delete(SavePath(slot));
        }

        public static int GetLatestSaveSlot()
        {
            int latestSlot = -1;
            System.DateTime latestTime = System.DateTime.MinValue;

            for (int i = 0; i < 100; i++)
            {
                if (!Exists(i)) continue;

                var lastWrite = File.GetLastWriteTime(SavePath(i));
                if (lastWrite <= latestTime) continue;

                latestTime = lastWrite;
                latestSlot = i;
            }

            return latestSlot;
        }
    }
}
