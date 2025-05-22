using System;
using System.Collections.Generic;

namespace RicKit.Localization.Utils
{
    public static class ReflectionHelper
    {
        public static Type[] GetAllTypeOfInterface<T>() where T : class
        {
            var types = AppDomain.CurrentDomain.GetAssemblies();
            var list = new List<Type>();
            foreach (var assembly in types)
            {
                var ts = assembly.GetTypes();
                foreach (var t in ts)
                {
                    if (t.GetInterface(typeof(T).Name) != null)
                    {
                        list.Add(t);
                    }
                }
            }
            return list.ToArray();
        }
    }
}