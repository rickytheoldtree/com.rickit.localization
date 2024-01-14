using System;
using System.Collections.Generic;
using System.Linq;
using RicKit.Localization.DictConvertor;
using UnityEngine;

namespace RicKit.Localization.Config
{
    public class Config : ScriptableObject
    {
        public List<LanguageIsoPair> languageIsoPairs = new List<LanguageIsoPair>();
        private readonly Dictionary<string,string> languageIsoPairsDict = new Dictionary<string, string>();
        public List<string> SupportedLanguages { get; private set; }
        public WebDriverType webDriver;
        public bool customDriverPath;
        public string driverPath;
        public enum WebDriverType
        {
            Edge,
            Chrome,
            FireFox,
        }
        [Serializable]
        public struct LanguageIsoPair
        {
            public string language;
            public string isoCode;
            public LanguageIsoPair(string language, string isoCode)
            {
                this.language = language;
                this.isoCode = isoCode;
            }
        }
        public IDictConverter CurrentDictConverter
        {
            get
            {
                var types = Utils.ReflectionHelper.GetAllTypeOfInterface<IDictConverter>();
                foreach (var type in types)
                {
                    if (type.FullName == currentJsonConverterName)
                    {
                        return Activator.CreateInstance(type) as IDictConverter;
                    }
                }
                return new DefaultDictConverter();
                
            }
        }
        public string currentJsonConverterName = "RicKit.Localization.JsonConverter.DefaultDictConverter";

        public void Update()
        {
            languageIsoPairsDict.Clear();
            foreach (var pair in languageIsoPairs)
            {
                languageIsoPairsDict.Add(pair.language, pair.isoCode);
            }
            SupportedLanguages = languageIsoPairsDict.Keys.ToList();
        }
        public string GetIsoCode(string language)
        {
            if (languageIsoPairsDict.TryGetValue(language, out var code))
            {
                return code;
            }
            return null;
        }
        public void ClearIsoPairs()
        {
            languageIsoPairs.Clear();
        }
        private bool IsoPairsContains(string language)
        {
            return languageIsoPairs.Any(p => p.language == language);
        }
        public void AddIsoPair(string language, string isoCode)
        {
            if (!IsoPairsContains(language))
            {
                languageIsoPairs.Add(new LanguageIsoPair(language, isoCode));
            }
        }
    }

}
