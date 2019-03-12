using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.TexAnim {
    /// <summary>
    /// アニメーション用のアイテムを扱うクラス.
    /// マテリアルと対応するアイテムとし、同一マテリアル内のテクスチャを複数保持する.
    /// </summary>
   public class AnimItem {

        public AnimItem(Material mat, int matNo, AnimTex[] texes) {
            material = mat;
            this.matNo = matNo;
            this.texes = texes;
        }

        public void Animate(float deltaTime) {
            if (material == null) return;

            for (var i=0; i<texes.Length; i++) {
                var tex = texes[i];
                if (tex != null && tex.updateTime(deltaTime)) {
                    if (HasTarget(ref tex)) {
                        material.SetTextureOffset(tex.texProp.PropId, tex.nextOffset());
                    }
                    texes[i] = tex;
                }
            }
        }

        public void Deactivate() {
            if (material == null) return;

            if (settings.backScale) {
                // textureが置き換わっていて、リストアが必要であれば行う
                for (var i=0; i<texes.Length; i++) {
                    var tex = texes[i];
                    if (tex != null) {
                        RestoreTexPos(tex);
                        texes[i] = null;
                    }
                }
            }

            material = null;
        }

        /// <summary>
        /// アニメーション対象のテクスチャのIDが変わっていないか判定し、
        /// 変更されたテクスチャがアニメ対象外であればマテリアルのスケールを強制的に0,1にセットし、falseを返す
        ///
        ///
        /// menuからの"マテリアル変更"は、materialオブジェクトを使いまわす
        /// "テクスチャ変更"は、さらにオフセットやスケールが変わらない仕様のため、
        /// ***_anim.tex以外のファイルはスケールを元に戻す
        /// </summary>
        /// <param name="anmTex">チェック対象オブジェクト</param>
        /// <returns>アニメーション対象であればtrue　それ以外はfalse</returns>
        private bool HasTarget(ref AnimTex anmTex) {
            var tex1 = material.GetTexture(anmTex.texProp.PropId);
            if (tex1 == null) {
                // LogUtil.Debug("has no texture. ", anmTex.texProp, ", tex=", anmTex);
                anmTex = null;
                return false;
            }

            if (tex1.GetInstanceID() != anmTex.texId) {
                // LogUtil.Debug("tex id changed.", anmTex.texId, "=>", tex1.GetInstanceID());
                var parsedAnmTex = ParseAnimUtil.ParseAnimTex(material, anmTex.texProp, tex1);
                if (parsedAnmTex == null) {
                    // LogUtil.Debug(" no anim tex");
                    if (settings.backScale) RestoreTexPos(anmTex);
                    anmTex = null;
                    return false;
                }

                anmTex = parsedAnmTex;
            }
            return true;
        }

        private void RestoreTexPos(AnimTex anmTex) {
            // マテリアルのスケールとオフセットをデフォルトに復元
            var propId = anmTex.texProp.PropId;
            var propFPSId = anmTex.texProp.PropFPSId;

            material.SetTextureScale(propId, Vector2.one);
            material.SetTextureOffset(propId, Vector2.zero);
            if (material.HasProperty(propFPSId)) {
                material.SetFloat(propFPSId, -1);
            }
        }

        public void UpdateTexes(Material mate, AnimTex[] texes1) {
            if (material != mate) {
                material = mate;
                texes = texes1;

            } else {
                for (var i=0; i<texes.Length; i++) {
                    if (texes[i] == null && texes1[i] != null) {
                        texes[i] = texes1[i];
                    }
                }
            }
        }

        private static readonly Settings settings = Settings.Instance;
        public Material material;
        public int matNo;

        public AnimTex[] texes;
    }
}
