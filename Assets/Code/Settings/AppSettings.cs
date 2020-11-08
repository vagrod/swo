using UnityEngine;

namespace SeaWarsOnline.Core
{
    public class AppSettings
    {

        public enum PaperEffectTypes {
            PartialEffect = 0,
            FullEffect = 1,
            NoEffect = 2
        }

        public string Language { get; set; }
        public bool AutoScroll { get; set; }
        public bool IsPortraitMode { get; set; }
        public string CellAnimation { get; set; }
        public string SkinName { get; set; }
        public PaperEffectTypes PaperEffect { get; set; }

        public void Load(){
            Language = PlayerPrefs.GetString("Language", "English");
            AutoScroll = PlayerPrefs.GetInt("AutoScroll", 1) == 1;
            CellAnimation = PlayerPrefs.GetString("CellAnimation", "Wave");
            SkinName = PlayerPrefs.GetString("SkinName", "default");
            PaperEffect = DetectPaperEffect();
            IsPortraitMode = DetectOrientation();
        }

        private PaperEffectTypes DetectPaperEffect(){
            if (Input.touchSupported)
                return (PaperEffectTypes)PlayerPrefs.GetInt("PaperEffect", 0);
             
            return (PaperEffectTypes)PlayerPrefs.GetInt("PaperEffect", 2);
        }

        private bool DetectOrientation(){
#if UNITY_WSA_10_0 ||  UNITY_WSA ||  WINDOWS_UWP
            var isDefaultTablet = true;
#else 
            var isDefaultTablet = false;
            var side1 = Display.main.systemWidth;

            if (side1 < 1)
                side1 = Screen.currentResolution.width;

            var side2 = Display.main.systemHeight;

            if (side2 < 1)
                side2 = Screen.currentResolution.height;

            if (side1 > 0f && side2 > 0f)
            {
                var min = Mathf.Min(side1, side2);
                var max = Mathf.Max(side1, side2);
                var ratio = (float)min / (float)max;

                isDefaultTablet = ratio > 0.63f;
            }
#endif 
            
            if (isDefaultTablet)
                return PlayerPrefs.GetInt("IsPortraitMode", 0) == 1;
            else 
                return PlayerPrefs.GetInt("IsPortraitMode", 1) == 1;
        }

        public void Save(){
             PlayerPrefs.SetString("Language", Language);
             PlayerPrefs.SetInt("AutoScroll", AutoScroll ? 1 : 0);
             PlayerPrefs.SetString("CellAnimation", CellAnimation);
             PlayerPrefs.SetString("SkinName", SkinName);
             PlayerPrefs.SetInt("IsPortraitMode", IsPortraitMode ? 1 : 0);
             PlayerPrefs.SetInt("PaperEffect", (int)PaperEffect);
        }

    }
}