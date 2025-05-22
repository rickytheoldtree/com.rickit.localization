using System;
using System.Collections.Generic;
using System.Linq;
using RicKit.Localization.DictConvertor;
using RicKit.Localization.Utils;
using UnityEditor;
using UnityEngine;

namespace RicKit.Localization.Config
{
    [CustomEditor(typeof(Config),true)]
    public class ConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty isoPair;
        private Config config;
        public void OnEnable()
        {
            isoPair = serializedObject.FindProperty("languageIsoPairs");
            config = (Config)target;
            config.Refresh();
        }

        public override void OnInspectorGUI()
        {
            SetIsoPair();
            EditorGUILayout.PropertyField(isoPair, true);
            EditorGUILayout.Separator();
            ChooseJsonConverter();
            serializedObject.ApplyModifiedProperties();
        }

        private void SetIsoPair()
        {
            //drop区域
            var rect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.Height(50));
            //"将csv拖拽至此"居中显示
            GUI.Label(rect, "将csv拖拽至此", new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 20,
            });
            var evt = Event.current;
            if (evt.type == EventType.DragUpdated && rect.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                evt.Use();
            }
            else if (evt.type == EventType.DragPerform && rect.Contains(evt.mousePosition))
            {
                DragAndDrop.AcceptDrag();
                var paths = DragAndDrop.paths;
                var dict = new Dictionary<string, string>();
                foreach (var path in paths)
                {
                    if (path.EndsWith(".csv"))
                    {
                        var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                        var lines = textAsset.text.Split('\n');
                        foreach (var line in lines)
                        {
                            var pair = line.Split(',');
                            if (pair.Length == 2)
                            {
                                dict.Add(pair[0], pair[1]);
                            }
                        }
                    }
                }
                if (dict.Count != 0)
                {
                    config.ClearIsoPairs();
                    foreach (var pair in dict)
                    {
                        config.AddIsoPair(pair.Key, pair.Value);
                    }
                    config.Refresh();
                    EditorUtility.SetDirty(config);
                    AssetDatabase.SaveAssets();
                    isoPair.serializedObject.Update();
                }
                evt.Use();
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }
        }
        private List<IDictConverter> converters;
        private int converterIndex = -1;
        private void ChooseJsonConverter()
        {
            if (converters == null)
            {
                converters = ReflectionHelper.GetAllTypeOfInterface<IDictConverter>().Select(t => Activator.CreateInstance(t) as IDictConverter).ToList();
                converterIndex = converters.FindIndex(c => c.GetType() == config.CurrentDictConverter.GetType());
                if (converterIndex == -1)
                {
                    converterIndex = 0;
                }
            }
            EditorGUI.BeginChangeCheck();
            converterIndex = EditorGUILayout.Popup("JsonConverter", converterIndex, converters.Select(c => c.GetType().Name).ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                config.currentJsonConverterName = converters[converterIndex].GetType().FullName;
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }
            
        }
    }
}

