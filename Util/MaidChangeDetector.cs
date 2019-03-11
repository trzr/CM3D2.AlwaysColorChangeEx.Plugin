using System;
using System.Collections.Generic;
using System.Linq;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {
    /// <summary>
    /// Description of MaidChangeDetector.
    /// </summary>
    public class MaidChangeDetector {

        public void Add(Action<Maid, MaidProp> notifier) {
            notifiers.Add(notifier);
        }

        public bool Remove(Action<Maid, MaidProp> notifier) {
            return notifiers.Remove(notifier);
        }

        public void Clear() {
            cache.Clear();
        }

        public void Detect(bool useStockMaid=false) {
            if (!notifiers.Any()) return;

            if (counter.Next()) {
                DetectMaidTarget(useStockMaid);
            }
        }

        private bool IsEnabled(Maid m) {
            return m.isActiveAndEnabled && m.Visible;// && m.body0.Face != null;
        }
        /// <summary>
        /// メイド+男を対象に検出処理を行う.
        /// </summary>
        public void DetectAllTarget(bool useStockMaid=false) {
            var chrMgr = GameMain.Instance.CharacterMgr;

            if (useStockMaid) {
                DetectTarget(chrMgr.GetStockMaid, chrMgr.GetStockMaidCount());
            } else {
                DetectTarget(chrMgr.GetMaid, chrMgr.GetMaidCount());
            }

            DetectTarget(chrMgr.GetMan, chrMgr.GetManCount());
        }

        /// <summary>
        /// メイド情報に対して検出処理を行う.
        /// </summary>
        public void DetectMaidTarget(bool useStockMaid) {
            var chrMgr = GameMain.Instance.CharacterMgr;

            if (useStockMaid) {
                DetectTarget(chrMgr.GetStockMaid, chrMgr.GetStockMaidCount());
            } else {
                DetectTarget(chrMgr.GetMaid, chrMgr.GetMaidCount());
            }
        }

        private void DetectTarget(Func<int, Maid> GetMaid, int maidCount) {
            for (var i = 0; i < maidCount; i++) {
                var maid = GetMaid(i);

                if (maid == null || maid.IsAllProcPropBusy) continue;
                if (SLOT_COUNT != maid.body0.goSlot.Count) continue; // 公式外のメイドを除外

                var maidId = maid.GetInstanceID();
                if (!IsEnabled(maid)) {
                    cache.Remove(maidId);
                    continue;
                }
                MenuCache mc;
                if (!cache.TryGetValue(maidId, out mc)) {
                    var props = PrivateAccessor.Get<MaidProp[]>(maid, "m_aryMaidProp");
                    if (props != null) {
                        mc = new MenuCache();
                        mc.SetProps(props);
                        cache[maidId] = mc;
                    }

                    continue;
                }

//                var maidProps = PrivateAccessor.Get<MaidProp[]>(maid, "m_aryMaidProp");
                for (var mpn = (int)MPN.body; mpn < mc.maidProps.Length; mpn++) {
                    var prop = mc.maidProps[mpn];
                    if (mc.menuIds[mpn] == prop.nFileNameRID) continue;

                    var mpn1 = mpn;
                    LogUtil.Debug(() => "Item changed [" + (MPN)mpn1 + "] " + mc.maidProps[mpn1].strFileName);
                    mc.menuIds[mpn] = prop.nFileNameRID;
                    foreach (var notifier in notifiers) {
                        notifier(maid, prop);
                    }
                }
            }
        }

        static readonly int SLOT_COUNT;
        static MaidChangeDetector() {
            var cnt = PrivateAccessor.Get<int>(typeof(TBody), "strSlotNameItemCnt");
            if (cnt <= 0) {
                cnt = 3;
            }
            SLOT_COUNT = TBody.m_strDefSlotName.Length/cnt;
        }

        static readonly List<MPN> list = Enum.GetValues(typeof(MPN)).Cast<MPN>().ToList();
        static readonly Dictionary<int, MenuCache> cache = new Dictionary<int, MenuCache>();

        private readonly IntervalCounter counter = new IntervalCounter(60);
        readonly List<Action<Maid, MaidProp>> notifiers = new List<Action<Maid, MaidProp>>();

        internal class MenuCache {
            internal readonly int[] menuIds = new int[list.Count];
            internal MaidProp[] maidProps;
            public void SetProps(MaidProp[] props) {
                maidProps = props;
                // 初回は現在の状態をセット
                for (var mpn = (int) MPN.body; mpn < maidProps.Length; mpn++) {
                    menuIds[mpn] = maidProps[mpn].nFileNameRID;
                }
            }
        }
    }
}
