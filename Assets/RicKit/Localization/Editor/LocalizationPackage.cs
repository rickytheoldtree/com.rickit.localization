using System;
using System.Collections.Generic;
using RicKit.Localization.Utils;
using UnityEngine;

namespace RicKit.Localization
{
    [Serializable]
    public class LocalizationPackage : ScriptableObject
    {
        public string language;
        public bool isNew;
        [Serializable]
        public struct Kvp
        {
            public string key;
            public string value;
            public Kvp(string key, string value)
            {
                this.key = key;
                this.value = value;
            }
        }
        public List<string> SupportedLanguages => LocalizationEditorUtils.GetSupportedLanguages();
        public List<Kvp> fields = new List<Kvp>();
    }
}
