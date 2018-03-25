using System;
using System.Collections.Generic;
using System.IO;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Data {
    /// <summary>
    /// ファイル、ファイル名に関する定数を扱うクラス.
    /// </summary>
    public static class FileConst {
        #region Constants
        public const string HEAD_MENU  = "CM3D2_MENU";
        public const string HEAD_MOD   = "CM3D2_MOD";
        public const string HEAD_MODEL = "CM3D2_MESH";
        public const string HEAD_MATE  = "CM3D2_MATERIAL";
        public const string HEAD_TEX   = "CM3D2_TEX";
        public const string HEAD_PMAT  = "CM3D2_PMATERIAL";

        public const string RET = "《改行》";

        public const string EXT_MOD      = ".mod";
        public const string EXT_MENU     = ".menu";
        public const string EXT_MATERIAL = ".mate";
        public const string EXT_PMAT     = ".pmat";
        public const string EXT_MODEL    = ".model";
        public const string EXT_TEXTURE  = ".tex";
        public const string EXT_TXT      = ".txt";
        public const string EXT_JSON     = ".json";

        #endregion
        private static readonly char [] INVALID_FILENAMECHARS = Path.GetInvalidFileNameChars();
        private static readonly Settings settings = Settings.Instance;

        public static bool HasInvalidChars(string filename) {
            return filename.IndexOfAny(INVALID_FILENAMECHARS) > -1;
        }

        public static readonly Dictionary<string, string> SuffixDic = new Dictionary<string, string>() {
            {"パンツずらし",     "_zurashi"},
            {"めくれスカート",    "_mekure"},
            {"めくれスカート後ろ", "_mekure_back"},
            {"半脱ぎ",        "_mekure_nugi"},
        };
        public static int SuffixUnknownCount;

        public static string GetResSuffix(string key) {
            string suffix;
            if (SuffixDic.TryGetValue(key, out suffix)) return suffix;
            suffix = settings.resSuffix + (++SuffixUnknownCount);
            SuffixDic[key] = suffix;
            return suffix;
        }

        public static readonly Dictionary<string, string> TexSuffix = 
            new Dictionary<string, string>() {
            {"_MainTex",       ""},
            {"_ToonRamp",       "_toon"},
            {"_ShadowTex",      "_shadow"},
            {"_ShadowRateToon", "_rate"},
            {"_HiTex", "_s"},
        };

        public static string GetTexSuffix(string propName) {
            string suffix;
            return TexSuffix.TryGetValue(propName, out suffix) ? suffix : "";
        }

        public static string GetModelSuffix(string propName) {
            string suffix;
            return modelSuffix.TryGetValue(propName, out suffix) ? suffix : "";
        }

        private static readonly Dictionary<string, string> modelSuffix = new Dictionary<string, string> {
            {TBody.SlotID.body.ToString(),      "_body"},
            {TBody.SlotID.head.ToString(),      "_head"},
            {TBody.SlotID.eye.ToString(),       "_eye"},
            {TBody.SlotID.chikubi.ToString(),   "_chikubi"},
            {TBody.SlotID.accHa.ToString(),     "_accha"},
            {TBody.SlotID.hairF.ToString(),     "_hairf"},
            {TBody.SlotID.hairR.ToString(),     "_hairr"},
            {TBody.SlotID.hairS.ToString(),     "_hairs"},
            {TBody.SlotID.hairT.ToString(),     "_hairt"},
            {TBody.SlotID.hairAho.ToString(),   "_haira"},
            {TBody.SlotID.underhair.ToString(),   "_underh"},
            {TBody.SlotID.accHat.ToString(),      "_acchat"},
            {TBody.SlotID.headset.ToString(),     "_headset"},
            {TBody.SlotID.wear.ToString(),        "_wear"},
            {TBody.SlotID.skirt.ToString(),       "_skirt"},
            {TBody.SlotID.onepiece.ToString(),    "_onep"},
            {TBody.SlotID.mizugi.ToString(),      "_mizugi"},
            {TBody.SlotID.bra.ToString(),         "_bra"},
            {TBody.SlotID.panz.ToString(),        "_panz"},
            {TBody.SlotID.stkg.ToString(),        "_stkg"},
            {TBody.SlotID.shoes.ToString(),       "_shoe"},
            {TBody.SlotID.accKami_1_.ToString(),  "_acckami1"},
            {TBody.SlotID.accKami_2_.ToString(),  "_acckami2"},
            {TBody.SlotID.accKami_3_.ToString(),  "_acckami3"},
            {TBody.SlotID.megane.ToString(),      "_megane"},
            {TBody.SlotID.accHead.ToString(),     "_acchead"},
            {TBody.SlotID.glove.ToString(),       "_glove"},
            {TBody.SlotID.accHana.ToString(),     "_acchana"},
            {TBody.SlotID.accMiMiL.ToString(),    "_accmimil"},
            {TBody.SlotID.accMiMiR.ToString(),    "_accmimir"},
            {TBody.SlotID.accKubi.ToString(),     "_acckubi"},
            {TBody.SlotID.accKubiwa.ToString(),   "_acckubiwa"},
            {TBody.SlotID.accKamiSubL.ToString(), "_acckamisl"},
            {TBody.SlotID.accKamiSubR.ToString(), "_acckamisr"},
            {TBody.SlotID.accNipL.ToString(),     "_accnipl"},
            {TBody.SlotID.accNipR.ToString(),     "_accnipr"},
            {TBody.SlotID.accUde.ToString(),      "_accude"},
            {TBody.SlotID.accHeso.ToString(),     "_accheso"},
            {TBody.SlotID.accAshi.ToString(),     "_accashi"},
            {TBody.SlotID.accSenaka.ToString(),   "_accsenaka"},
            {TBody.SlotID.accShippo.ToString(),   "_accshippo"},
            {TBody.SlotID.accXXX.ToString(),      "_accxxx"},
            {TBody.SlotID.seieki_naka.ToString(), "_snaka"},
            {TBody.SlotID.seieki_hara.ToString(), "_shara"},
            {TBody.SlotID.seieki_face.ToString(), "_sface"},
            {TBody.SlotID.seieki_mune.ToString(), "_smune"},
            {TBody.SlotID.seieki_hip.ToString(),  "_ship"},
            {TBody.SlotID.seieki_ude.ToString(),  "_sude"},
            {TBody.SlotID.seieki_ashi.ToString(), "_sashi"},
            {TBody.SlotID.HandItemL.ToString(),   "_handl"},
            {TBody.SlotID.HandItemR.ToString(),   "_handr"},

            {TBody.SlotID.kubiwa.ToString(),      "_kubiwa"},
            {TBody.SlotID.kousoku_upper.ToString(), "_kousokuu"},
            {TBody.SlotID.kousoku_lower.ToString(), "_kousokul"},
            {TBody.SlotID.accAnl.ToString(),      "_accanl"},
            {TBody.SlotID.accVag.ToString(),      "_accvag"},
            {TBody.SlotID.chinko.ToString(),      "_chinko"},
        };
    }
}
