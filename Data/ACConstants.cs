using System;
using System.Collections.Generic;
using System.Linq;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Data
{
    /// <summary>
    /// Description of SingletonClass1.
    /// </summary>
    public static class ACConstants
    {
        public static readonly Dictionary<TBody.SlotID, SlotInfo> SlotNames = new Dictionary<TBody.SlotID, SlotInfo>(Enum.GetNames(typeof(TBody.SlotID)).Length) {
            {TBody.SlotID.body,        new SlotInfo(TBody.SlotID.body,      MPN.body,      "身体", false) },
            {TBody.SlotID.head,        new SlotInfo(TBody.SlotID.head,      MPN.head,      "頭", false)},
            {TBody.SlotID.eye,         new SlotInfo(TBody.SlotID.eye,       MPN.eye,       "目", false)},
            {TBody.SlotID.chikubi,     new SlotInfo(TBody.SlotID.chikubi,   MPN.chikubi,   "乳首", true)},       
            {TBody.SlotID.accHa,       new SlotInfo(TBody.SlotID.accHa,     MPN.accha,     "歯", true)},
            {TBody.SlotID.hairF,       new SlotInfo(TBody.SlotID.hairF,     MPN.hairf,     "前髪", true)},
            {TBody.SlotID.hairR,       new SlotInfo(TBody.SlotID.hairR,     MPN.hairr,     "後髪", true)},
            {TBody.SlotID.hairS,       new SlotInfo(TBody.SlotID.hairS,     MPN.hairs,     "横髪", true)},
            {TBody.SlotID.hairT,       new SlotInfo(TBody.SlotID.hairT,     MPN.hairt,     "エクステ毛", true)},
            {TBody.SlotID.hairAho,     new SlotInfo(TBody.SlotID.hairAho,   MPN.hairaho,   "アホ毛", true)},
            {TBody.SlotID.underhair,   new SlotInfo(TBody.SlotID.underhair, MPN.underhair, "アンダーヘア", true)},
            {TBody.SlotID.accHat,      new SlotInfo(TBody.SlotID.accHat,    MPN.acchat,    "帽子", true)},
            {TBody.SlotID.headset,     new SlotInfo(TBody.SlotID.headset,   MPN.headset,   "ヘッドドレス", true)},
            {TBody.SlotID.wear,        new SlotInfo(TBody.SlotID.wear,      MPN.wear,      "トップス", true)},
            {TBody.SlotID.skirt,       new SlotInfo(TBody.SlotID.skirt,     MPN.skirt,     "ボトムス", true)},
            {TBody.SlotID.onepiece,    new SlotInfo(TBody.SlotID.onepiece,  MPN.onepiece,  "ワンピース", true)},
            {TBody.SlotID.mizugi,      new SlotInfo(TBody.SlotID.mizugi,    MPN.mizugi,    "水着", true)},
            {TBody.SlotID.bra,         new SlotInfo(TBody.SlotID.bra,       MPN.bra,       "ブラジャー", true)},
            {TBody.SlotID.panz,        new SlotInfo(TBody.SlotID.panz,      MPN.panz,      "パンツ", true)},
            {TBody.SlotID.stkg,        new SlotInfo(TBody.SlotID.stkg,      MPN.stkg,      "靴下", true)},
            {TBody.SlotID.shoes,       new SlotInfo(TBody.SlotID.shoes,     MPN.shoes,     "靴", true)},
            {TBody.SlotID.accKami_1_,  new SlotInfo(TBody.SlotID.accKami_1_, MPN.acckami,  "アクセ：前髪", true)},
            {TBody.SlotID.accKami_2_,  new SlotInfo(TBody.SlotID.accKami_2_, MPN.acckami,  "アクセ：前髪：左", true)},
            {TBody.SlotID.accKami_3_,  new SlotInfo(TBody.SlotID.accKami_3_, MPN.acckami,  "アクセ：前髪：右", true)},
            {TBody.SlotID.megane,      new SlotInfo(TBody.SlotID.megane,    MPN.megane,    "アクセ：メガネ", true)},
            {TBody.SlotID.accHead,     new SlotInfo(TBody.SlotID.accHead,   MPN.acchead,   "アクセ：アイマスク", true)},
            {TBody.SlotID.glove,       new SlotInfo(TBody.SlotID.glove,     MPN.glove,     "アクセ：手袋", true)},
            {TBody.SlotID.accHana,     new SlotInfo(TBody.SlotID.accHana,   MPN.acchana,   "アクセ：鼻", true)},
            {TBody.SlotID.accMiMiL,    new SlotInfo(TBody.SlotID.accMiMiL,  MPN.accmimi,   "アクセ：左耳", true)},
            {TBody.SlotID.accMiMiR,    new SlotInfo(TBody.SlotID.accMiMiR,  MPN.accmimi,   "アクセ：右耳", true)},
            {TBody.SlotID.accKubi,     new SlotInfo(TBody.SlotID.accKubi,   MPN.acckubi,   "アクセ：ネックレス", true)},
            {TBody.SlotID.accKubiwa,   new SlotInfo(TBody.SlotID.accKubiwa, MPN.acckubiwa, "アクセ：チョーカー", true)},
            {TBody.SlotID.accKamiSubL, new SlotInfo(TBody.SlotID.accKamiSubL, MPN.acckamisub,"アクセ：左リボン", true)},
            {TBody.SlotID.accKamiSubR, new SlotInfo(TBody.SlotID.accKamiSubR, MPN.acckamisub,"アクセ：右リボン", true)},
            {TBody.SlotID.accNipL,     new SlotInfo(TBody.SlotID.accNipL,   MPN.accnip,    "アクセ：左乳首", true)},
            {TBody.SlotID.accNipR,     new SlotInfo(TBody.SlotID.accNipR,   MPN.accnip,    "アクセ：右乳首", true)},
            {TBody.SlotID.accUde,      new SlotInfo(TBody.SlotID.accUde,    MPN.accude,    "アクセ：腕", true)},
            {TBody.SlotID.accHeso,     new SlotInfo(TBody.SlotID.accHeso,   MPN.accheso,   "アクセ：へそ", true)},
            {TBody.SlotID.accAshi,     new SlotInfo(TBody.SlotID.accAshi,   MPN.accashi,   "アクセ：足首", true)},
            {TBody.SlotID.accSenaka,   new SlotInfo(TBody.SlotID.accSenaka, MPN.accsenaka, "アクセ：背中", true)},
            {TBody.SlotID.accShippo,   new SlotInfo(TBody.SlotID.accShippo, MPN.accshippo, "アクセ：しっぽ", true)},
            {TBody.SlotID.accXXX,      new SlotInfo(TBody.SlotID.accXXX,    MPN.accxxx,    "アクセ：前穴", true)},
            {TBody.SlotID.seieki_naka, new SlotInfo(TBody.SlotID.seieki_naka, MPN.seieki_naka, "精液：中", true)},
            {TBody.SlotID.seieki_hara, new SlotInfo(TBody.SlotID.seieki_hara, MPN.seieki_hara, "精液：腹", true)},
            {TBody.SlotID.seieki_face, new SlotInfo(TBody.SlotID.seieki_face, MPN.seieki_face, "精液：顔", true)},
            {TBody.SlotID.seieki_mune, new SlotInfo(TBody.SlotID.seieki_mune, MPN.seieki_mune, "精液：胸", true)},
            {TBody.SlotID.seieki_hip,  new SlotInfo(TBody.SlotID.seieki_hip, MPN.seieki_hip,   "精液：尻", true)},
            {TBody.SlotID.seieki_ude,  new SlotInfo(TBody.SlotID.seieki_ude, MPN.seieki_ude,   "精液：腕", true)},
            {TBody.SlotID.seieki_ashi, new SlotInfo(TBody.SlotID.seieki_ashi, MPN.seieki_ashi, "精液：足", true)},
            {TBody.SlotID.HandItemL,   new SlotInfo(TBody.SlotID.HandItemL, MPN.handitem, "手持アイテム：左", true)},
            {TBody.SlotID.HandItemR,   new SlotInfo(TBody.SlotID.HandItemR, MPN.handitem, "手持アイテム：右", true)},

            {TBody.SlotID.kubiwa,      new SlotInfo(TBody.SlotID.kubiwa,    MPN.KubiScl,  "首輪", true)},
            {TBody.SlotID.kousoku_upper, new SlotInfo(TBody.SlotID.kousoku_upper, MPN.kousoku_upper, "拘束具：上", true)},
            {TBody.SlotID.kousoku_lower, new SlotInfo(TBody.SlotID.kousoku_lower, MPN.kousoku_lower, "拘束具：下", true)},
            {TBody.SlotID.accAnl,      new SlotInfo(TBody.SlotID.accAnl,    MPN.accanl,  "アナルバイブ", true)},
            {TBody.SlotID.accVag,      new SlotInfo(TBody.SlotID.accVag,    MPN.accvag,  "バイブ", true)},
            {TBody.SlotID.chinko,      new SlotInfo(TBody.SlotID.chinko,   MPN.null_mpn, "チ○コ", false)},
            //{TBody.SlotID.moza,        new SlotInfo(TBody.SlotID.moza,   MPN.moza, "モザ", true)},
        };
        public static readonly Dictionary<TBody.SlotID, TBody.SlotID> OppositeSlotNames = new Dictionary<TBody.SlotID, TBody.SlotID>() {
            {TBody.SlotID.accKami_2_,  TBody.SlotID.accKami_2_},  // アクセ：前髪：左
            {TBody.SlotID.accKami_3_,  TBody.SlotID.accKami_2_},  // アクセ：前髪：右
            {TBody.SlotID.accMiMiL,    TBody.SlotID.accMiMiR},    // アクセ：左耳
            {TBody.SlotID.accMiMiR,    TBody.SlotID.accMiMiL},    // アクセ：右耳
            {TBody.SlotID.accKamiSubL, TBody.SlotID.accKamiSubR}, // アクセ：左リボン
            {TBody.SlotID.accKamiSubR, TBody.SlotID.accKamiSubL}, // アクセ：右リボン
            {TBody.SlotID.accNipL,     TBody.SlotID.accNipR},     // アクセ：左乳首
            {TBody.SlotID.accNipR,     TBody.SlotID.accNipL},     // アクセ：右乳首
            {TBody.SlotID.HandItemL,   TBody.SlotID.HandItemR},   // 手持アイテム：左
            {TBody.SlotID.HandItemR,   TBody.SlotID.HandItemL},   // 手持アイテム：右
            //{TBody.SlotID.kousoku_upper, TBody.SlotID.kousoku_upper}, // 拘束具：上
            //{TBody.SlotID.kousoku_lower, TBody.SlotID.kousoku_upper}, // 拘束具：下
        };
        // マスク対象スロット
//        public static readonly Dictionary<int, SlotInfo> MaskSlots = new Dictionary<int, SlotInfo>(Enum.GetNames(typeof(TBody.SlotID)).Length);

        public static readonly Dictionary<string, string> NodeNames = new Dictionary<string, string>() {
            {"Bip01 Head", "頭" },
            {"Bip01 Neck_SCL_", "首"},
            {"Mune_L_sub", "左胸上"},
            {"Mune_L", "左胸下"},
            {"Mune_R_sub", "右胸上"},
            {"Mune_R", "右胸下"},
            {"Bip01 Pelvis_SCL_", "骨盤"},
            {"Bip01 Spine_SCL_", "脊椎"},
            {"Bip01 Spine1_SCL_", "腰中"},
            {"Bip01 Spine0a_SCL_", "腹部"},
            {"Bip01 Spine1a_SCL_", "胸部"},
            {"Bip01", "股間"},
            {"Hip_L", "左尻"},
            {"Hip_R", "右尻"},
            {"momotwist_L", "左前腿"},
            {"momoniku_L", "左後腿"},
            {"momotwist2_L", "左前腿下部"},
            {"Bip01 L Thigh_SCL_", "左ふくらはぎ"},
            {"Bip01 L Calf_SCL_", "左足首"},
            {"Bip01 L Toe0", "左足小指付け根"},
            {"Bip01 L Toe01", "左足小指先"},
            {"Bip01 L Toe1", "左足中指付け根"},
            {"Bip01 L Toe11", "左足中指先"},
            {"Bip01 L Toe2", "左足親指付け根"},
            {"Bip01 L Toe21", "左足親指先"},
            {"momotwist_R", "右前腿"},
            {"momoniku_R", "右後腿"},
            {"momotwist2_R", "右前腿下部"},
            {"Bip01 R Thigh_SCL_", "右ふくらはぎ"},
            {"Bip01 R Calf_SCL_", "右足首"},
            {"Bip01 R Toe0", "右足小指付け根"},
            {"Bip01 R Toe01", "右足小指先"},
            {"Bip01 R Toe1", "右足中指付け根"},
            {"Bip01 R Toe11", "右足中指先"},
            {"Bip01 R Toe2", "右足親指付け根"},
            {"Bip01 R Toe21", "右足親指先"},
            {"Bip01 L Clavicle_SCL_", "左鎖骨"},
            {"Kata_L", "左肩"},
            {"Kata_L_nub", "左肩上腕"},
            {"Uppertwist_L", "左上腕A"},
            {"Uppertwist1_L", "左上腕B"},
            {"Bip01 L UpperArm", "左上腕"},
            {"Bip01 L Forearm", "左肘"},
            {"Foretwist1_L", "左前腕"},
            {"Foretwist_L", "左手首"},
            {"Bip01 L Hand", "左手"},
            {"Bip01 L Finger0", "左親指付け根"},
            {"Bip01 L Finger01", "左親指関節"},
            {"Bip01 L Finger02", "左親指先"},
            {"Bip01 L Finger1", "左人指し指付け根"},
            {"Bip01 L Finger11", "左人指し指関節"},
            {"Bip01 L Finger12", "左人指し指先"},
            {"Bip01 L Finger2", "左中指付け根"},
            {"Bip01 L Finger21", "左中指関節"},
            {"Bip01 L Finger22", "左中指先"},
            {"Bip01 L Finger3", "左薬指付け根"},
            {"Bip01 L Finger31", "左薬指関節"},
            {"Bip01 L Finger32", "左薬指先"},
            {"Bip01 L Finger4", "左小指付け根"},
            {"Bip01 L Finger41", "左小指関節"},
            {"Bip01 L Finger42", "左小指先"},
            {"Bip01 R Clavicle_SCL_", "右鎖骨"},
            {"Kata_R", "右肩"},
            {"Kata_R_nub", "右肩上腕"},
            {"Uppertwist_R", "右上腕A"},
            {"Uppertwist1_R", "右上腕B"},
            {"Bip01 R UpperArm", "右上腕"},
            {"Bip01 R Forearm", "右肘"},
            {"Foretwist1_R", "右前腕"},
            {"Foretwist_R", "右手首"},
            {"Bip01 R Hand", "右手"},
            {"Bip01 R Finger0", "右親指付け根"},
            {"Bip01 R Finger01", "右親指関節"},
            {"Bip01 R Finger02", "右親指先"},
            {"Bip01 R Finger1", "右人指し指付け根"},
            {"Bip01 R Finger11", "右人指し指関節"},
            {"Bip01 R Finger12", "右人指し指先"},
            {"Bip01 R Finger2", "右中指付け根"},
            {"Bip01 R Finger21", "右中指関節"},
            {"Bip01 R Finger22", "右中指先"},
            {"Bip01 R Finger3", "右薬指付け根"},
            {"Bip01 R Finger31", "右薬指関節"},
            {"Bip01 R Finger32", "右薬指先"},
            {"Bip01 R Finger4", "右小指付け根"},
            {"Bip01 R Finger41", "右小指関節"},
            {"Bip01 R Finger42", "右小指先"} 
        };

        // 以下旧版用
        [System.Obsolete("use SlotNames")]
        public static readonly Dictionary<string, TBody.SlotID> Slotnames = new Dictionary<string, TBody.SlotID>() {
            {"身体", TBody.SlotID.body},
            {"乳首", TBody.SlotID.chikubi},        
            {"アンダーヘア", TBody.SlotID.underhair},
            {"頭", TBody.SlotID.head},
            {"目", TBody.SlotID.eye},
            {"歯", TBody.SlotID.accHa},
            {"前髪", TBody.SlotID.hairF},
            {"後髪", TBody.SlotID.hairR},
            {"横髪", TBody.SlotID.hairS},
            {"エクステ毛", TBody.SlotID.hairT},
            {"アホ毛", TBody.SlotID.hairAho},
            {"帽子", TBody.SlotID.accHat},
            {"ヘッドドレス", TBody.SlotID.headset},
            {"トップス", TBody.SlotID.wear},
            {"ボトムス", TBody.SlotID.skirt},
            {"ワンピース", TBody.SlotID.onepiece},
            {"水着", TBody.SlotID.mizugi},
            {"ブラジャー", TBody.SlotID.bra},
            {"パンツ", TBody.SlotID.panz},
            {"靴下", TBody.SlotID.stkg},
            {"靴", TBody.SlotID.shoes},
            {"アクセ：前髪", TBody.SlotID.accKami_1_},
            {"アクセ：前髪：左", TBody.SlotID.accKami_2_},
            {"アクセ：前髪：右", TBody.SlotID.accKami_3_},
            {"アクセ：メガネ", TBody.SlotID.megane},
            {"アクセ：アイマスク", TBody.SlotID.accHead},
            {"アクセ：手袋", TBody.SlotID.glove},
            {"アクセ：鼻", TBody.SlotID.accHana},
            {"アクセ：左耳", TBody.SlotID.accMiMiL},
            {"アクセ：右耳", TBody.SlotID.accMiMiR},
            {"アクセ：ネックレス", TBody.SlotID.accKubi},
            {"アクセ：チョーカー", TBody.SlotID.accKubiwa},
            {"アクセ：左リボン", TBody.SlotID.accKamiSubL},
            {"アクセ：右リボン", TBody.SlotID.accKamiSubR},
            {"アクセ：左乳首", TBody.SlotID.accNipL},
            {"アクセ：右乳首", TBody.SlotID.accNipR},
            {"アクセ：腕", TBody.SlotID.accUde},
            {"アクセ：へそ", TBody.SlotID.accHeso},
            {"アクセ足首", TBody.SlotID.accAshi},
            {"アクセ：背中", TBody.SlotID.accSenaka},
            {"アクセ：しっぽ", TBody.SlotID.accShippo},
            {"アクセ：前穴", TBody.SlotID.accXXX},
            {"精液：中", TBody.SlotID.seieki_naka},
            {"精液：腹", TBody.SlotID.seieki_hara},
            {"精液：顔", TBody.SlotID.seieki_face},
            {"精液：胸", TBody.SlotID.seieki_mune},
            {"精液：尻", TBody.SlotID.seieki_hip},
            {"精液：腕", TBody.SlotID.seieki_ude},
            {"精液：足", TBody.SlotID.seieki_ashi},
            {"手持アイテム：左", TBody.SlotID.HandItemL},
            {"手持ちアイテム：右", TBody.SlotID.HandItemR},
            {"首輪？", TBody.SlotID.kubiwa},
            {"拘束具：上？", TBody.SlotID.kousoku_upper},
            {"拘束具：下？", TBody.SlotID.kousoku_lower},
            {"アナルバイブ？", TBody.SlotID.accAnl},
            {"バイブ？", TBody.SlotID.accVag},
            {"チ○コ", TBody.SlotID.chinko}
        };
        [System.Obsolete("use NodeNames")]
        public static readonly Dictionary<string, string> Nodenames = new Dictionary<string, string>() {
            {"頭", "Bip01 Head"},
            {"首", "Bip01 Neck_SCL_"},
            {"左胸上", "Mune_L_sub"},
            {"左胸下", "Mune_L"},
            {"右胸上", "Mune_R_sub"},
            {"右胸下", "Mune_R"},
            {"骨盤", "Bip01 Pelvis_SCL_"},
            {"脊椎", "Bip01 Spine_SCL_"},
            {"腰中", "Bip01 Spine1_SCL_"},
            {"腹部", "Bip01 Spine0a_SCL_"},
            {"胸部", "Bip01 Spine1a_SCL_"},
            {"股間", "Bip01"},
            {"左尻", "Hip_L"},
            {"右尻", "Hip_R"},
            {"左前腿", "momotwist_L"},
            {"左後腿", "momoniku_L"},
            {"左前腿下部", "momotwist2_L"},
            {"左ふくらはぎ", "Bip01 L Thigh_SCL_"},
            {"左足首", "Bip01 L Calf_SCL_"},
            {"左足小指付け根", "Bip01 L Toe0"},
            {"左足小指先", "Bip01 L Toe01"},
            {"左足中指付け根", "Bip01 L Toe1"},
            {"左足中指先", "Bip01 L Toe11"},
            {"左足親指付け根", "Bip01 L Toe2"},
            {"左足親指先", "Bip01 L Toe21"},
            {"右前腿", "momotwist_R"},
            {"右後腿", "momoniku_R"},
            {"右前腿下部", "momotwist2_R"},
            {"右ふくらはぎ", "Bip01 R Thigh_SCL_"},
            {"右足首", "Bip01 R Calf_SCL_"},
            {"右足小指付け根", "Bip01 R Toe0"},
            {"右足小指先", "Bip01 R Toe01"},
            {"右足中指付け根", "Bip01 R Toe1"},
            {"右足中指先", "Bip01 R Toe11"},
            {"右足親指付け根", "Bip01 R Toe2"},
            {"右足親指先", "Bip01 R Toe21"},
            {"左鎖骨", "Bip01 L Clavicle_SCL_"},
            {"左肩", "Kata_L"},
            {"左肩上腕", "Kata_L_nub"},
            {"左上腕A", "Uppertwist_L"},
            {"左上腕B", "Uppertwist1_L"},
            {"左上腕", "Bip01 L UpperArm"},
            {"左肘", "Bip01 L Forearm"},
            {"左前腕", "Foretwist1_L"},
            {"左手首", "Foretwist_L"},
            {"左手", "Bip01 L Hand"},
            {"左親指付け根", "Bip01 L Finger0"},
            {"左親指関節", "Bip01 L Finger01"},
            {"左親指先", "Bip01 L Finger02"},
            {"左人指し指付け根", "Bip01 L Finger1"},
            {"左人指し指関節", "Bip01 L Finger11"},
            {"左人指し指先", "Bip01 L Finger12"},
            {"左中指付け根", "Bip01 L Finger2"},
            {"左中指関節", "Bip01 L Finger21"},
            {"左中指先", "Bip01 L Finger22"},
            {"左薬指付け根", "Bip01 L Finger3"},
            {"左薬指関節", "Bip01 L Finger31"},
            {"左薬指先", "Bip01 L Finger32"},
            {"左小指付け根", "Bip01 L Finger4"},
            {"左小指関節", "Bip01 L Finger41"},
            {"左小指先", "Bip01 L Finger42"},
            {"右鎖骨", "Bip01 R Clavicle_SCL_"},
            {"右肩", "Kata_R"},
            {"右肩上腕", "Kata_R_nub"},
            {"右上腕A", "Uppertwist_R"},
            {"右上腕B", "Uppertwist1_R"},
            {"右上腕", "Bip01 R UpperArm"},
            {"右肘", "Bip01 R Forearm"},
            {"右前腕", "Foretwist1_R"},
            {"右手首", "Foretwist_R"},
            {"右手", "Bip01 R Hand"},
            {"右親指付け根", "Bip01 R Finger0"},
            {"右親指関節", "Bip01 R Finger01"},
            {"右親指先", "Bip01 R Finger02"},
            {"右人指し指付け根", "Bip01 R Finger1"},
            {"右人指し指関節", "Bip01 R Finger11"},
            {"右人指し指先", "Bip01 R Finger12"},
            {"右中指付け根", "Bip01 R Finger2"},
            {"右中指関節", "Bip01 R Finger21"},
            {"右中指先", "Bip01 R Finger22"},
            {"右薬指付け根", "Bip01 R Finger3"},
            {"右薬指関節", "Bip01 R Finger31"},
            {"右薬指先", "Bip01 R Finger32"},
            {"右小指付け根", "Bip01 R Finger4"},
            {"右小指関節", "Bip01 R Finger41"},
            {"右小指先", "Bip01 R Finger42"} 
        };
    }
    public class SlotInfo {
        public TBody.SlotID Id        { get; private set; }
        public MPN mpn                { get; private set; }
        public string Name            { get; private set; }
        public string DisplayName     { get; private set; }
        public string LongName        { get; private set; }
        public bool   maskable        { get; private set; }
        private int no;
        public int No {
            get {
                if (no == -1) {
                    try {
                        this.no = (int)TBody.hashSlotName[Name];
                    } catch(Exception e) {
                        LogUtil.Log("Initialize Error Slot name is illegal", Name, e);
                    }
                }
                return no;
            }
            private set { no = value; }
        }

        public SlotInfo(TBody.SlotID id, MPN mpn, string displayName, bool mask) {
            this.Id = id;
            this.Name = Id.ToString();
            this.mpn = mpn;
            this.DisplayName = displayName;
            this.LongName = displayName + " [" + Name + "]";
            this.no = -1;
            this.maskable = mask;
        }
    }

}
