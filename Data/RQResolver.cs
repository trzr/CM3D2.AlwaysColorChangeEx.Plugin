
using System;
using System.Collections.Generic;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Data {
    /// <summary>
    /// Description of RQResolver.
    /// </summary>
    public class RQResolver {
        private static readonly RQResolver INSTANCE = new RQResolver();
        public static RQResolver Instance {
            get { return INSTANCE; }
        }

        private readonly float[][] _slotRq;
        private static readonly float[] EMPTY_RQ = new float[0];
        public RQResolver() {
            _slotRq = new[] {
                new [] { 970f }, // body (skin)
                new [] { 970f, 990f }, // head (face:970, mayu:980)
                EMPTY_RQ, // eye
                new [] { 980f }, // hairF,
                EMPTY_RQ, // hairR,
                EMPTY_RQ, // hairS,
                EMPTY_RQ, // hairT,
                new [] { 3171f }, // wear,
                new [] { 3101f }, // skirt,
                new [] { 3171f }, // onepiece,
                new [] { 3091f }, // mizugi,
                new [] { 3021f }, // panz,
                new [] { 3131f }, // bra,
                new [] { 3061f }, // stkg,
                new [] { 3071f }, // shoes,
                new [] { 3251f }, // headset,
                new [] { 3141f }, // glove,
                new [] { 3221f }, // accHead,
                EMPTY_RQ, // hairAho,
                EMPTY_RQ, // accHana,
                new [] { 3211f }, // accHa,
                new [] { 3261f }, // accKami_1_,
                new [] { 3201f }, // accMiMiR,
                new [] { 3271f }, // accKamiSubR,
                new [] { 3136f, 3121f, }, // accNipR,
                new [] { 3281f }, // HandItemR,
                new [] { 3181f }, // accKubi,
                new [] { 3191f }, // accKubiwa,
                new [] { 3051f }, // accHeso,
                new [] { 3151f }, // accUde,
                new [] { 3081f }, // accAshi,
                new [] { 3161f,3176f }, // accSenaka,
                new [] { 3111f }, // accShippo,
                new [] { 3041f }, // accAnl,
                new [] { 3031f }, // accVag,
                new [] { 2898f }, // kubiwa,
                new [] { 3231f }, // megane,
                EMPTY_RQ, // accXXX,
                EMPTY_RQ, // chinko,
                new [] { 3010f }, // chikubi,
                new [] { 3241f }, // accHat,
                new [] { 3301f }, // kousoku_upper,
                new [] { 3301f }, // kousoku_lower,
                new [] { 3015f }, // seieki_naka,
                new [] { 3015f }, // seieki_hara,
                new [] { 3015f }, // seieki_face,
                new [] { 3015f }, // seieki_mune,
                new [] { 3015f }, // seieki_hip,
                new [] { 3015f }, // seieki_ude,
                new [] { 3015f }, // seieki_ashi,
                new [] { 3136f, 3121f, }, // accNipL,
                new [] { 3201f }, // accMiMiL,
                new [] { 3271f }, // accKamiSubL,
                new [] { 3261f }, // accKami_2_,
                new [] { 3261f }, // accKami_3_,
                new [] { 3281f }, // HandItemL,
                new [] { 3005f }, // underhair,
                EMPTY_RQ, // moza,
            };
        }
        public float[] Resolve(int slotId) {
            return _slotRq.Length >= slotId ? _slotRq[slotId] : EMPTY_RQ;
        }
    }
}
