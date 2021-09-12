using System;
using System.Collections.Generic;

namespace Pomelo.Data.Serialize.Dynamic
{
    public static class LambdaRegistery
    {
        private static Dictionary<string, Func<object, object>> getterDic = new Dictionary<string, Func<object, object>>();
        private static Dictionary<string, Action<object, object>> setterDic = new Dictionary<string, Action<object, object>>();

        public static Func<object, object> FindGetter(string name)
        {
            return getterDic.ContainsKey(name) ? getterDic[name] : null;
        }

        public static Action<object, object> FindSetter(string name)
        {
            return setterDic.ContainsKey(name) ? setterDic[name] : null;
        }

        public static void RegisterGetter(string name, Func<object, object> getter)
        {
            getterDic[name] = getter;
        }

        public static void RegisterSetter(string name, Action<object, object> setter)
        {
            setterDic[name] = setter;
        }

        public static T Cast<T>(object obj) => (T)obj;
    }
}
