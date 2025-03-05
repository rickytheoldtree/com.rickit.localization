#if TMP_SUPPORT
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RicKit.EditorTools
{
    public class TMPSdfTool : UnityEditor.Editor
    {
        [MenuItem("Assets/TMP Tool/Fully Generate TMP Atlas")]
        public static void GenerateSDF()
        {
            GenerateSDF("Assets");
        }
        private static void GenerateSDF(string folderPath)
        {
            var paths = AssetDatabase.FindAssets("t:TMP_FontAsset", new[] {folderPath});
            var assets = new List<Object>();
            foreach (var p in paths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(p));
                assets.Add(asset);
            }

            SetAtlasDynamic(assets);
            ClearDynamicData(assets);
            TMPAtlasAutoGenerate();
            SetAtlasStatic(assets);
        }

        private static void ClearDynamicData(IEnumerable<Object> assets)
        {
            foreach (var obj in assets)
            {
                if (obj is TMP_FontAsset fontAsset)
                {
                    fontAsset.ClearFontAssetData();
                    EditorUtility.SetDirty(fontAsset);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/TMP Tool/Generate TMP Atlas")]
        public static void TMPAtlasAutoGenerate()
        {
            var textAssets = Selection.GetFiltered(typeof(TextAsset), SelectionMode.Assets);
            if (textAssets.Length == 0)
            {
                Debug.LogError("No TextAsset selected.");
                return;
            }

            var sb = new StringBuilder();
            foreach (var o in textAssets)
            {
                var textAsset = (TextAsset)o;
                sb.Append(textAsset.text);

            }
            
            var go = new GameObject("TMPText(Temp asset, delete it)", typeof(TextMeshPro));
            var text = go.GetComponent<TextMeshPro>();
            text.text = sb.ToString();
            text.ForceMeshUpdate();
            DestroyImmediate(go);
        }
        
        [MenuItem("Assets/TMP Tool/TMP Atlas Dynamic")]
        public static void SetAtlasDynamic()
        {
            SetAtlasDynamic(Selection.objects);
        }

        private static void SetAtlasDynamic(IEnumerable<Object> objects)
        {
            foreach (var obj in objects)
            {
                if (obj is TMP_FontAsset fontAsset)
                {
                    fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                    EditorUtility.SetDirty(fontAsset);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Assets/TMP Tool/TMP Atlas Static")]
        public static void SetAtlasStatic()
        {
            SetAtlasStatic(Selection.objects);
        }

        private static void SetAtlasStatic(IEnumerable<Object> objects)
        {
            foreach (var obj in objects)
            {
                if (obj is TMP_FontAsset fontAsset)
                {
                    fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;
                    EditorUtility.SetDirty(fontAsset);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif