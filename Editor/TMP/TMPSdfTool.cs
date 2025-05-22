#if TMP_SUPPORT
using TMPro;
using UnityEngine;

namespace RicKit.Localization.Editor.TMP
{
    [CreateAssetMenu(fileName = "TMPSdfTool", menuName = "RicKit/TMP/SDF Tool")]
    public class TMPSdfTool : ScriptableObject
    {
        public TMP_FontAsset defaultFont;
        public TMP_FontAsset[] fallbackFonts;
        public TextAsset[] textAssets;
    }
}
#endif