using System.Collections.Generic;
using System.Linq;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.TexAnim {
    /// <summary>
    /// Textureのアニメーションを実現するMonoBehaviour
    /// </summary>
    public class TexAnimator : MonoBehaviour {
        #region MonoBehaviour Methods

        void Start() {
            // LogUtil.Debug("Start TexAnimator object");
            if (!targets.Any()) ParseMaterials();
        }

        void OnDestroy() {
            // LogUtil.Debug("OnDestroy TexAnimator object");

            Clear();
        }

        // void FixedUpdate() {}
        void Update() {
            foreach (var target in targets) {
                target.Animate(Time.deltaTime);
            }
        }
        #endregion

        public bool ParseMaterials() {
            if (targets.Any()) Clear();

            var materials = GetMaterials();
            var hasAnimItem = false;
            if (materials != null) {
                var i = 0;
                foreach (var mate in materials) {
                    hasAnimItem |= ParseMaterial(mate, i++);
                }
            };
            return hasAnimItem;
        }

        public bool ParseMaterials(IEnumerable<MenuFileHandler.MateInfo> mis) {
            var materials = GetMaterials();
            var hasAnimItem = false;
            foreach (var mi in mis) {
                if (materials != null && mi.matNo < materials.Length) {
                    hasAnimItem |= ParseMaterial(materials[mi.matNo], mi.matNo);
                }
            }

            return hasAnimItem;
        }

        private bool ParseMaterial(Material mate, int matNo) {
            var animTexes = ParseAnimUtil.ParseAnimTex(mate);
            if (animTexes != null) {
                var target = GetTarget(matNo);
                if (target != null) {
                    target.UpdateTexes(mate, animTexes);
                } else {
                    targets.Add(new AnimItem(mate, matNo, animTexes));
                }

                return true;
            }

            return false;
        }

        private Material[] GetMaterials() {
            var children = gameObject.transform.GetComponentsInChildren<Renderer>(true);
            var render = children.FirstOrDefault(r => r.material != null && r.materials.Length > 0 && r.material.shader != null);
            return render == null ? null : render.materials;
        }

        private AnimItem GetTarget(int matNo) {
            foreach (var target in targets) {
                if (target.matNo == matNo) {
                    return target;
                }
            }

            return null;
        }

        public void RemoveTarget(int matNo) {
            // LogUtil.Debug("RemoveTarget:", matNo);

            var toRemove = GetTarget(matNo);
            if (toRemove != null) {
                toRemove.Deactivate();
                targets.Remove(toRemove);
            }
        }

        public void SetTargets(List<AnimItem> items) {
            if (targets.Any()) Clear();
            targets.AddRange(items);
        }

        public void Clear() {

            foreach (var toRemove in targets) {
                toRemove.Deactivate();
            }
            targets.Clear();
        }

        private readonly List<AnimItem> targets = new List<AnimItem>();
    }
}
