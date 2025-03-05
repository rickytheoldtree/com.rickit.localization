using System.Collections.Generic;

namespace RicKit.Localization.DictConvertor
{
    public interface IDictConverter
    {
        Dictionary<string,string> Convert(string json);
        string Convert(Dictionary<string,string> dict);
    }
}