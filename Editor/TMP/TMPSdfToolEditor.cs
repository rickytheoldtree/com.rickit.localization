#if TMP_SUPPORT
using System.Text;
using RicKit.Localization.Editor.TMP;
using TMPro;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RicKit.EditorTools
{
    [CustomEditor(typeof(TMPSdfTool))]
    public class TMPSdfToolEditor : UnityEditor.Editor
    {
        [MenuItem("RicKit/TMP/TMP SDF Tool")]
        public static void Open()
        {
            var path = AssetDatabase.FindAssets("t:TMPSdfTool");
            if (path.Length == 0)
            {
                var tool = CreateInstance<TMPSdfTool>();
                AssetDatabase.CreateAsset(tool, "Assets/TMPTool.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>("Assets/TMPTool.asset");
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var tool = (TMPSdfTool)target;
            if (GUILayout.Button("Update SDF Atlas"))
            {
                UpdateSDFAtlas(tool.fontAssets, tool.textAssets);
            }
        }
        
        private static void UpdateSDFAtlas(TMP_FontAsset[] assets, TextAsset[] textAssets)
        {
            SetAtlasDynamic(assets);
            ClearDynamicData(assets);
            TMPAtlasAutoGenerate(textAssets);
            SetAtlasStatic(assets);
        }

        private static void ClearDynamicData(TMP_FontAsset[] assets)
        {
            foreach (var obj in assets)
            {
                obj.ClearFontAssetData();
                EditorUtility.SetDirty(obj);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void TMPAtlasAutoGenerate(TextAsset[] textAssets)
        {
            if (textAssets.Length == 0)
            {
                Debug.LogError("No TextAsset selected.");
                return;
            }

            var sb = new StringBuilder();
            foreach (var textAsset in textAssets)
            {
                sb.Append(textAsset.text);
            }
            
            var go = new GameObject("TMPText(Temp asset, delete it)", typeof(TextMeshPro));
            var text = go.GetComponent<TextMeshPro>();
            text.text = sb.ToString();
            text.ForceMeshUpdate();
            DestroyImmediate(go);
        }
        private static void SetAtlasDynamic(TMP_FontAsset[] objects)
        {
            foreach (var obj in objects)
            {
                obj.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                EditorUtility.SetDirty(obj);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void SetAtlasStatic(TMP_FontAsset[] objects)
        {
            foreach (var obj in objects)
            {
                obj.atlasPopulationMode = AtlasPopulationMode.Static;
                EditorUtility.SetDirty(obj);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif