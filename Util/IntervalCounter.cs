
using System;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {
    /// <summary>
    /// 指定した間隔でtrueを返すカウンタクラス
    /// 0 値指定の場合は、常にtrueを返し、
    /// 負値指定の場合は、常にfalseを返す.
    /// </summary>
    public class IntervalCounter {
        public IntervalCounter(int interval0) {
            interval = interval0;
            if (interval < 0) {
                Next = () => false; 
            } else if (interval == 0) {
                Next = () => true;
            } else {
                Next = () => {
                    if (nextCount++ <= interval) return false;
                    nextCount = 0;
                    return true;
                };
            }
        }
        private int interval;
        private int nextCount;
        public readonly Func<bool> Next;

        public void Reset() {
            nextCount = 0;
        }
    }
}
