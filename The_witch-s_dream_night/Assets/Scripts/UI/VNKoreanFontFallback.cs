using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;

namespace VN
{
    public static class VNKoreanFontFallback
    {
        private static TMP_FontAsset fallbackFontAsset;
        private static bool hasTriedCreateFont;
        private const string RuntimeFontResourcePath = "Fonts/LINESeedKR-Rg";

        public static TMP_FontAsset FontAsset
        {
            get
            {
                EnsureFallbackFont();
                return fallbackFontAsset;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BootstrapBeforeSceneLoad()
        {
            EnsureFallbackFont();
            ApplyToTMPSettings();

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapAfterSceneLoad()
        {
            ApplyToAllLoadedScenes();
        }

        public static void ApplyTo(TMP_Text text)
        {
            EnsureFallbackFont();
            if (text == null || fallbackFontAsset == null) return;

            try
            {
                if (text.font != fallbackFontAsset)
                    text.font = fallbackFontAsset;

                ApplyMaterial(text);
            }
            catch (MissingReferenceException exception)
            {
                Debug.LogError($"[VNKoreanFontFallback] Failed to apply LINE Seed KR font because a previous TMP font texture was already destroyed. Object: {text.name}\n{exception}");
            }
            catch (System.NullReferenceException exception)
            {
                Debug.LogError($"[VNKoreanFontFallback] Failed to apply LINE Seed KR font. Object: {text.name}\n{exception}");
            }
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

        public static void ApplyToAllLoadedScenes()
        {
            EnsureFallbackFont();
            ApplyToTMPSettings();
            if (fallbackFontAsset == null) return;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                GameObject[] roots = scene.GetRootGameObjects();
                for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                {
                    ApplyToAllIn(roots[rootIndex]);
                }
            }
        }

        public static void EnsureFallbackFont()
        {
            if (fallbackFontAsset != null || hasTriedCreateFont) return;
            hasTriedCreateFont = true;

            Font sourceFont = Resources.Load<Font>(RuntimeFontResourcePath);
            if (sourceFont == null)
            {
                Debug.LogError("[VNKoreanFontFallback] LINESeedKR-Rg.ttf is missing under Assets/Resources/Fonts.");
                return;
            }

            fallbackFontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                90,
                9,
                GlyphRenderMode.SDFAA,
                2048,
                2048,
                AtlasPopulationMode.Dynamic,
                true);

            if (fallbackFontAsset == null)
            {
                Debug.LogError("[VNKoreanFontFallback] Failed to create runtime LINE Seed KR TMP font asset.");
                return;
            }

            if (fallbackFontAsset.material == null || fallbackFontAsset.atlasTexture == null)
            {
                Debug.LogError("[VNKoreanFontFallback] Runtime LINE Seed KR TMP font asset has no valid material or atlas texture.");
                fallbackFontAsset = null;
                return;
            }

            fallbackFontAsset.name = "Runtime LINE Seed KR";
            fallbackFontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            fallbackFontAsset.isMultiAtlasTexturesEnabled = true;

            ApplyToTMPSettings();
        }

        private static void ApplyToTMPSettings()
        {
            if (fallbackFontAsset == null) return;

            TMP_Settings.defaultFontAsset = fallbackFontAsset;

            List<TMP_FontAsset> fallbackAssets = TMP_Settings.fallbackFontAssets;
            if (fallbackAssets == null)
            {
                fallbackAssets = new List<TMP_FontAsset>();
                TMP_Settings.fallbackFontAssets = fallbackAssets;
            }

            if (!fallbackAssets.Contains(fallbackFontAsset))
                fallbackAssets.Insert(0, fallbackFontAsset);
        }

        private static void ApplyMaterial(TMP_Text text)
        {
            if (text == null || fallbackFontAsset == null || fallbackFontAsset.material == null) return;

            Material material = fallbackFontAsset.material;
            if (material != null && text.fontSharedMaterial != material)
                text.fontSharedMaterial = material;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyToAllLoadedScenes();
        }
    }
}
