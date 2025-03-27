#if TMP_SUPPORT
using TMPro;
using UnityEngine;

namespace RicKit.Localization.Editor.TMP
{
    public class TMPSdfTool : ScriptableObject
    {
        public TMP_FontAsset[] fontAssets;
        public TextAsset[] textAssets;
    }
}
#endif