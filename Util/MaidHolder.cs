using System;
using System.Diagnostics;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util
{
    /// <summary>
    /// 編集中のメイド情報を扱うデータホルダクラス
    /// </summary>
    public sealed class MaidHolder
    {
        private static MaidHolder instance = new MaidHolder();        
        public static MaidHolder Instance {
            get { return instance;  }
        }
        private readonly int SLOT_COUNT;
//        public static void Init() {
//            if (SLOT_COUNT <= 0) {
//                SLOT_COUNT = TBody.m_strDefSlotName.Length/cnt;
//            }
//        }
        private readonly Material[] EmptyList = new Material[0];
        private readonly List<Material>   EmptyList1 = new List<Material>(0);
        
        public string MaidName { get; private set; }
        // 選択中のメイド
        public Maid currentMaid { get; set; }
        // 選択中のスロット
        public SlotInfo currentSlot { get; set; }

        private MaidHolder() {
            MaidName = string.Empty;
            int cnt = PrivateAccessor.Get<int>(typeof(TBody),"strSlotNameItemCnt");
            SLOT_COUNT = TBody.m_strDefSlotName.Length/cnt;
        }
        public bool Applicable() {
            return (currentMaid != null) && !currentMaid.boAllProcPropBUSY;
        }
        public bool CurrentActivated() {
            return currentMaid != null && currentMaid.isActiveAndEnabled;
        }
        // enabledであればデータ参照可
        public bool CurrentEnabled() {
            return currentMaid != null && currentMaid.enabled;
        }

        public bool isOfficial;
        public bool checkOfficial(Maid maid) {
            //LogUtil.Debug("slotCount:", SLOT_COUNT, ", maid. count=", maid.body0.goSlot.Count);
            return (maid.body0.goSlot.Count == SLOT_COUNT);
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
        public bool UpdateMaid(Maid maid0, string name, Action act) 
        {
            if (maid0 == null) {
                // メイドリストから最初に有効なメイドを取得
                int count = GameMain.Instance.CharacterMgr.GetMaidCount();
                for (int i=0; i< count; i++) {
                    Maid m = GameMain.Instance.CharacterMgr.GetMaid(i);
                    if (m != null && m.enabled) {
                        maid0 = m;
                        break;
                    }
                }            
            }
            if (currentMaid == maid0) return false;
            currentMaid = maid0;
            if (currentMaid != null) {
                MaidName = name?? currentMaid.Param.status.last_name + " " + currentMaid.Param.status.first_name;

                isOfficial = checkOfficial(currentMaid);
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
        public string GetCurrentMenuFile() 
        {
            if (currentMaid != null) {
                MaidProp prop = currentMaid.GetProp(currentSlot.mpn);
                if (prop != null) return prop.strFileName;

                LogUtil.Log("maid prop is null", currentSlot.mpn);
            }
            return null;
        }
        public int GetCurrentMenuFileID() 
        {
            if (currentMaid != null) {
                MaidProp prop = currentMaid.GetProp(currentSlot.mpn);
                if (prop != null) return prop.nFileNameRID;

                LogUtil.Log("maid prop is null", currentSlot.mpn);
            }
            return 0;
        }

        /// <summary>選択中のスロットを取得する</summary>
        /// <returns>スロット</returns>
        public TBodySkin GetCurrentSlot() {
            return currentMaid.body0.GetSlot((int)currentSlot.Id);
        }

        public Material[] GetMaterials() {
            return GetMaterials(currentSlot.Id);
        }

        public Material[] GetMaterials(SlotInfo slot) {
            return GetMaterials(slot.Id);
        }
        public Material[] GetMaterials(TBody.SlotID slotID) {
            int slotNo = (int)slotID;
            return slotNo >= currentMaid.body0.goSlot.Count
                ? EmptyList
                : GetMaterials(currentMaid.body0.GetSlot(slotNo));
        }

        public Material[] GetMaterials(string slotName) {
            return GetMaterials(currentMaid.body0.GetSlot(slotName));
        }

        public Renderer GetRenderer(TBody.SlotID slotID) {
            int slotNo = (int)slotID;
            return slotNo >= currentMaid.body0.goSlot.Count
                ? null
                : GetRenderer(currentMaid.body0.GetSlot(slotNo));
        }

        public Material[] GetMaterials(TBodySkin slot)
        {
            var renderer = GetRenderer(slot);
            return renderer == null ? EmptyList : renderer.materials;
        }
        public Material GetMaterial(int matNo) {
            return GetMaterial(currentMaid.body0.GetSlot((int)currentSlot.Id), matNo);
        }
        public Material GetMaterial(TBodySkin slot, int matNo)
        {
            if (slot.obj == null) return null;

            var r = GetRenderer(slot.obj, matNo);
            return r != null ? r.materials[matNo] : null;
        }

        public Renderer GetRenderer(TBodySkin slot) 
        {
            GameObject gobj = slot.obj;
            if (gobj == null) return null;

            return GetRenderer(gobj, 0);
        }
        private Renderer GetRenderer(GameObject gobj, int matNo) {
            // trueにするのはマスク等で非表示のアイテムの情報も扱うため
            Renderer[] children = gobj.transform.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in children) {
                if (r.material != null && r.materials.Length > matNo && r.material.shader != null) {
                    return r;
                }
            }
            return null;
        }

        /// <summary>
        /// スロット名とマテリアル番号を指定して、マテリアルオブジェクトを取得する
        /// </summary>
        /// 
        /// <param name="slotName">スロット名(列挙型の名前)</param>
        /// <param name="matNo">マテリアル番号</param>
        /// <returns>マテリアル　ただし、見つからない場合はnullを返す</returns>
        public Material GetMaterial(string slotName, int matNo)
        {
            global::TBodySkin slot = currentMaid.body0.GetSlot(slotName);
            if (slot.obj == null) return null;

            var r = GetRenderer(slot.obj, matNo);
            return r != null ? r.materials[matNo] : null;
        }

        public void SetDelNodes(PresetData preset, bool bApply) {
            SetDelNodes(currentMaid, preset.delNodes, bApply);
        }
        public void SetDelNodes(Maid maid, PresetData preset, bool bApply) {
            // if (preset.delNodes == null) return;
            SetDelNodes(maid, preset.delNodes, bApply);
        }
        public void SetDelNodes(Dictionary<string, bool> dDelNodes, bool bApply) {
            SetDelNodes(currentMaid, dDelNodes, bApply);
        }
        public void SetDelNodes(Maid maid, Dictionary<string, bool> dDelNodes, bool bApply) 
        {
            if (!dDelNodes.Any()) return;

            foreach (KeyValuePair<string, bool> entry in dDelNodes) {
                var nodeItem = ACConstants.NodeNames[entry.Key];
                if (entry.Value) { // 
                    foreach (TBodySkin slot in maid.body0.goSlot) {
                        if (slot.obj == null) continue;
                        // 強制表示
                        if (slot.m_dicDelNodeBody.ContainsKey(entry.Key)) {
                            slot.m_dicDelNodeBody[entry.Key] = true; // entry.Value
                        }
                    }
                } else {
                    bool hasSet = false;
                    foreach (TBody.SlotID slotId in nodeItem.slots) {
                        var slot = maid.body0.GetSlot((int)slotId);
                        if (slot.obj == null) continue;

                        // slot.boVisible = true;
                        if (slot.m_dicDelNodeBody.ContainsKey(entry.Key)) {
                            slot.m_dicDelNodeBody[entry.Key] = false; // entry.Value
                        }
                        hasSet = true;
                        break;
                    }
                    if (!hasSet) {
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
            SetDelNodesForce(currentMaid, dDelNodes, bApply);
        }
        // 強引に全スロットに対してノード非表示を適用
        public void SetDelNodesForce(Maid maid, Dictionary<string, bool> dDelNodes, bool bApply) 
        {
            if (!dDelNodes.Any()) return;

            foreach (TBodySkin slot in maid.body0.goSlot) {
                if (slot.obj == null) continue;
                slot.boVisible = true;
                foreach (KeyValuePair<string, bool> entry in dDelNodes) {
                    if (slot.m_dicDelNodeBody.ContainsKey(entry.Key)) {
                        slot.m_dicDelNodeBody[entry.Key] = entry.Value;
                    }
                }
            }
            if (bApply) FixFlag();
        }

        private Hashtable GetMaskTable() 
        {
            return currentMaid == null 
                ? null
                : PrivateAccessor.Get<Hashtable>(currentMaid.body0, "m_hFoceHide");
        }

        /// <summary>
        /// スロットの可視性を設定する.
        /// temporaryにtrueを設定すると可視性のみ設定するがfalseの場合は、スロットへのマスク設定を行う
        /// </summary>
        /// <param name="maskDic">マスク設定Dic</param>
        /// <param name="temporary">一時適用フラグ</param>
        public void SetSlotVisibles(Dictionary<TBody.SlotID, MaskInfo> maskDic, bool temporary) 
        {
//            Hashtable m_foceHide = GetMaskTable();
//            if (m_foceHide == null) {
//                LogUtil.Error("cannot take MaskTable");
//                return;
//            }
            
            foreach (KeyValuePair<TBody.SlotID, MaskInfo> pair in maskDic) {
                MaskInfo maskInfo = pair.Value;
                // 未読み込みの場合はスキップ
            
                if (maskInfo.state == SlotState.NotLoaded) continue;

                maskInfo.slot.boVisible = maskInfo.value;

                if (!temporary) {
                    TBodySkin slot = currentMaid.body0.GetSlot((int)pair.Key);
                    if (!maskInfo.value) {
                        slot.listMaskSlot.Add((int)pair.Key);
                    } else {
                        // 全スロットから削除する
                        //slot.listMaskSlot.Remove((int)pair.Key);
                        foreach (TBodySkin tBodySkin in currentMaid.body0.goSlot) {
                            tBodySkin.listMaskSlot.Remove((int)pair.Key);
                        }
                    }
                    // 下記の情報はGetMaskで取得されるフラグに関するものであるが、
                    // maskItemとは別で、下着モード、ヌードモードなどの指定で使われるフラグ
                    //if (!tmp) m_foceHide[pair.Key] = !maskInfo.value;
                }
            }
        }
        public void SetMaskSlots(PresetData preset) {
            SetMaskSlots(currentMaid, preset.slots);
        }
        public void SetMaskSlots(Maid maid, PresetData preset) {
            SetMaskSlots(maid, preset.slots);
        }
        public void SetMaskSlots(List<CCSlot> slotList) {
            SetMaskSlots(currentMaid, slotList);
        }
        public void SetMaskSlots(Maid maid, List<CCSlot> slotList) 
        {
            foreach (var slotItem in slotList) {
                // 未読み込みの場合はスキップ            
                if (slotItem.mask == SlotState.NotLoaded) continue;

                TBodySkin slot = maid.body0.GetSlot((int)slotItem.id);
                if (slotItem.mask == SlotState.Masked) {
                    slot.listMaskSlot.Add((int)slotItem.id);

                } else if (slotItem.mask == SlotState.Displayed) {
                    // 全スロットから削除する
                    foreach (TBodySkin tBodySkin in maid.body0.goSlot) {
                        tBodySkin.listMaskSlot.Remove((int)slotItem.id);
                    }
                }
                // NonDisplayの場合は何もしない

            }
        }
        // 表示状態を変更するのみ。
        // フラグを適用することで元に戻せる
        public void SetAllVisible() 
        {
            foreach (TBodySkin tBodySkin in currentMaid.body0.goSlot) {
                tBodySkin.boVisible = true;
            }
        }

        // マスク情報をすべてクリアして反映
        public void ClearMasks()
        {
            foreach (TBodySkin tBodySkin in currentMaid.body0.goSlot) {
                tBodySkin.boVisible = true;
                tBodySkin.listMaskSlot.Clear();
            }
            FixFlag();
        }
        public void FixFlag() {
            FixFlag(currentMaid);
        }
        public void FixFlag(Maid maid) 
        {
            maid.body0.FixMaskFlag();
            maid.body0.FixVisibleFlag(false);
            maid.AllProcPropSeqStart();
        }
    }
}
