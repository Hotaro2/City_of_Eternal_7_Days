using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace VN
{
    public static class VNKoreanFontFallback
    {
        private static TMP_FontAsset fallbackFontAsset;

        public static void ApplyTo(TMP_Text text)
        {
            EnsureFallbackFont();
            if (text == null || fallbackFontAsset == null || text.font == null) return;

            var fallbackTable = text.font.fallbackFontAssetTable;
            if (fallbackTable == null || fallbackTable.Contains(fallbackFontAsset)) return;

            fallbackTable.Add(fallbackFontAsset);
            text.ForceMeshUpdate();
        }

        public static void ApplyToAllIn(GameObject root)
        {
            if (root == null) return;

            var texts = root.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                ApplyTo(texts[i]);
            }
        }

        private static void EnsureFallbackFont()
        {
            if (fallbackFontAsset != null) return;

            Font font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Arial Unicode MS" }, 90);
            if (font == null) return;

            fallbackFontAsset = TMP_FontAsset.CreateFontAsset(
                font,
                90,
                9,
                GlyphRenderMode.SDFAA,
                2048,
                2048,
                AtlasPopulationMode.Dynamic,
                true);

            if (fallbackFontAsset != null)
            {
                fallbackFontAsset.name = "Runtime Korean Fallback";
            }
        }
    }
}
