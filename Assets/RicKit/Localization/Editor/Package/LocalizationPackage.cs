using System;
using System.Collections.Generic;
using RicKit.Localization.Utils;
using UnityEngine;

namespace RicKit.Localization.Package
{
    [Serializable]
    public class LocalizationPackage : ScriptableObject
    {
        public string language;
        public bool isNew;
        [Serializable]
        public struct Kvp : IEquatable<Kvp>
        {
            public string key;
            public string value;
            public Kvp(string key, string value)
            {
                this.key = key;
                this.value = value;
            }

            public bool Equals(Kvp other)
            {
                return key == other.key && value == other.value;
            }

            public override bool Equals(object obj)
            {
                return obj is Kvp other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((key != null ? key.GetHashCode() : 0) * 397) ^ (value != null ? value.GetHashCode() : 0);
                }
            }
        }
        public List<string> SupportedLanguages => LocalizationEditorUtils.GetSupportedLanguages();
        public List<Kvp> fields = new List<Kvp>();
    }
}
