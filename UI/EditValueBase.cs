
using System;
using System.Globalization;

namespace CM3D2.AlwaysColorChangeEx.Plugin.UI {
    public abstract class EditValueBase<T> where T: IComparable, IFormattable {
        public T val;
        public bool isSync;
        public string editVal;
        public readonly EditRange<T> range;
        
        protected EditValueBase(T val1, EditRange<T> attr) {
            range = attr;
            Set( val1 );
        }

        protected EditValueBase(T val1, string format, T min, T max) {
            range = new EditRange<T>(format, min, max);
            Set( val1 );
        }

        public void Set(T val1) {
            val = val1;
            editVal = val1.ToString(range.format, NumberFormatInfo.CurrentInfo);
            isSync = true;
        }

        public void SetWithCheck(T val1) {
            if (val1.CompareTo(range.editMin) == -1) val = range.editMin;
            else if (val1.CompareTo(range.editMax) == 1) val = range.editMax;
            else val = val1;
            editVal = val.ToString(range.format, NumberFormatInfo.CurrentInfo);
            isSync = true;
        }

        public void Set(string editVal1) {
            editVal = editVal1;

            T v;
            isSync = false;
            if (TryParse(editVal1, out v)) {
                if (v.CompareTo(range.editMin) == -1)     v = range.editMin;
                else if (v.CompareTo(range.editMax) == 1) v = range.editMax;
                else isSync = true;
                val = v;
            }
        }
        protected abstract bool TryParse(string edit, out T v);
    }
}
