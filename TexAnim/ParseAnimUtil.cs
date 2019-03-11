using System;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.TexAnim {
    /// <summary>
    /// マテリアル情報にアニメーションターゲットが含まれるかをパースするユーティリティクラス.
    /// </summary>
    public static class ParseAnimUtil {
        private static readonly Settings settings = Settings.Instance;

        private static readonly TexProp[] targets = {TexProp.MainTex, TexProp.ShadowTex,};

        /// <summary>
        /// マテリアル情報からアニメーションタイプを判断して、対象のオブジェクトを生成する.
        /// _MainTexと_ShadowTexのみ走査し、見つからない場合はnullを返す.
        /// </summary>
        /// <param name="m">マテリアル</param>
        /// <returns></returns>
        public static AnimTex[] ParseAnimTex(Material m) {
            AnimTex[] ret = null;
            var i=0;
            foreach (var type in targets) {
                var animTex = ParseAnimTex(m, type);
                if (animTex != null) {
                    ret = ret ?? new AnimTex[targets.Length];
                    ret[i] = animTex;
                }
                i++;
            }

            return ret;
        }

        public static AnimTex ParseAnimTex(Material mate, TexProp texProp, Texture tex=null) {
            try {
                if (tex != null) {
                    if (!IsTarget(tex)) return null;
                } else {
                    if (!TryGetTargetTex(mate, texProp, out tex)) return null;
                }

                var scale = (texProp == TexProp.MainTex)? mate.mainTextureScale : mate.GetTextureScale(texProp.PropId);
                if ( mate.HasProperty(texProp.PropScrollXId) || mate.HasProperty(texProp.PropScrollYId)) {
                    Vector2 scroll;
                    scroll.x = fitting( mate.GetFloat(texProp.PropScrollXId) );
                    scroll.y = fitting( mate.GetFloat(texProp.PropScrollYId) );
                    var frameSecond = ParseFrameSecond(mate, texProp);
                    var animTex = new ScrollTex(scroll, tex, frameSecond) {texProp = texProp};

                    animTex.InitOffsetIndex(mate.GetTextureOffset(texProp.PropId));
                    return animTex;
                }

                if (scale.x <= 0.5f || scale.y <= 0.5f) {
                    if (Equals(scale.x, 0) || Equals(scale.y, 0)) return null;

                    var frameSecond = ParseFrameSecond(mate, texProp);
                    var animTex = new SlideScaledTex(scale, tex, frameSecond) {texProp = texProp};

                    if (animTex.imageLength > 1) {
                        animTex.InitOffsetIndex(mate.GetTextureOffset(texProp.PropId));
                        LogUtil.DebugF("{0} X:{1},Y:{2},length={3}", texProp, animTex.ratioX, animTex.ratioY, animTex.imageLength);
                        return animTex;
                    }
                    return animTex;
                }
            } catch(Exception e) {
                // シェーダに未対応テクスチャが設定される場合などではNullRefが発生するため、スルー
                LogUtil.Debug("Failed to parse texture:", texProp, e);
            }
            return null;
        }
        private static float fitting(float f) {
            if (f > 0.5f) return 0.5f;
            return f < -0.5f ? -0.5f : f;
        }

        private static float ParseFrameSecond(Material m, TexProp prop) {
            if (m.HasProperty(prop.PropFPSId)) {
                var val = m.GetFloat(prop.PropFPSId);
                if (val > 0) return 1/val;
            }
            return settings.defaultFrameSecond;
        }

        public static bool Equals(float left, float right, float epsilon= 0.000001f) {
            if (left < right) return right - left < epsilon;
            return left - right < epsilon;
        }

        public static bool HasTargetTexName(Material m, TexProp texType) {
            var tex = (texType == TexProp.MainTex)? m.mainTexture : m.GetTexture(texType.PropId);
            return tex != null && IsTarget(tex);
        }

        public static bool TryGetTargetTex(Material m, TexProp texType, out Texture tex) {
            tex = (texType == TexProp.MainTex)? m.mainTexture : m.GetTexture(texType.PropId);
            return tex != null && IsTarget(tex);
        }

        public static bool IsTarget(Texture tex) {
            if (tex.name.Length > 4) {
                return tex.name[tex.name.Length - 4] != '.' 
                    ? tex.name.EndsWith(settings.namePostfix, StringComparison.OrdinalIgnoreCase)
                    : tex.name.EndsWith(settings.namePostfixWithExt, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }

}
