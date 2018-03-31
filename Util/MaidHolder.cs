using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {
    /// <summary>
    /// 編集中のメイド情報を扱うデータホルダクラス
    /// </summary>
    public sealed class MaidHolder {
        private static readonly MaidHolder INSTANCE = new MaidHolder();
        public static MaidHolder Instance {
            get { return INSTANCE;  }
        }
        private readonly int _slotCount;

        private readonly Material[] _emptyList = new Material[0];
        
        public string MaidName { get; private set; }
        // 選択中のメイド
        public Maid CurrentMaid { get; set; }
        // 選択中のスロット
        public SlotInfo CurrentSlot { get; set; }

        private MaidHolder() {
            MaidName = string.Empty;
            var cnt = PrivateAccessor.Get<int>(typeof(TBody),"strSlotNameItemCnt");
            if (cnt <= 0) {
                cnt = 3;
            }
            _slotCount = TBody.m_strDefSlotName.Length/cnt;
        }

        public bool Applicable() {
            return (CurrentMaid != null) && !CurrentMaid.boAllProcPropBUSY;
        }

        public bool CurrentActivated() {
            return CurrentMaid != null && CurrentMaid.isActiveAndEnabled;
        }

        // enabledであればデータ参照可
        public bool CurrentEnabled() {
            return CurrentMaid != null && CurrentMaid.enabled;
        }

        public bool isOfficial;
        public bool CheckOfficial(Maid maid) {
            //LogUtil.Debug("slotCount:", SLOT_COUNT, ", maid. count=", maid.body0.goSlot.Count);
            return (maid.body0.goSlot.Count == _slotCount);
        }
        /// <summary>
        /// メイドを更新する.
        /// 名前が未指定の場合は、statusのlast_nameとfirst_nameから生成する.
        /// 
        /// </summary>
        /// <param name="maid0">メイド</param>
        /// <param name="name">メイドの名前</param>
        /// <param name="act"></param>
        /// <returns>別のメイドに変更された場合、trueを返す</returns>
        public bool UpdateMaid(Maid maid0, string name, Action act) {
            if (maid0 == null) {
                // メイドリストから最初に有効なメイドを取得
                var count = GameMain.Instance.CharacterMgr.GetMaidCount();
                for (var i=0; i< count; i++) {
                    var m = GameMain.Instance.CharacterMgr.GetMaid(i);
                    if (m == null || !m.enabled) continue;

                    maid0 = m;
                    break;
                }
            }

            if (CurrentMaid == maid0) return false;
            CurrentMaid = maid0;
            if (CurrentMaid != null) {
                MaidName = name?? MaidHelper.GetName(CurrentMaid);

                isOfficial = CheckOfficial(CurrentMaid);
            } else {
                MaidName = "(not selected)";
            }
            LogUtil.Debug("maid changed.", MaidName);

            act();

            return true;
        }
        public bool UpdateMaid(Action act) {
            return UpdateMaid(null, null, act);
        }

        public string GetCurrentMenuFile() {
            if (CurrentMaid == null) return null;

            var prop = CurrentMaid.GetProp(CurrentSlot.mpn);
            if (prop != null) return prop.strFileName;

            LogUtil.Log("maid prop is null", CurrentSlot.mpn);
            return null;
        }

        public int GetCurrentMenuFileID() {
            if (CurrentMaid == null) return 0;

            var prop = CurrentMaid.GetProp(CurrentSlot.mpn);
            if (prop != null) return prop.nFileNameRID;

            LogUtil.Log("maid prop is null", CurrentSlot.mpn);
            return 0;
        }

        /// <summary>選択中のスロットを取得する</summary>
        /// <returns>スロット</returns>
        public TBodySkin GetCurrentSlot() {
            return CurrentMaid.body0.GetSlot((int)CurrentSlot.Id);
        }

        public Material[] GetMaterials() {
            return GetMaterials(CurrentSlot.Id);
        }

        public Material[] GetMaterials(SlotInfo slot) {
            return GetMaterials(slot.Id);
        }

        public Material[] GetMaterials(TBody.SlotID slotID) {
            var slotNo = (int)slotID;
            return slotNo >= CurrentMaid.body0.goSlot.Count
                ? _emptyList
                : GetMaterials(CurrentMaid.body0.GetSlot(slotNo));
        }

        public Material[] GetMaterials(string slotName) {
            return GetMaterials(CurrentMaid.body0.GetSlot(slotName));
        }

        public Renderer GetRenderer(TBody.SlotID slotID) {
            var slotNo = (int)slotID;
            return slotNo >= CurrentMaid.body0.goSlot.Count
                ? null
                : GetRenderer(CurrentMaid.body0.GetSlot(slotNo));
        }

        public Material[] GetMaterials(TBodySkin slot) {
            var renderer = GetRenderer(slot);
            return renderer == null ? _emptyList : renderer.materials;
        }

        public Material GetMaterial(int matNo) {
            return GetMaterial(CurrentMaid.body0.GetSlot((int)CurrentSlot.Id), matNo);
        }

        public Material GetMaterial(TBodySkin slot, int matNo) {
            if (slot.obj == null) return null;

            var r = GetRenderer(slot.obj, matNo);
            return r != null ? r.materials[matNo] : null;
        }

        public Renderer GetRenderer(TBodySkin slot) {
            var gobj = slot.obj;
            return gobj == null ? null : GetRenderer(gobj, 0);
        }

        private Renderer GetRenderer(GameObject gobj, int matNo) {
            // trueにするのはマスク等で非表示のアイテムの情報も扱うため
            var children = gobj.transform.GetComponentsInChildren<Renderer>(true);
            return children.FirstOrDefault(r => r.material != null && r.materials.Length > matNo && r.material.shader != null);
        }

        /// <summary>
        /// スロット名とマテリアル番号を指定して、マテリアルオブジェクトを取得する
        /// </summary>
        /// 
        /// <param name="slotName">スロット名(列挙型の名前)</param>
        /// <param name="matNo">マテリアル番号</param>
        /// <returns>マテリアル　ただし、見つからない場合はnullを返す</returns>
        public Material GetMaterial(string slotName, int matNo) {
            var slot = CurrentMaid.body0.GetSlot(slotName);
            if (slot.obj == null) return null;

            var r = GetRenderer(slot.obj, matNo);
            return r != null ? r.materials[matNo] : null;
        }

        public void SetDelNodes(PresetData preset, bool bApply) {
            SetDelNodes(CurrentMaid, preset.delNodes, bApply);
        }

        public void SetDelNodes(Maid maid, PresetData preset, bool bApply) {
            // if (preset.delNodes == null) return;
            SetDelNodes(maid, preset.delNodes, bApply);
        }

        public void SetDelNodes(Dictionary<string, bool> dDelNodes, bool bApply) {
            SetDelNodes(CurrentMaid, dDelNodes, bApply);
        }

        public void SetDelNodes(Maid maid, Dictionary<string, bool> dDelNodes, bool bApply) {
            if (!dDelNodes.Any()) return;

            foreach (var entry in dDelNodes) {
                var nodeItem = ACConstants.NodeNames[entry.Key];
                if (entry.Value) { // 
                    foreach (var slot in maid.body0.goSlot) {
                        if (slot.obj == null) continue;
                        // 強制表示
                        if (slot.m_dicDelNodeBody.ContainsKey(entry.Key)) {
                            slot.m_dicDelNodeBody[entry.Key] = true; // entry.Value
                        }
                    }
                } else {
                    var hasSet = false;
                    foreach (var slotId in nodeItem.slots) {
                        var slot = maid.body0.GetSlot((int)slotId);
                        if (slot.obj == null) continue;

                        // slot.boVisible = true;
                        if (slot.m_dicDelNodeBody.ContainsKey(entry.Key)) {
                            slot.m_dicDelNodeBody[entry.Key] = false; // entry.Value
                        }
                        hasSet = true;
                        break;
                    }

                    if (hasSet) continue;
                    {
                        // もしどこにもない場合はbodyにセット
                        var slot = maid.body0.GetSlot((int)TBody.SlotID.body);
                        if (slot.obj == null) continue;

                        //slot.boVisible = true;
                        if (slot.m_dicDelNodeBody.ContainsKey(entry.Key)) {
                            slot.m_dicDelNodeBody[entry.Key] = false; // entry.Value
                        }
                    }
                }
            }
            if (bApply) FixFlag();
        }

        public void SetDelNodesForce(Dictionary<string, bool> dDelNodes, bool bApply) {
            SetDelNodesForce(CurrentMaid, dDelNodes, bApply);
        }

        // 強引に全スロットに対してノード非表示を適用
        public void SetDelNodesForce(Maid maid, Dictionary<string, bool> dDelNodes, bool bApply) {
            if (!dDelNodes.Any()) return;

            foreach (var slot in maid.body0.goSlot) {
                if (slot.obj == null) continue;
                slot.boVisible = true;
                foreach (var entry in dDelNodes) {
                    if (slot.m_dicDelNodeBody.ContainsKey(entry.Key)) {
                        slot.m_dicDelNodeBody[entry.Key] = entry.Value;
                    }
                }
            }
            if (bApply) FixFlag();
        }

        // ReSharper disable once UnusedMember.Local
        private Hashtable GetMaskTable() {
            return CurrentMaid == null 
                ? null
                : PrivateAccessor.Get<Hashtable>(CurrentMaid.body0, "m_hFoceHide");
        }

        /// <summary>
        /// スロットの可視性を設定する.
        /// temporaryにtrueを設定すると可視性のみ設定するがfalseの場合は、スロットへのマスク設定を行う
        /// </summary>
        /// <param name="maid">操作対象メイド</param>
        /// <param name="maskDic">マスク設定Dic</param>
        /// <param name="temporary">一時適用フラグ</param>
        public void SetSlotVisibles(Maid maid, Dictionary<TBody.SlotID, MaskInfo> maskDic, bool temporary) {
//            Hashtable m_foceHide = GetMaskTable();
//            if (m_foceHide == null) {
//                LogUtil.Error("cannot take MaskTable");
//                return;
//            }
            
            foreach (var pair in maskDic) {
                var maskInfo = pair.Value;

                // 未読み込みの場合はスキップ
                if (maskInfo.state == SlotState.NotLoaded) continue;

                maskInfo.slot.boVisible = maskInfo.value;

                if (temporary) continue;

                var slot = maid.body0.GetSlot((int)pair.Key);
                if (!maskInfo.value) {
                    slot.listMaskSlot.Add((int)pair.Key);
                } else {
                    // 全スロットから削除する
                    //slot.listMaskSlot.Remove((int)pair.Key);
                    foreach (var tBodySkin in maid.body0.goSlot) {
                        tBodySkin.listMaskSlot.Remove((int)pair.Key);
                    }
                }
                // 下記の情報はGetMaskで取得されるフラグに関するものであるが、
                // maskItemとは別で、下着モード、ヌードモードなどの指定で使われるフラグ
                //if (!tmp) m_foceHide[pair.Key] = !maskInfo.value;
            }
        }

        public void SetMaskSlots(PresetData preset) {
            SetMaskSlots(CurrentMaid, preset.slots);
        }

        public void SetMaskSlots(Maid maid, PresetData preset) {
            SetMaskSlots(maid, preset.slots);
        }

        public void SetMaskSlots(List<CCSlot> slotList) {
            SetMaskSlots(CurrentMaid, slotList);
        }

        public void SetMaskSlots(Maid maid, List<CCSlot> slotList) {
            foreach (var slotItem in slotList) {
                // 未読み込みの場合はスキップ            
                if (slotItem.mask == SlotState.NotLoaded) continue;

                var slot = maid.body0.GetSlot((int)slotItem.id);
                switch (slotItem.mask) {
                case SlotState.Masked:
                    slot.listMaskSlot.Add((int)slotItem.id);
                    break;
                case SlotState.Displayed:
                    // 全スロットから削除する
                    foreach (var tBodySkin in maid.body0.goSlot) {
                        tBodySkin.listMaskSlot.Remove((int)slotItem.id);
                    }

                    break;
                }
                // NonDisplayの場合は何もしない

            }
        }
        // 表示状態を変更するのみ。
        // フラグを適用することで元に戻せる
        public void SetAllVisible() {
            foreach (var tBodySkin in CurrentMaid.body0.goSlot) {
                tBodySkin.boVisible = true;
            }
        }

        // マスク情報をすべてクリアして反映
        public void ClearMasks() {
            foreach (var tBodySkin in CurrentMaid.body0.goSlot) {
                tBodySkin.boVisible = true;
                tBodySkin.listMaskSlot.Clear();
            }
            FixFlag();
        }

        public void FixFlag() {
            FixFlag(CurrentMaid);
        }

        public void FixFlag(Maid maid, bool propProp=false)  {
            maid.body0.FixMaskFlag();
            maid.body0.FixVisibleFlag();

            if (propProp) {
                // 以下のフラグを立てることで、次回以降Maid.Update()でAllProcPropSeq()が実行される
                maid.AllProcPropSeqStart();
            }
        }
    }
}
