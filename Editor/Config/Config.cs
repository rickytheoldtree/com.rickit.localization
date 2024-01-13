using System.Collections.Generic;
using System.Linq;
using RicKit.Localization.JsonConvertor;
using UnityEngine;

namespace RicKit.Localization.Config
{
    public class Config : ScriptableObject
    {
        public List<LocalizationPackage.Kvp> languageIsoPairs = new List<LocalizationPackage.Kvp>();
        private readonly Dictionary<string,string> languageIsoPairsDict = new Dictionary<string, string>();
        public List<string> SupportedLanguages { get; private set; }
        public WebDriverType webDriver;
        public enum WebDriverType
        {
            Edge,
            Chrome,
            FireFox,
        }
        public IJsonConverter CurrentJsonConverter
        {
            get
            {
                var types = Utils.ReflectionHelper.GetAllTypeOfInterface<IJsonConverter>();
                foreach (var type in types)
                {
                    if (type.FullName == currentJsonConverterName)
                    {
                        return System.Activator.CreateInstance(type) as IJsonConverter;
                    }
                }
                return new DefaultJsonConverter();
                
            }
        }
        public string currentJsonConverterName = "RicKit.Localization.JsonConverter.DefaultJsonConverter";
        public int Count => SupportedLanguages.Count;

        public void Update()
        {
            languageIsoPairsDict.Clear();
            foreach (var pair in languageIsoPairs)
            {
                languageIsoPairsDict.Add(pair.key, pair.value);
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
            return languageIsoPairs.Any(p => p.key == language);
        }
        public void AddIsoPair(string language, string isoCode)
        {
            if (!IsoPairsContains(language))
            {
                languageIsoPairs.Add(new LocalizationPackage.Kvp(language, isoCode));
            }
        }
    }

}
