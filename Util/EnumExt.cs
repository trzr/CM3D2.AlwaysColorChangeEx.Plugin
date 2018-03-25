using System;
using System.Collections.Generic;
using System.Linq;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {
    /// <summary>
    /// Description of EnumExt.
    /// </summary>
    public static class EnumExt<T> where T : struct, IConvertible {
        static readonly List<T> Values = Enum.GetValues(typeof(T)).Cast<T>().ToList();
        
        public static void Exec(Action<T> action) {
            foreach (var val in Values) {
                action(val);
            }
        }   
    }
}
