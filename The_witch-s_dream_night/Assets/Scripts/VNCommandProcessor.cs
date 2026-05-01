using System;
using System.Collections.Generic;
using UnityEngine;

namespace VN
{
    /// <summary>
    /// Ink 태그를 해석하여 실제 연출(Presenter) 명령으로 변환합니다.
    /// </summary>
    public sealed class VNCommandProcessor
    {
        private readonly IVNPresenter presenter;

        public VNCommandProcessor(IVNPresenter presenter)
        {
            this.presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
        }

        public void Process(IReadOnlyList<string> tags)
        {
            if (tags == null || tags.Count == 0) return;

            foreach (var rawTag in tags)
            {
                string tag = rawTag.Trim();
                if (string.IsNullOrWhiteSpace(tag)) continue;

                var parts = tag.Split(new[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;

                string cmd = parts[0].ToLower();
                if (cmd.StartsWith("#")) cmd = cmd.Substring(1);

                switch (cmd)
                {
                    case "bg": // #bg [키] [트랜지션]
                        {
                            string bgKey = parts[1];
                            string trans = (parts.Length > 2) ? parts[2].ToLower() : null;
                            presenter.SetBackground(bgKey, trans);
                        }
                        break;

                    case "ch": // #ch [이름] [표정] [위치] [트랜지션]
                        {
                            string charName = parts[1];
                            string expr = "default";
                            string pos = "center";
                            string trans = null;

                            if (parts.Length == 3)
                            {
                                string arg = parts[2].ToLower();
                                if (IsPosition(arg)) pos = arg;
                                else if (arg == "fade") trans = arg;
                                else expr = arg;
                            }
                            else if (parts.Length == 4)
                            {
                                // #ch 이름 표정 위치  OR  #ch 이름 위치 fade
                                if (IsPosition(parts[2].ToLower()))
                                {
                                    pos = parts[2].ToLower();
                                    if (parts[3].ToLower() == "fade") trans = "fade";
                                }
                                else
                                {
                                    expr = parts[2];
                                    if (IsPosition(parts[3].ToLower())) pos = parts[3].ToLower();
                                    else if (parts[3].ToLower() == "fade") trans = "fade";
                                }
                            }
                            else if (parts.Length >= 5)
                            {
                                expr = parts[2];
                                pos = parts[3];
                                if (parts[4].ToLower() == "fade") trans = "fade";
                            }
                            presenter.SetCharacter(charName, expr, pos, trans);
                        }
                        break;

                    case "hide": // #hide [이름] [트랜지션]
                        {
                            string charName = parts[1];
                            string trans = (parts.Length > 2) ? parts[2].ToLower() : null;
                            presenter.HideCharacter(charName, trans);
                        }
                        break;

                    case "shake": // #shake [시간] [강도]
                        {
                            float duration = 0.5f;
                            float strength = 10f;
                            if (parts.Length > 1) float.TryParse(parts[1], out duration);
                            if (parts.Length > 2) float.TryParse(parts[2], out strength);
                            presenter.ShakeScreen(duration, strength);
                        }
                        break;

                    case "bgm":
                        presenter.PlayBGM(parts[1]);
                        break;

                    case "sfx":
                        presenter.PlaySFX(parts[1]);
                        break;
                }
            }
        }

        private bool IsPosition(string value)
        {
            return value == "left" || value == "center" || value == "right";
        }
    }
}
