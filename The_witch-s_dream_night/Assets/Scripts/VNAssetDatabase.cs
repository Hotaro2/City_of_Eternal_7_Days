using System;
using System.Collections.Generic;
using UnityEngine;

namespace VN
{
    /// <summary>
    /// 문자열 키와 실제 유니티 에셋(Sprite, AudioClip)을 매핑하는 데이터베이스입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "VNAssetDatabase", menuName = "VN/Asset Database")]
    public sealed class VNAssetDatabase : ScriptableObject
    {
        [Serializable]
        public struct SpriteEntry { public string key; public Sprite sprite; }
        
        [Serializable]
        public struct AudioEntry { public string key; public AudioClip clip; }

        [Serializable]
        public struct ExpressionEntry
        {
            public string expressionName; // 예: happy, sad
            public Sprite sprite;
        }

        [Serializable]
        public struct CharacterAsset
        {
            public string characterName; // 캐릭터 이름 (예: amy)
            public Sprite defaultSprite; // 기본 이미지 (표정이 없을 때 사용)
            public List<ExpressionEntry> expressions; // 표정 리스트
        }

        [Header("Visuals - Backgrounds")]
        public List<SpriteEntry> backgrounds = new();

        [Header("Visuals - Characters (Grouped)")]
        public List<CharacterAsset> characters = new();

        [Header("Audio")]
        public List<AudioEntry> bgm = new();
        public List<AudioEntry> sfx = new();

        public Sprite GetBackground(string key) 
        {
            var entry = backgrounds.Find(e => e.key.Equals(key, StringComparison.OrdinalIgnoreCase));
            return entry.sprite;
        }

        /// <summary>
        /// 캐릭터 이름과 표정 이름을 기반으로 스프라이트를 찾습니다.
        /// </summary>
        public Sprite GetCharacter(string charName, string expression = null) 
        {
            var charAsset = characters.Find(c => c.characterName.Equals(charName, StringComparison.OrdinalIgnoreCase));
            
            // 캐릭터 자체가 없으면 null 반환
            if (string.IsNullOrEmpty(charAsset.characterName)) return null;

            // 표정 이름이 없거나 "default"면 기본 스프라이트 반환
            if (string.IsNullOrEmpty(expression) || expression.Equals("default", StringComparison.OrdinalIgnoreCase))
                return charAsset.defaultSprite;

            // 표정 리스트에서 검색
            var exprEntry = charAsset.expressions.Find(e => e.expressionName.Equals(expression, StringComparison.OrdinalIgnoreCase));
            
            // 표정을 못 찾으면 기본 스프라이트로 대체
            return exprEntry.sprite != null ? exprEntry.sprite : charAsset.defaultSprite;
        }

        public AudioClip GetBGM(string key) 
        {
            var entry = bgm.Find(e => e.key.Equals(key, StringComparison.OrdinalIgnoreCase));
            return entry.clip;
        }

        public AudioClip GetSFX(string key) 
        {
            var entry = sfx.Find(e => e.key.Equals(key, StringComparison.OrdinalIgnoreCase));
            return entry.clip;
        }
    }
}
