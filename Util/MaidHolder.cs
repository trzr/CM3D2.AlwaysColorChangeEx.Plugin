using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CM3D2.AlwaysColorChange.Plugin.Data;

namespace CM3D2.AlwaysColorChange.Plugin.Util
{
    /// <summary>
    /// Description of MaidHolder.
    /// </summary>
    public sealed class MaidHolder
    {
        private static MaidHolder instance = new MaidHolder();        
        public static MaidHolder Instance {
            get { return instance;  }
        }

        private static readonly List<Material> EmptyList = new List<Material>(0);
        
        private MaidHolder() { }

        // 選択中のメイド
        public Maid maid { get; set; }
        // 選択中のスロット
        public SlotInfo currentSlot { get; set; }
            
        /// <summary>選択中のスロットを取得する</summary>
        /// <returns>スロット</returns>
        public TBodySkin GetCurrentSlot() {
            return maid.body0.GetSlot(currentSlot.Name);
        }

        public List<Material> GetMaterials() {
            return GetMaterials( maid.body0.GetSlot(currentSlot.Name) );
        }

        public List<Material> GetMaterials(SlotInfo slot) {
            return GetMaterials(slot.Name);
        }

        public List<Material> GetMaterials(string slotName) {
            return GetMaterials(maid.body0.GetSlot(slotName));
        }

        public List<Material> GetMaterials(TBodySkin slot)
        {
            GameObject gobj = slot.obj;
            if (gobj == null) {
                return EmptyList;
            }

            var materialList = new List<Material>();
            Transform[] componentsInChildren = gobj.transform.GetComponentsInChildren<Transform>(true);
            foreach (Transform tf in componentsInChildren) {
                Renderer r = tf.renderer;
                if (r != null && r.material != null && r.material.shader != null) {
                    // 確認：複数回ヒットするケースが存在するのか？基本はないが…
                    materialList.AddRange(r.materials);
                }
            }
            return materialList;

//            var materialList = new List<Material>();
//            int idx = 0;
//            Transform[] componentsInChildren = gobj.transform.GetComponentsInChildren<Transform>(true);
//            foreach (Transform tf in componentsInChildren) {
//                Renderer r = tf.renderer;
//                if (r != null && r.material != null && r.materials.Length > 0 && r.material.shader != null) {
//                    materialList.AddRange(r.materials);
//                    var buf = new StringBuilder();
//                    buf.Append(r.name).Append("=>");
//                    foreach (Material m in r.materials) {
//                        buf.Append(m.name).Append(",");
//                    }
//                    buf.Append(r.materials.Length);
//                    LogUtil.DebugLog(slotName, idx, buf);
//                }
//                idx++;
//            }
//            return materialList;
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
            TBody body = maid.body0;
            List<TBodySkin> goSlot = body.goSlot;
            int index = (int)global::TBody.hashSlotName[slotName];
            global::TBodySkin tBodySkin = goSlot[index];
            GameObject gobj = tBodySkin.obj;
            if (gobj == null) return null;

            Transform[] componentsInChildren = gobj.transform.GetComponentsInChildren<Transform>(true);
            foreach (Transform tf in componentsInChildren) {
                Renderer r = tf.renderer;
                if (r != null && r.material != null && r.materials.Length > matNo) {
                    return r.materials[matNo];
                }
            }
            return null;
        }

        private List<Renderer> GetRenderers(string slotName)
        {
            TBody body = maid.body0;
            List<TBodySkin> goSlot = body.goSlot;
            int index = (int)global::TBody.hashSlotName[slotName];
            global::TBodySkin tBodySkin = goSlot[index];
            GameObject obj = tBodySkin.obj;
            if (obj == null) {
                return null;
            }
            var rendererList = new List<Renderer>();
            Transform[] componentsInChildren = obj.transform.GetComponentsInChildren<Transform>(true);
            foreach (Transform tf in componentsInChildren) {
                Renderer r = tf.renderer;
                if (r != null) {
                    rendererList.Add(r);
                }
            }
            return rendererList;
        }

        public void SetDelNodes(Dictionary<string, bool> dDelNodes, bool bApply) {
            if (!dDelNodes.Any()) return;

            foreach (TBodySkin slot in maid.body0.goSlot) {
                slot.boVisible = true;
                foreach (KeyValuePair<string, bool> entry in dDelNodes) {
                    if (slot.m_dicDelNodeBody.ContainsKey(entry.Key)) {
                        slot.m_dicDelNodeBody[entry.Key] = entry.Value;
                    }
                }
            }
            if (bApply) FixFlag();
        }
            
        private Hashtable GetMaskTable() {
            if (maid == null) return null;
            try {
                var field = maid.body0.GetType().GetField("m_hFoceHide",BindingFlags.NonPublic | BindingFlags.Instance);//  | BindingFlags.GetField | BindingFlags.SetField
                return (Hashtable)field.GetValue(maid.body0);
            } catch(Exception e) {}
            return null;
        }

        public void SetSlotVisibles(Dictionary<TBody.SlotID, MaskInfo> maskDic) {
            Hashtable m_foceHide = GetMaskTable();
            if (m_foceHide == null) {
                LogUtil.ErrorLog("cannot take MaskTable");
                return;
            }

            foreach (KeyValuePair<TBody.SlotID, MaskInfo> pair in maskDic) {
                MaskInfo maskInfo = pair.Value;
                // 未読み込みの場合はスキップ
                if (maskInfo.state == SlotState.NotLoaded) continue;

                maskInfo.slot.boVisible = maskInfo.value;
//                m_foceHide[pair.Key] = maskInfo.value;
            }
        }

        // 表示状態を変更するのみ。
        // フラグを適用することで元に戻せる
        public void SetAllVisible() {
            foreach (TBodySkin tBodySkin in maid.body0.goSlot) {
                tBodySkin.boVisible = true;
            }
        }

        // マスク情報をすべてクリアして反映
        public void ClearMasks()
        {
            foreach (TBodySkin tBodySkin in maid.body0.goSlot) {
                tBodySkin.boVisible = true;
                tBodySkin.listMaskSlot.Clear();
            }
            FixFlag();
        }

        public void FixFlag() {
            maid.body0.FixMaskFlag();
            maid.body0.FixVisibleFlag(false);
            maid.AllProcPropSeqStart();
        }
    }
}
