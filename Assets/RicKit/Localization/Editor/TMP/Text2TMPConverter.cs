#if TMP_SUPPORT
using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace RicKit.Editor
{
    public class TextToTextMeshProConverter : EditorWindow
    {
        [MenuItem("RicKit/TMP/Text to TMP")]
        public static void ShowWindow()
        {
            GetWindow<TextToTextMeshProConverter>("Text to TMP");
        }

        private Material defaultSDFMaterial;
        private Material outlineMaterial;
        private Material shadowMaterial;
        private TMP_FontAsset defaultTMPFont;

        private void OnGUI()
        {
            defaultTMPFont = (TMP_FontAsset)EditorGUILayout.ObjectField("默认 TMP 字体", defaultTMPFont, typeof(TMP_FontAsset), false);
            defaultSDFMaterial = (Material)EditorGUILayout.ObjectField("默认 SDF 材质", defaultSDFMaterial, typeof(Material), false);
            outlineMaterial = (Material)EditorGUILayout.ObjectField("Outline 材质", outlineMaterial, typeof(Material), false);
            shadowMaterial = (Material)EditorGUILayout.ObjectField("Shadow 材质", shadowMaterial, typeof(Material), false);
            
            if (GUILayout.Button("转换选中物体的 Text"))
            {
                ConvertAndApplyMaterials();
            }
        }

        private void ConvertAndApplyMaterials()
        {
            foreach (var obj in Selection.gameObjects)
            {
                var textComponent = obj.GetComponent<Text>();
                if (!textComponent) continue;
                DestroyImmediate(textComponent);
                
                var tmpComponent = obj.AddComponent<TextMeshProUGUI>();
                tmpComponent.text = textComponent.text;
                tmpComponent.fontSize = textComponent.fontSize;
                tmpComponent.color = textComponent.color;
                tmpComponent.alignment = ConvertTextAnchorToTMPAlignment(textComponent.alignment);
                tmpComponent.fontStyle = ConvertFontStyleToTMPFontStyle(textComponent.fontStyle);
                tmpComponent.font = defaultTMPFont;
                tmpComponent.enableAutoSizing = textComponent.resizeTextForBestFit;
                tmpComponent.fontSizeMin = textComponent.resizeTextMinSize;
                tmpComponent.fontSizeMax = textComponent.resizeTextMaxSize;
                tmpComponent.enableWordWrapping = textComponent.horizontalOverflow == HorizontalWrapMode.Wrap;
                tmpComponent.overflowMode = textComponent.verticalOverflow == VerticalWrapMode.Truncate ? TextOverflowModes.Truncate : TextOverflowModes.Overflow;
                tmpComponent.raycastTarget = textComponent.raycastTarget;
                
                
                tmpComponent.fontSharedMaterial = defaultSDFMaterial;
                if (tmpComponent.TryGetComponent(out Outline outline))
                {
                    tmpComponent.fontSharedMaterial = outlineMaterial;
                    DestroyImmediate(outline);
                }
                if (tmpComponent.TryGetComponent(out Shadow shadow))
                {
                    tmpComponent.fontSharedMaterial = shadowMaterial;
                    DestroyImmediate(shadow);
                }
                
                //设脏
                EditorUtility.SetDirty(obj);
            }
        }
        
        private TextAlignmentOptions ConvertTextAnchorToTMPAlignment(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft:
                    return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter:
                    return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight:
                    return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft:
                    return TextAlignmentOptions.Left;
                case TextAnchor.MiddleCenter:
                    return TextAlignmentOptions.Center;
                case TextAnchor.MiddleRight:
                    return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft:
                    return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter:
                    return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight:
                    return TextAlignmentOptions.BottomRight;
                default:
                    return TextAlignmentOptions.Left; // Default alignment
            }
        }
        
        private FontStyles ConvertFontStyleToTMPFontStyle(FontStyle style)
        {
            switch (style)
            {
                case FontStyle.Normal:
                    return FontStyles.Normal;
                case FontStyle.Bold:
                    return FontStyles.Bold;
                case FontStyle.Italic:
                    return FontStyles.Italic;
                case FontStyle.BoldAndItalic:
                    return FontStyles.Bold | FontStyles.Italic;
                default:
                    return FontStyles.Normal; // Default style
            }
        }

    }
}
#endif
