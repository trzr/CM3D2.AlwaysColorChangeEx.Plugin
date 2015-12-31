using System;
using System.Collections.Generic;
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
            get {
                return instance;
            }
        }
        
        private MaidHolder()
        {
        }
        public Maid maid { get; set; }
        private static readonly List<Material> EmptyList = new List<Material>(0);


        public List<Material> GetMaterials(SlotInfo slot)
        {
            return GetMaterials(slot.Name);
        }
        public List<Material> GetMaterials(string slotName)
        {
            TBody body = maid.body0;
            List<TBodySkin> goSlot = body.goSlot;
            int index = (int)global::TBody.hashSlotName[slotName];
            global::TBodySkin tBodySkin = goSlot[index];
            GameObject gobj = tBodySkin.obj;
            if (gobj == null) {
                return EmptyList;
            }

            var materialList = new List<Material>();
            Transform[] componentsInChildren = gobj.transform.GetComponentsInChildren<Transform>(true);
            foreach (Transform tf in componentsInChildren) {
                Renderer r = tf.renderer;
                if (r != null && r.material != null && r.material.shader != null) {
                    materialList.AddRange(r.materials);
                }
            }
            return materialList;
        }

        private List<Renderer> GetRenderers(string slotname)
        {
            TBody body = maid.body0;
            List<TBodySkin> goSlot = body.goSlot;
            int index = (int)global::TBody.hashSlotName[slotname];
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
