#if TMP_SUPPORT
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RicKit.EditorTools
{
    public class TMPSdfTool : EditorWindow
    {
        private static string folderPath;

        private void OnEnable()
        {
            folderPath = EditorPrefs.GetString($"TMPSdfTool_folderPath_{Application.identifier}", "Assets");
        }

        [MenuItem("RicKit/TMP/TMP SDF Tool")]
        public static void ShowWindow()
        {
            GetWindow<TMPSdfTool>("TMP SDF Tool");
        }

        private void OnGUI()
        {
            folderPath = EditorGUILayout.TextField("Folder Path", folderPath);
            if(GUILayout.Button("Generate SDF"))
            {
                EditorPrefs.SetString($"TMPSdfTool_folderPath_{Application.identifier}", folderPath);
                GenerateSDF();
            }
        }

        private static void GenerateSDF()
        {
            //加载该文件夹下所有资源
            var paths = AssetDatabase.FindAssets("t:TMP_FontAsset", new[] {folderPath});
            var assets = new List<Object>();
            foreach (var p in paths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(p));
                assets.Add(asset);
            }

            SetAtlasDynamic(assets);
            TMPAtlasAutoGenerate();
            SetAtlasStatic(assets);
        }
        
        [MenuItem("Assets/TMP Atlas Generate")]
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
        
        [MenuItem("Assets/TMP Atlas Dynamic")]
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
        
        [MenuItem("Assets/TMP Atlas Static")]
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