using System;
using System.Collections.Generic;
using System.Linq;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.TexAnim {
    public class AnimTargetDetector {
        /// <summary>アニメーション対象でないmenuIDのSet</summary>
        private readonly HashSet<int> NoAnimMenuId = new HashSet<int>();
        private readonly MenuFileHandler menuHandler = new MenuFileHandler();

        public void ChangeMenu(Maid maid, MaidProp prop) {
            if (prop.nFileNameRID == 0 || NoAnimMenuId.Contains(prop.nFileNameRID)) return;

//            bool isAnimTarget;
//            if (AnimMenu.TryGetValue(prop.nFileNameRID, out isAnimTarget)) {
//                menuHandler.Parse(prop.strFileName);
//            }
            var changeInfos = menuHandler.Parse(prop.strFileName);
            if (changeInfos == null) return;
            var hasTarget = false;
            foreach (var changeInfo in changeInfos) {
                LogUtil.Debug("change item:", changeInfo.slot, ", matInfos:", changeInfo.matInfos.Count);

                TBody.SlotID slotID;
                if (!EnumUtil.TryParse(changeInfo.slot, true, out slotID)) continue;
                var slot = maid.body0.GetSlot((int) slotID);
                if (slot.obj == null) continue;

                var animator = slot.obj.transform.GetComponentInChildren<TexAnimator>(false);
                if (animator == null) {
                    var mates = GetMaterials(slot);
                    var animItems = ParseMaidSlot(slot, mates, changeInfo.matInfos);
                    if (animItems != null) {
                        LogUtil.Debug("AddComponent for ", slot, ", from ", prop.name);
                        animator = slot.obj.AddComponent<TexAnimator>();
                        animator.name = "TexAnimator";
                        animator.SetTargets(animItems);
                        hasTarget = true;
                    }

                    continue;
                }

                if (changeInfo.matInfos == null) {
                    hasTarget |= animator.ParseMaterials();
                } else {
                    hasTarget |= animator.ParseMaterials(changeInfo.matInfos);
                }
            }

            if (!hasTarget) NoAnimMenuId.Add(prop.nFileNameRID);
        }

        public List<AnimItem> ParseMaidSlot(TBodySkin slot, Material[] mates, IList<MenuFileHandler.MateInfo> miList) {
            if (mates == null) return null;

            // 変更のあったスロットをパースし、マテリアル/テクスチャからアニメーション対象を抽出
            List<AnimItem> targets = null;
            try {
                if (miList == null) {
                    var mateNo = 0;
                    foreach (var mate in mates) {
                        var animTexes = ParseAnimUtil.ParseAnimTex(mate);
                        if (animTexes != null) {
                            if (targets == null) targets = new List<AnimItem>();
                            targets.Add(new AnimItem(mate, mateNo, animTexes));
                        }
                        mateNo++;
                    }
                } else {
                    foreach (var mi in miList) {
                        var mate = mates[mi.matNo];
                        var animTexes = ParseAnimUtil.ParseAnimTex(mate);
                        if (animTexes != null) {
                            if (targets == null) targets = new List<AnimItem>();
                            targets.Add(new AnimItem(mate, mi.matNo, animTexes));
                        }
                    }
                }

            } catch(Exception e) {
                LogUtil.Debug("slotId:", slot, e.Message);
            }
            return targets;
        }

        private Material[] GetMaterials(TBodySkin slot) {
            var children = slot.obj.transform.GetComponentsInChildren<Renderer>(true);
            foreach (var r in children) {
                if (r.material != null && r.materials.Length > 0 && r.material.shader != null) {
                    return r.materials;
                }
            }
            return null;
        }
    }
}
