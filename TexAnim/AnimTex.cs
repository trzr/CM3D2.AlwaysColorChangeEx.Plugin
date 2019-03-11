using System;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.TexAnim {
    /// <summary>
    /// Description of AnimTex.
    /// </summary>
    public abstract class AnimTex {
        private float dTime;
        public float changeFrameSecond;

        public TexProp texProp;

        protected AnimTex(float frameSecond) {
            changeFrameSecond = frameSecond;
        }
        protected Texture tex;
        public Texture Tex {
            get { return tex; }
            set {
                tex = value;
                texId = tex.GetInstanceID();
            }
        }
        public int texId { get; private set;}

        public virtual bool updateTime(float deltaTime) {
            dTime += deltaTime;
            if (dTime <= changeFrameSecond) return false;

            dTime = 0.0f;
            return true;
        }
        public abstract Vector2 nextOffset();


        public abstract void InitOffsetIndex(Vector2 offset) ;
    }

    /// <summary>
    /// スライド用テクスチャ
    /// </summary>
    public class SlideScaledTex : AnimTex {
        private int frameNo;

        private Vector2 scale;
        private Vector2[] offsets;
        public int ratioX;
        public int ratioY;
        public int imageLength;
        public SlideScaledTex(Vector2 scale, Texture tex, float frameSec) 
            : base(frameSec) {
            SetScale(ref scale);
            Tex = tex;
        }

        public void SetScale(ref Vector2 scale1) {
            scale = scale1;
            ratioX  = (int)(Math.Round(1/scale.x, 3));// 四捨五入してから切り捨て
            ratioY  = (int)(Math.Round(1/scale.y, 3));

            // スライド用のイメージオフセット配列を生成
            imageLength = ratioX * ratioY;
            offsets = new Vector2[imageLength];
            for (var i=0; i<imageLength; i++) {
                offsets[i].x = scale.x * (i%ratioX);
                offsets[i].y = scale.y * ((imageLength-1-i)/ratioX);// unity座標は、Y軸↑が正
            }
        }
//        // 指定したインデックスの前にある文字列から数値を抽出する
//        private static int parseNum(string name, int endIdx, int max) {
//            int ret = 0;
//            int idx = endIdx-1;
//            int digit = 0;
//            while (idx >=0 && digit < max) {
//                int ascii = (int)name[idx];
//                if (48<= ascii && ascii <=57) {
//                    ret += (int)((ascii-48)*Math.Pow(10, digit++));
//                } else {
//                    break;
//                }
//            }
//            return ret;
//        }
        public override Vector2 nextOffset() {
            if(++frameNo >= imageLength) {
                frameNo = 0;
            }
            return offsets[frameNo];
        }

        public override void InitOffsetIndex(Vector2 offset) {
            for(var i=0; i<imageLength; i++) {
                if (offsets[i] == offset) {
                    frameNo = i;
                    return;
                }
            }
            frameNo = 0;
        }
    }

    /// <summary>
    /// UVスクロール用テクスチャ
    /// </summary>
    public class ScrollTex : AnimTex {
        public Vector2 scrollRatio;
        private Vector2 offset;
        private readonly bool zeroX = true;
        private readonly bool zeroY = true;
        public ScrollTex(Vector2 scroll, Texture tex, float frameSec)
            : base(frameSec) {
            scrollRatio = scroll;
            zeroX = Math.Abs(scroll.x) < 0.0000001f;
            zeroY = Math.Abs(scroll.y) < 0.0000001f;
            Tex = tex;
            this.tex.wrapMode = TextureWrapMode.Repeat;
        }

        public override Vector2 nextOffset() {
            if (!zeroX) offset.x = Mathf.Repeat(offset.x + scrollRatio.x, 1);
            if (!zeroY) offset.y = Mathf.Repeat(offset.y + scrollRatio.y, 1);
            return offset;
        }
        public override void InitOffsetIndex(Vector2 offset1) {
            offset = offset1;
        }
    }

    public class TexProp {
        public static readonly TexProp MainTex = new TexProp("_MainTex", "_MainAnime");
        public static readonly TexProp ShadowTex = new TexProp("_ShadowTex", "_ShadowAnime");
        public TexProp(string prop, string subPrefix) {
            Prop = prop;
            PropId = Shader.PropertyToID(prop);
            PropFPSId = Shader.PropertyToID(prop + "FPS");
            PropScrollXId = Shader.PropertyToID(subPrefix + "ScrollX");
            PropScrollYId = Shader.PropertyToID(subPrefix + "ScrollY");
        }
        public string Prop { get; private set; }
        public int PropId { get; private set; }
        public int PropFPSId { get; private set; }
        public int PropScrollXId { get; private set; }
        public int PropScrollYId { get; private set; }

        public override string ToString() {
            return Prop;
        }
    }
}
