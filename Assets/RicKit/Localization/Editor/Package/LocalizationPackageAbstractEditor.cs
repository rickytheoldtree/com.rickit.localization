using System.Collections.Generic;
using System.Linq;
using RicKit.Localization.Utils;
using UnityEditor;
using UnityEngine;
using Kvp = RicKit.Localization.Package.LocalizationPackage.Kvp;
using Object = UnityEngine.Object;

namespace RicKit.Localization.Package
{
    public abstract class LocalizationPackageAbstractEditor : UnityEditor.Editor
    {
        public static string searchKey;
        private string lastSearchKey, newKey, newValue;
        private Dictionary<int, Kvp> searchResult = new Dictionary<int, Kvp>();
        private bool foldAdd, foldTxtImport, foldMultiAdd, foldOthers, foldTranslation, forceUpdate;
        public static bool foldSearch;
        private static bool foldSupportedLanguages;
        protected LocalizationPackage package;
        private Vector2 scrollPos;

        private void OnEnable()
        {
            package = (LocalizationPackage)target;
            LocalizationEditorUtils.UpdateConfig();
        }

        private void ShowSupportedLanguages()
        {
            var supportedLanguages = package.SupportedLanguages;
            var lang = package.language;
            var richTextStyle = new GUIStyle(EditorStyles.label)
            {
                richText = true
            };
            EditorGUI.indentLevel++;
            foldSupportedLanguages = EditorGUILayout.Foldout(foldSupportedLanguages, "Supported Languages");
            if (foldSupportedLanguages)
            {
                EditorGUI.indentLevel++;
                foreach (var language in supportedLanguages)
                {
                    EditorGUILayout.BeginHorizontal();
                    var str = language;
                    if (language == lang)
                        str = $"<color=yellow>{str}</color>";
                    EditorGUILayout.LabelField(str, richTextStyle);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        private int langIndex;
        public override void OnInspectorGUI()
        {
            if(package.SupportedLanguages == null || package.SupportedLanguages.Count == 0)
            {
                EditorGUILayout.HelpBox("请在Config中设置支持的语言", MessageType.Warning);
                return;
            }
            
            #region 常规功能
            langIndex = EditorGUILayout.Popup("语言：", langIndex, package.SupportedLanguages.ToArray());
            package.language = package.SupportedLanguages.ElementAt(langIndex);
            ShowSupportedLanguages();
            //获取target的父文件夹
            var path = AssetDatabase.GetAssetPath(target);
            path = path.Substring(0, path.LastIndexOf('/'));
            path = $"{path}/Json/{package.language}.json";
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextField("路径：", path);
                }
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (obj && GUILayout.Button("打开文件", GUILayout.Width(100)))
                {
                    //打开Json文件
                    AssetDatabase.OpenAsset(obj);
                }
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("保存到Json"))
            {
                package.Save();
            }

            if (GUILayout.Button("从Json加载") ||
                LocalizationEditorUtils.langShow != package.language)
            {
                forceUpdate = true;
                package.Load();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("生成AllText.txt"))
            {
                package.GenerateAllText();
            }
            if (package.isNew && GUILayout.Button("将NewPackage/Json复制到MainPackage/NewJson"))
            {
                package.MoveNewPackageJson2MainPackageNewJson();
            }
            EditorGUILayout.Separator();

            #endregion

            #region 搜索与修改

            foldSearch = EditorGUILayout.Foldout(foldSearch, "搜索与修改");
            if (foldSearch)
            {
                searchKey =
                    EditorGUILayout.TextField("搜索：", searchKey);
                if (searchKey != lastSearchKey || forceUpdate)
                {
                    searchResult = package.Search(searchKey);
                    lastSearchKey = searchKey;
                    forceUpdate = false;
                }

                if (searchResult.Count > 0)
                {
                    EditorGUILayout.HelpBox("\"删除\"会删除缓存中的该键值对，\"添加到New\"会将key添加到NewPackage，\"保存\"会保存到缓存",
                        MessageType.Info);
                    var localization = package.fields;
                    var richTextStyle = new GUIStyle(EditorStyles.label)
                    {
                        richText = true
                    };
                    //显示数量
                    EditorGUILayout.LabelField($"搜索到{searchResult.Count}个结果");
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        for (var i = 0; i < searchResult.Count; i++)
                        {
                            var result = searchResult.ElementAt(i);
                            var kvp = result.Value;
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                var str = kvp.key;
                                if (!string.IsNullOrEmpty(searchKey))
                                    str = str.Replace(searchKey, $"<color=yellow>{searchKey}</color>");
                                EditorGUILayout.LabelField(str, richTextStyle);
                                var v = EditorGUILayout.TextField(kvp.value);
                                kvp.value = v;
                                searchResult[result.Key] = kvp;
                                if (GUILayout.Button("删除"))
                                {
                                    localization.Remove(kvp);
                                    searchResult.Remove(result.Key);
                                    EditorUtility.SetDirty(target);
                                    AssetDatabase.SaveAssets();
                                    forceUpdate = true;
                                    break;
                                }

                                if (GUILayout.Button("添加到New"))
                                {
                                    LocalizationEditorUtils.AddKeyToNewPackageEnglish(kvp.key, kvp.value);
                                    break;
                                }

                                if (GUILayout.Button("保存"))
                                {
                                    localization[result.Key] = kvp;
                                    EditorUtility.SetDirty(target);
                                    AssetDatabase.SaveAssets();
                                    // 取消选中textfield
                                    GUI.FocusControl(null);
                                    forceUpdate = true;
                                    break;
                                }
                            }
                        }
                    }

                    EditorGUILayout.EndScrollView();
                }
            }

            #endregion

            #region 加Key

            foldAdd = EditorGUILayout.Foldout(foldAdd, "加key");
            if (foldAdd)
            {
                EditorGUI.indentLevel++;
                newKey = EditorGUILayout.TextField("Key:", newKey);
                newValue = EditorGUILayout.TextField("Value:", newValue);
                if (GUILayout.Button("加key"))
                {
                    forceUpdate = true;
                    package.AddKey(newKey, newValue);
                    serializedObject.Update();
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.HelpBox("只会加在英文里", MessageType.Info);
            }

            #endregion

            #region txt Json互转(Supported Languages)

            foldTxtImport = EditorGUILayout.Foldout(foldTxtImport, "Txt-Json互转");
            if (foldTxtImport)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("转txt"))
                    package.ExportForTranslation();
                if (GUILayout.Button("批量转txt"))
                    package.ExportAllSupportedForTranslation();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("转Json"))
                    package.ImportFromTranslation(package.language);
                if (GUILayout.Button("批量转Json"))
                    package.ImportAllSupportedFromTranslation();
                EditorGUILayout.EndHorizontal();
            }

            #endregion

            #region 批量增改Key工具

            foldMultiAdd = EditorGUILayout.Foldout(foldMultiAdd, "批量增改Key工具");
            if (foldMultiAdd)
            {
                LocalizationEditorUtils.CreateNewJsonFolder(package);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("打开补充包生成工具");
                if (GUILayout.Button("打开"))
                    package.OpenNewPackage();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("批量导入Json");
                if (GUILayout.Button("相同key取新值"))
                    package.MergeJsons();
                if (GUILayout.Button("相同key取旧值"))
                    package.MergeJsonsIncreaseOnly();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox("将NewJson文件夹下的Json文件和Json文件夹中的文件合并。输出到Json文件夹，所以记得使用前备份Json文件夹",
                    MessageType.Info);
            }

            #endregion
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}