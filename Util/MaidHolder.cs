using System;
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
    /// Description of MaidHolder.
    /// </summary>
    public sealed class MaidHolder
    {
        private static MaidHolder instance = new MaidHolder();        
        public static MaidHolder Instance {
            get { return instance;  }
        }

        private static readonly Material[] EmptyList = new Material[0];
        private static readonly List<Material>   EmptyList1 = new List<Material>(0);
        
        public string MaidName { get; private set; }
        // 選択中のメイド
        public Maid currentMaid { get; set; }
        // 選択中のスロット
        public SlotInfo currentSlot { get; set; }

        private MaidHolder() {
            MaidName = string.Empty;
        }

        public bool CurrentActivated() {
            return currentMaid != null && currentMaid.isActiveAndEnabled;
        }

        // enabledであればデータ参照可
        public bool CurrentEnabled() {
            return currentMaid != null && currentMaid.enabled;
        }
        // メイドの更新
        // 
        // @return 別のメイドに変更された場合はtrueを返す
        public bool UpdateMaid(Maid maid0, Action act) 
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
            if (maid0 != null) {
                MaidName = currentMaid.Param.status.last_name + " " + currentMaid.Param.status.first_name;                
            } else {
                MaidName = "(not selected)";
            }

            act();
            return true;
        }
        // メイドの更新
        // 
        // @return 別のメイドに変更された場合はtrueを返す
        public bool UpdateMaid(Action act) 
        {
            return UpdateMaid(null, act);
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

        /// <summary>選択中のスロットを取得する</summary>
        /// <returns>スロット</returns>
        public TBodySkin GetCurrentSlot() {
            return currentMaid.body0.GetSlot((int)currentSlot.Id);
        }

        public Material[] GetMaterials() {
            return GetMaterials( currentMaid.body0.GetSlot((int)currentSlot.Id) );
        }

        public Material[] GetMaterials(SlotInfo slot) {
            return GetMaterials(slot.Id);
        }
        public Material[] GetMaterials(TBody.SlotID slotID) {
            return GetMaterials(currentMaid.body0.GetSlot((int)slotID));
        }

        public Material[] GetMaterials(string slotName) {
            return GetMaterials(currentMaid.body0.GetSlot(slotName));
        }

        public Material[] GetMaterials(TBodySkin slot)
        {
            GameObject gobj = slot.obj;
            if (gobj == null) {
                return EmptyList;
            }

            Transform[] componentsInChildren = gobj.transform.GetComponentsInChildren<Transform>(true);
            foreach (Transform tf in componentsInChildren) {
                Renderer r = tf.renderer;
                if (r != null && r.material != null && r.materials.Length > 0 && r.material.shader != null) {
                    // 確認：複数回ヒットするケースが存在するのか？基本はないが…
                    return r.materials;

                    //// 確認用ログ
                    //if (materialList.Count > 1) {
                    //    LogUtil.Log("multiple materials exist.", materialList.Count, slot.m_strModelFileName);
                    //}
                }
            }
            return EmptyList;
        }
        public Material GetMaterial(int matNo) {
            return GetMaterial(currentMaid.body0.GetSlot(currentSlot.Name), matNo);
        }
        public Material GetMaterial(TBodySkin slot, int matNo)
        {
            if (slot.obj == null) return null;

            Transform[] componentsInChildren = slot.obj.transform.GetComponentsInChildren<Transform>(true);
            foreach (Transform tf in componentsInChildren) {
                Renderer r = tf.renderer;
                if (r != null && r.material != null && r.materials.Length > matNo && r.material.shader != null) {
                    return r.materials[matNo];
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

            Transform[] componentsInChildren = slot.obj.transform.GetComponentsInChildren<Transform>(true);
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
            TBody body = currentMaid.body0;
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

        public void SetDelNodes(Dictionary<string, bool> dDelNodes, bool bApply) 
        {
            if (!dDelNodes.Any()) return;

            foreach (TBodySkin slot in currentMaid.body0.goSlot) {
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

        public void SetSlotVisibles(Dictionary<TBody.SlotID, MaskInfo> maskDic, bool tmp) 
        {
//            Hashtable m_foceHide = GetMaskTable();
//            if (m_foceHide == null) {
//                LogUtil.ErrorLog("cannot take MaskTable");
//                return;
//            }
            
            foreach (KeyValuePair<TBody.SlotID, MaskInfo> pair in maskDic) {
                MaskInfo maskInfo = pair.Value;
                // 未読み込みの場合はスキップ
            
                if (maskInfo.state == SlotState.NotLoaded) continue;

                maskInfo.slot.boVisible = maskInfo.value;

                if (!tmp) {
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

        public void FixFlag() 
        {
            currentMaid.body0.FixMaskFlag();
            currentMaid.body0.FixVisibleFlag(false);
            currentMaid.AllProcPropSeqStart();
        }
    }
}
