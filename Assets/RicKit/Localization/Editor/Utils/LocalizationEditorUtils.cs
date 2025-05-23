using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RicKit.Localization.Package;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Kvp = RicKit.Localization.Package.LocalizationPackage.Kvp;

namespace RicKit.Localization.Utils
{
    public static class LocalizationEditorUtils
    {
        private const string DefaultRoot = "Assets\\Localization";
        private const string MainPackageRoot = DefaultRoot + "\\MainPackage";
        private const string MainPackageName = "MainEditor.asset";
        private const string NewPackageRoot = DefaultRoot + "\\NewPacakge";
        private const string NewPackageName = "NewEditor.asset";
        private const string ConfigName = "Config.asset";
        public static string langShow;
        private static string langFrom;

        [MenuItem("RicKit/Localization/打开主面板")]
        private static void GetMainEditorMenu()
        {
            var main = GetMainEditor();
            Selection.activeObject = main;
        }

        private static LocalizationPackage GetMainEditor()
        {
            var localization =
                AssetDatabase.LoadAssetAtPath<LocalizationPackage>($"{MainPackageRoot}\\{MainPackageName}");
            if (!localization)
            {
                if (!Directory.Exists(MainPackageRoot))
                {
                    Directory.CreateDirectory(MainPackageRoot);
                    AssetDatabase.Refresh();
                }

                localization = ScriptableObject.CreateInstance<LocalizationPackage>();
                AssetDatabase.CreateAsset(localization, $"{MainPackageRoot}\\{MainPackageName}");
                Debug.Log($"Create {MainPackageName} at {MainPackageRoot}");
            }
            else
            {
                Debug.Log($"{MainPackageName} 在 {MainPackageRoot}");
            }

            GetConfig();
            return localization;
        }

        public static Config.Config GetConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<Config.Config>($"{DefaultRoot}\\{ConfigName}");
            if (!config)
            {
                if (!Directory.Exists(DefaultRoot))
                {
                    Directory.CreateDirectory(DefaultRoot);
                    AssetDatabase.Refresh();
                }

                config = ScriptableObject.CreateInstance<Config.Config>();
                AssetDatabase.CreateAsset(config, $"{DefaultRoot}\\{ConfigName}");
                Debug.Log($"Create {ConfigName} at {DefaultRoot}");
            }

            return config;
        }

        public static string GetIsoCode(string language)
        {
            return GetConfig().GetIsoCode(language);
        }

        public static List<string> GetSupportedLanguages()
        {
            var config = GetConfig();
            return config.SupportedLanguages;
        }

        public static void UpdateConfig()
        {
            var config = GetConfig();
            config.Refresh();
        }

        private static LocalizationPackage GetNewEditor()
        {
            var localization =
                AssetDatabase.LoadAssetAtPath<LocalizationPackage>($"{NewPackageRoot}\\{NewPackageName}");
            if (!localization)
            {
                if (!Directory.Exists(NewPackageRoot))
                {
                    Directory.CreateDirectory(NewPackageRoot);
                    AssetDatabase.Refresh();
                }

                localization = ScriptableObject.CreateInstance<LocalizationPackage>();
                AssetDatabase.CreateAsset(localization, $"{NewPackageRoot}\\{NewPackageName}");
                Debug.Log($"Create {NewPackageName} at {NewPackageRoot}");
            }
            else
            {
                Debug.Log($"{NewPackageName} 在 {NewPackageRoot}");
            }

            localization.isNew = true;
            return localization;
        }

        public static void CreateNewJsonFolder(LocalizationPackage localization)
        {
            if (!Directory.Exists($"{GetRootPath(localization)}\\NewJson"))
            {
                Directory.CreateDirectory($"{GetRootPath(localization)}\\NewJson");
                AssetDatabase.Refresh();
            }
        }

        private static void OutputJson(string path, Dictionary<string, string> dic)
        {
            File.WriteAllText(path, GetConfig().CurrentDictConverter.Convert(dic));
            AssetDatabase.Refresh();
        }

