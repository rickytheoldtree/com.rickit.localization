using System.Collections.Generic;

namespace RicKit.Localization.JsonConvertor
{
    public interface IJsonConverter
    {
        Dictionary<string,string> Convert(string json);
        string Convert(Dictionary<string,string> dict);
    }
}