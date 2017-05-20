
using System;
using System.Collections.Generic;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Data
{
    /// <summary>
    /// Description of RQResolver.
    /// </summary>
    public class RQResolver
    {
        private readonly static RQResolver instance = new RQResolver();
        public static RQResolver Instance {
            get { return instance; }
        }

        private readonly float[][] slotRQ;
        private static readonly float[] EMPTY_RQ = new float[0];
        public RQResolver()
        {
            slotRQ = new float[][] {
                EMPTY_RQ, // body
                EMPTY_RQ, // head
                EMPTY_RQ, // eye
                EMPTY_RQ, // hairF,
                EMPTY_RQ, // hairR,
                EMPTY_RQ, // hairS,
                EMPTY_RQ, // hairT,
                new float[] { 3171 }, // wear,
                new float[] { 3101 }, // skirt,
                new float[] { 3171 }, // onepiece,
                new float[] { 3091 }, // mizugi,
                new float[] { 3021 }, // panz,
                new float[] { 3131 }, // bra,
                new float[] { 3061 }, // stkg,
                new float[] { 3071 }, // shoes,
                new float[] { 3251 }, // headset,
                new float[] { 3141 }, // glove,
                new float[] { 3221 }, // accHead,
                EMPTY_RQ, // hairAho,
                EMPTY_RQ, // accHana,
                new float[] { 3211 }, // accHa,
                new float[] { 3261 }, // accKami_1_,
                new float[] { 3201 }, // accMiMiR,
                new float[] { 3271 }, // accKamiSubR,
                new float[] { 3121, 3136, }, // accNipR,
                new float[] { 3281 }, // HandItemR,
                new float[] { 3181 }, // accKubi,
                new float[] { 3191 }, // accKubiwa,
                new float[] { 3051 }, // accHeso,
                new float[] { 3151 }, // accUde,
                new float[] { 3081 }, // accAshi,
                new float[] { 3161,3176 }, // accSenaka,
                new float[] { 3111 }, // accShippo,
                new float[] { 3041 }, // accAnl,
                new float[] { 3031 }, // accVag,
                EMPTY_RQ, // kubiwa,
                new float[] { 3231 }, // megane,
                EMPTY_RQ, // accXXX,
                EMPTY_RQ, // chinko,
                new float[] { 3010 }, // chikubi,
                new float[] { 3241 }, // accHat,
                new float[] { 3301 }, // kousoku_upper,
                new float[] { 3301 }, // kousoku_lower,
                new float[] { 3015 }, // seieki_naka,
                new float[] { 3015 }, // seieki_hara,
                new float[] { 3015 }, // seieki_face,
                new float[] { 3015 }, // seieki_mune,
                new float[] { 3015 }, // seieki_hip,
                new float[] { 3015 }, // seieki_ude,
                new float[] { 3015 }, // seieki_ashi,
                new float[] { 3121, 3136, }, // accNipL,
                new float[] { 3201 }, // accMiMiL,
                new float[] { 3271 }, // accKamiSubL,
                new float[] { 3261 }, // accKami_2_,
                new float[] { 3261 }, // accKami_3_,
                new float[] { 3281 }, // HandItemL,
                new float[] { 3005 }, // underhair,
                EMPTY_RQ, // moza,
            };
        }
        public float[] resolve(int slotId) {
            return slotRQ.Length >= slotId ? slotRQ[slotId] : EMPTY_RQ;
        }
    }
}