        private static Dictionary<string, string> InputJson(string path)
        {
            return GetConfig().CurrentDictConverter.Convert(File.ReadAllText(path));
        }


        public static void AddKeyToNewPackageEnglish(string key, string value)
        {
            var localization = GetNewEditor();
            Selection.activeObject = localization;

            EditorApplication.delayCall += () =>
            {
                localization.language = GetSupportedLanguages()[0];
                localization.Load();
                localization.AddKey(key, value);
            };
        }

        public static void AddKey(this LocalizationPackage local, string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.Log("key is empty");
                return;
            }

            value ??= "";
            if (local.fields.Any(f => f.key == key))
            {
                Debug.Log($"key {key} already exist");
                return;
            }

            local.fields.Add(new Kvp(key, value));
            Debug.Log($"AddIsoPair key \"{key}\" to {local.name}: {local.language}");
            EditorUtility.SetDirty(local);
            AssetDatabase.SaveAssets();
            LocalizationPackageAbstractEditor.foldSearch = true;
            LocalizationPackageAbstractEditor.searchKey = key;
        }

        public static void Save(this LocalizationPackage local)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var l in local.fields)
            {
                if (string.IsNullOrEmpty(l.key))
                {
                    Debug.LogError("key is null");
                    return;
                }

                if (dic.ContainsKey(l.key))
                {
                    Debug.LogError($"key {l.key} is repeat");
                    return;
                }

                dic.Add(l.key, l.value);
            }

            if (!Directory.Exists($"{GetRootPath(local)}\\Json"))
            {
                Directory.CreateDirectory($"{GetRootPath(local)}\\Json");
            }

            OutputJson($"{GetRootPath(local)}\\Json\\{local.language}.json", dic);
        }

        public static void Load(this LocalizationPackage local)
        {
            langShow = local.language;
            var lang = GetSupportedLanguages()[0];
            if (!Directory.Exists($"{GetRootPath(local)}\\Json"))
            {
                Directory.CreateDirectory($"{GetRootPath(local)}\\Json");
            }

            if (File.Exists($"{GetRootPath(local)}\\Json\\{local.language}.json"))
            {
                lang = local.language;
            }

            var dic = File.Exists($"{GetRootPath(local)}\\Json\\{lang}.json")
                ? InputJson($"{GetRootPath(local)}\\Json\\{lang}.json")
                : new Dictionary<string, string>();
            local.fields.Clear();
            foreach (var d in dic)
            {
                local.fields.Add(new Kvp(d.Key, d.Value));
            }

            EditorUtility.SetDirty(local);
            AssetDatabase.SaveAssets();
        }

        public static void Sort(this LocalizationPackage local)
        {
            local.fields.Sort((a, b) => string.Compare(a.key, b.key, StringComparison.Ordinal));
            EditorUtility.SetDirty(local);
            AssetDatabase.SaveAssets();
        }

        public static void ExportForTranslation(this LocalizationPackage local)
        {
            var sb = new StringBuilder();
            foreach (var l in local.fields)
            {
                sb.AppendLine(l.value.Trim());
                sb.AppendLine("-----------------------");
            }

            if (!Directory.Exists($"{GetRootPath(local)}\\Txt"))
                Directory.CreateDirectory($"{GetRootPath(local)}\\Txt");
            File.WriteAllText($"{GetRootPath(local)}\\Txt\\{local.language}.txt", sb.ToString());
            AssetDatabase.Refresh();
        }

        public static void ExportAllSupportedForTranslation(this LocalizationPackage local)
        {
            foreach (var lang in local.SupportedLanguages)
            {
                local.language = lang;
                Load(local);
                ExportForTranslation(local);
            }
        }

        public static bool GetSplit(string text, out List<string> values)
        {
            //从第一个-开始到第一个不为-结束
            var startIndex = text.IndexOf('-');
            if (startIndex == -1)
            {
                values = null;
                Debug.LogError("找不到分隔符");
                return false;
            }

            var length = 0;
            while (text.Length > startIndex + length && text[startIndex + length] == '-')
            {
                length++;
            }

            var splitString = text.Substring(startIndex, length);
            values = text.Split(new[] { splitString }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s)).ToList();
            return true;
        }

        public static void ImportFromTranslation(this LocalizationPackage local, string targetLanguage)
        {
            if (!File.Exists($"{GetRootPath(local)}\\Txt\\{targetLanguage}.txt"))
            {
                Debug.LogError($"文件 {targetLanguage}.txt 不存在");
                return;
            }

            var str = File.ReadAllText($"{GetRootPath(local)}\\Txt\\{targetLanguage}.txt");
            if (!GetSplit(str, out var values))
            {
                return;
            }

            var englishJson = InputJson($"{GetRootPath(local)}\\Json\\English.json");
            var dic = new Dictionary<string, string>();
            var keys = englishJson.Keys.ToList();
            if (keys.Count != values.Count)
            {
                Debug.LogError("翻译后数量与原文本数量不一致");
                return;
            }

            for (var i = 0; i < keys.Count; i++)
            {
                dic.Add(keys[i], values[i].Trim());
            }

            OutputJson($"{GetRootPath(local)}\\Json\\{targetLanguage}.json", dic);
            Load(local);
        }

        public static void ImportAllSupportedFromTranslation(this LocalizationPackage local)
        {
            foreach (var lang in local.SupportedLanguages)
            {
                ImportFromTranslation(local, lang);
            }
        }

        public static Dictionary<int, Kvp> Search(this LocalizationPackage local, string searchKey)
        {
            var result = new Dictionary<int, Kvp>();
            if (string.IsNullOrEmpty(searchKey))
            {
                //如果searchKey为空，就返回所有的localization
                for (var i = 0; i < local.fields.Count; i++)
                {
                    var kvp = local.fields[i];
                    result.Add(i, kvp);
                }

                return result;
            }

            //如果target的localization中的key或者value包含searchKey，就加入到result中
            for (var i = 0; i < local.fields.Count; i++)
            {
                var kvp = local.fields[i];
                if ((kvp.key != null && kvp.key.Contains(searchKey)) ||
                    (kvp.value != null && kvp.value.Contains(searchKey)))
                {
                    result.Add(i, kvp);
                }
            }

            return result;
        }

        private static string GetRootPath(Object local)
        {
            //获取LocalizationEditor.asset的路径
            var path = AssetDatabase.GetAssetPath(local);
            path = Path.GetDirectoryName(path);
            return path;
        }

        /// <summary>
        /// 合并NewJson文件夹中的json文件到Json文件夹中，如果key相同，value会被覆盖
        /// </summary>
        /// <param name="local"></param>
        public static void MergeJsons(this LocalizationPackage local)
        {
            foreach (var lang in local.SupportedLanguages)
            {
                if (!Directory.Exists($"{GetRootPath(local)}\\NewJson"))
                {
                    Directory.CreateDirectory($"{GetRootPath(local)}\\NewJson");
                    AssetDatabase.Refresh();
                    Debug.Log("请将需要合并的json文件放入NewJson文件夹中");
                    return;
                }

                if (!File.Exists($"{GetRootPath(local)}\\NewJson\\{lang}.json"))
                {
                    Debug.Log($"文件{lang}.json不存在");
                    continue;
                }

                var dic = InputJson($"{GetRootPath(local)}\\NewJson\\{lang}.json");
                var dicOld = InputJson($"{GetRootPath(local)}\\Json\\{lang}.json");
                foreach (var d in dic)
                {
                    dicOld[d.Key] = d.Value;
                }

                OutputJson($"{GetRootPath(local)}\\Json\\{lang}.json", dicOld);
            }

            AssetDatabase.Refresh();
        }

        public static void MergeJsonsIncreaseOnly(this LocalizationPackage local)
        {
            foreach (var lang in local.SupportedLanguages)
            {
                if (!Directory.Exists($"{GetRootPath(local)}\\NewJson"))
                {
                    Directory.CreateDirectory($"{GetRootPath(local)}\\NewJson");
                    AssetDatabase.Refresh();
                    Debug.Log("请将需要合并的json文件放入NewJson文件夹中");
                    return;
                }

                if (!File.Exists($"{GetRootPath(local)}\\NewJson\\{lang}.json"))
                {
                    Debug.Log($"文件{lang}.json不存在");
                    continue;
                }

                var dic = InputJson($"{GetRootPath(local)}\\NewJson\\{lang}.json");
                var dicOld = InputJson($"{GetRootPath(local)}\\Json\\{lang}.json");
                foreach (var d in dic)
                {
                    if (!dicOld.ContainsKey(d.Key))
                    {
                        dicOld.Add(d.Key, d.Value);
                    }
                    else
                    {
                        Debug.Log($"跳过{d.Key}，因为已经存在");
                    }
                }

                OutputJson($"{GetRootPath(local)}\\Json\\{lang}.json", dicOld);
            }

            AssetDatabase.Refresh();
        }

        public static void OpenNewPackage(this LocalizationPackage local)
        {
            var newEditor = GetNewEditor();
            // 选中NewPackage.asset
            Selection.activeObject = newEditor;
        }

        public static void MoveNewPackageJson2MainPackageNewJson(this LocalizationPackage newEditor)
        {
            var mainEditor = GetMainEditor();
            //判断mainPackage的NewJson文件夹是否为空
            if (!Directory.Exists($"{GetRootPath(mainEditor)}\\NewJson"))
            {
                Directory.CreateDirectory($"{GetRootPath(mainEditor)}\\NewJson");
            }

            //判断NewJson里面是否有文件
            if (Directory.GetFiles($"{GetRootPath(mainEditor)}\\NewJson").Length != 0)
            {
                Debug.LogWarning("【移动取消】MainPackage\\NewJson文件夹内有东西，请自行确认后删除");
                return;
            }

            //将NewPackage的Json文件夹复制到MainPackage的NewJson文件夹
            foreach (var lang in newEditor.SupportedLanguages)
            {
                if (!File.Exists($"{GetRootPath(newEditor)}\\Json\\{lang}.json"))
                {
                    Debug.Log($"文件{lang}.json不存在");
                    continue;
                }

                File.Copy($"{GetRootPath(newEditor)}\\Json\\{lang}.json",
                    $"{GetRootPath(mainEditor)}\\NewJson\\{lang}.json");
            }

            AssetDatabase.Refresh();
        }

        #region 生成文本范围

        public static void GenerateAllText(this LocalizationPackage local)
        {
            var sb = new StringBuilder();
            sb.Append("�1234567890!@#$%^&*()_+-=<>?,./;:'\"{}[]|\\~`");
            sb.Append("，。、；‘’：“”【】《》？！￥…（）—");
            foreach (var lang in local.SupportedLanguages)
            {
                var json = InputJson($"{GetRootPath(local)}\\Json\\{lang}.json");
                foreach (var j in json)
                {
                    sb.Append(j.Value);
                }
            }

            File.WriteAllText($"{GetRootPath(local)}\\AllText.txt", sb.ToString());
            AssetDatabase.Refresh();
        }

        [MenuItem("RicKit/Localization/预设提取")]
        public static void PresetExtract()
        {
            GetAllChinese();
            GetAllKorean();
            GetAllJapanese();
            GenerateRussian();
            GenerateLatin();
            GenerateHindi();
        }

        [MenuItem("RicKit/Localization/提取中文")]
        public static void GetAllChinese()
        {
            var sb = new StringBuilder();
            var selects = Selection.objects;
            foreach (var select in selects)
            {
                if (select is TextAsset textAsset)
                {
                    sb.Append(textAsset.text);
                }
            }

            var text = sb.ToString();
            var result = new StringBuilder();
            //提取中文以及标点符号
            foreach (var c in text)
            {
                if (c >= 0x4e00 && c <= 0x9fa5)
                {
                    result.Append(c);
                }

                if (c >= 0x3000 && c <= 0x303f)
                {
                    result.Append(c);
                }

                if (c >= 0xff00 && c <= 0xffef)
                {
                    result.Append(c);
                }
            }

            File.WriteAllText($"{Application.dataPath}\\Chinese.txt", result.ToString());
            AssetDatabase.Refresh();
        }

        [MenuItem("RicKit/Localization/提取韩语")]
        public static void GetAllKorean()
        {
            var sb = new StringBuilder();
            var selects = Selection.objects;
            foreach (var select in selects)
            {
                if (select is TextAsset textAsset)
                {
                    sb.Append(textAsset.text);
                }
            }

            var text = sb.ToString();
            var result = new StringBuilder();
            //提取韩文以及标点符号
            foreach (var c in text)
            {
                if (c >= 0xac00 && c <= 0xd7a3)
                {
                    result.Append(c);
                }

                if (c >= 0x3000 && c <= 0x303f)
                {
                    result.Append(c);
                }

                if (c >= 0xff00 && c <= 0xffef)
                {
                    result.Append(c);
                }
            }

            File.WriteAllText($"{Application.dataPath}\\Korean.txt", result.ToString());
            AssetDatabase.Refresh();
        }


        [MenuItem("RicKit/Localization/提取日语")]
        public static void GetAllJapanese()
        {
            var sb = new StringBuilder();
            var selects = Selection.objects;
            foreach (var select in selects)
            {
                if (select is TextAsset textAsset)
                {
                    sb.Append(textAsset.text);
                }
            }

            var text = sb.ToString();
            var result = new StringBuilder();
            //提取日文以及标点符号
            foreach (var c in text)
            {
                if (c >= 0x3040 && c <= 0x309f)
                {
                    result.Append(c);
                }

                if (c >= 0x30a0 && c <= 0x30ff)
                {
                    result.Append(c);
                }

                if (c >= 0x3000 && c <= 0x303f)
                {
                    result.Append(c);
                }

                if (c >= 0xff00 && c <= 0xffef)
                {
                    result.Append(c);
                }
            }

            File.WriteAllText($"{Application.dataPath}\\Japanese.txt", result.ToString());
            AssetDatabase.Refresh();
        }

        //俄语完整列表
        [MenuItem("RicKit/Localization/生成俄语表")]
        public static void GenerateRussian()
        {
            var sb = new StringBuilder();
            for (var i = 0x0410; i <= 0x045f; i++)
            {
                sb.Append((char)i);
            }

            File.WriteAllText($"{Application.dataPath}\\Russian.txt", sb.ToString());
        }

        //生成通用拉丁语系
        [MenuItem("RicKit/Localization/生成拉丁语系")]
        public static void GenerateLatin()
        {
            var sb = new StringBuilder();
            for (var i = 0x0020; i <= 0x007f; i++)
            {
                sb.Append((char)i);
            }

            for (int i = 0x0080; i <= 0x00ff; i++)
            {
                sb.Append((char)i);
            }

            for (int i = 0x0100; i <= 0x017f; i++)
            {
                sb.Append((char)i);
            }

            for (int i = 0x0180; i <= 0x024f; i++)
            {
                sb.Append((char)i);
            }

            //ipa
            for (int i = 0x0250; i <= 0x02af; i++)
            {
                sb.Append((char)i);
            }

            //拉丁文拓展
            for (int i = 0x1e00; i <= 0x1eff; i++)
            {
                sb.Append((char)i);
            }

            //一般标点
            for (int i = 0x2000; i <= 0x206f; i++)
            {
                sb.Append((char)i);
            }

            File.WriteAllText($"{Application.dataPath}\\Latin.txt", sb.ToString());
            AssetDatabase.Refresh();
        }

        
        //生成印地语
        [MenuItem("RicKit/Localization/生成印地语")]
        public static void GenerateHindi()
        {
            var sb = new StringBuilder();
            for (var i = 0x0900; i <= 0x097f; i++)
            {
                sb.Append((char)i);
            }

            File.WriteAllText($"{Application.dataPath}\\Hindi.txt", sb.ToString());
        }
        #endregion
    }
}