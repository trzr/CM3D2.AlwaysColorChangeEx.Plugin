/*
 * 
 * このテンプレートを変更する場合「ツール→オプション→コーディング→標準ヘッダの編集」
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UnityEngine;

namespace CM3D2.AlwaysColorChange.Plugin
{
    /// <summary>
    /// Description of ShaderMapper.
    /// シェーダと
    /// </summary>
    public static class ShaderMapper
    {

        public static readonly Dictionary<string, string> Slotnames = new Dictionary<string, string>() {
            {"身体", "body"},
            {"乳首", "chikubi"},                        
            {"アンダーヘア", "underhair"},
            {"頭", "head"},
            {"目", "eye"},
            {"歯", "accHa"},
            {"前髪", "hairF"},
            {"後髪", "hairR"},
            {"横髪", "hairS"},
            {"エクステ毛", "hairT"},
            {"アホ毛", "hairAho"},
            {"帽子", "accHat"},
            {"ヘッドドレス", "headset"},
            {"トップス", "wear"},
            {"ボトムス", "skirt"},
            {"ワンピース", "onepiece"},
            {"水着", "mizugi"},
            {"ブラジャー", "bra"},
            {"パンツ", "panz"},
            {"靴下", "stkg"},
            {"靴", "shoes"},
            {"アクセ：前髪", "accKami_1_"},
            {"アクセ：前髪：左", "accKami_2_"},
            {"アクセ：前髪：右", "accKami_3_"},
            {"アクセ：メガネ", "megane"},
            {"アクセ：アイマスク", "accHead"},
            {"アクセ：手袋", "glove"},
            {"アクセ：鼻", "accHana"},
            {"アクセ：左耳", "accMiMiL"},
            {"アクセ：右耳", "accMiMiR"},
            {"アクセ：ネックレス", "accKubi"},
            {"アクセ：チョーカー", "accKubiwa"},
            {"アクセ：（左）リボン", "accKamiSubL"},
            {"アクセ：右リボン", "accKamiSubR"},
            {"アクセ：左乳首", "accNipL"},
            {"アクセ：右乳首", "accNipR"},
            {"アクセ：腕", "accUde"},
            {"アクセ：へそ", "accHeso"},
            {"アクセ足首", "accAshi"},
            {"アクセ：背中", "accSenaka"},
            {"アクセ：しっぽ", "accShippo"},
            {"アクセ：前穴", "accXXX"},
            {"精液：中", "seieki_naka"},
            {"精液：腹", "seieki_hara"},
            {"精液：顔", "seieki_face"},
            {"精液：胸", "seieki_mune"},
            {"精液：尻", "seieki_hip"},
            {"精液：腕", "seieki_ude"},
            {"精液：足", "seieki_ashi"},
            {"手持アイテム：左", "HandItemL"},
            {"手持ちアイテム：右", "HandItemR"},
            {"首輪？", "kubiwa"},
            {"拘束具：上？", "kousoku_upper"},
            {"拘束具：下？", "kousoku_lower"},
            {"アナルバイブ？", "accAnl"},
            {"バイブ？", "accVag"},
            {"チ○コ", "chinko"}
        };
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
        public static readonly Dictionary<string, string> shaderNameMap = new Dictionary<string, string> {
            {"CM3D2/Man",    "ご主人様" },
            {"CM3D2/Mosaic", "モザイク" },
            {"CM3D2/Lighted", "非トゥーン" },
            {"CM3D2/Lighted_Trans", "透過" },
            {"CM3D2/Toony_Lighted", "トゥーン" },
            {"CM3D2/Toony_Lighted_Trans", "トゥーン 透過" },
            {"CM3D2/Toony_Lighted_Trans_NoZ", "トゥーン 透過 NoZ" },
            {"CM3D2/Toony_Lighted_Outline", "トゥーン 輪郭線" },
            {"CM3D2/Toony_Lighted_Outline_Trans", "トゥーン 輪郭線 透過" },
            {"CM3D2/Toony_Lighted_Outline_Trans_NoZ", "トゥーン 輪郭線 透過 NoZ" },
            {"CM3D2/Toony_Lighted_Hair", "トゥーン 髪" },
            {"CM3D2/Toony_Lighted_Hair_Outline", "トゥーン 髪 輪郭線" },
            {"Unlit/Texture", "発光" },
            {"Unlit/Transparent", "発光 透過" },
            {"Diffuse", "リアル" },
            {"Transparent/Diffuse",　"透過 リアル"}
        };

         public static string name(string shaderName) {
            try {
                return shaderNameMap[shaderName];
            } catch(KeyNotFoundException e) {
                return null;
            }
        }
        public static MaterialFlag resolve(string shaderName) {
            try {
                return shaderMap[shaderName];
            } catch(KeyNotFoundException e) {
                return null;
            }
        }

        //        string[] propNames = new string[] { "_MainTex", "_ShadowTex", "_ToonRamp", "_ShadowRateToon", "Alpha", "Multiply", "InfinityColor", "TexTo8bitTex", "Max" };
        public static readonly string[] PropNamesEmpty   = new string[] {  };
        public static readonly string[] PropNamesColored = new string[] { "_MainTex" };
        public static readonly string[] PropNames        = new string[] { "_MainTex", "_ToonRamp", "_ShadowTex", "_ShadowRateToon" };
        public static readonly string[] PropNamesHair    = new string[] { "_MainTex", "_ToonRamp", "_ShadowTex", "_ShadowRateToon", "_HiTex" };

        // Toony/Lighted_Trans
        public readonly static MaterialFlag ToonyLightedTrans         = new MaterialFlag(PropNames, true, true, true, false, true, false);
        public readonly static MaterialFlag ToonyLightedOutlinedTrans = new MaterialFlag(PropNames, true, true, true, true, true, false);
        public readonly static MaterialFlag ToonyLightedOutlined      = new MaterialFlag(PropNames, true, true, true, true, false, false);

        private readonly static Dictionary<string, MaterialFlag> shaderMap = new Dictionary<string, MaterialFlag>(16) {
            {"CM3D2/Man",                             new MaterialFlag(PropNamesEmpty, true, true, false, false, false, false)},
            {"CM3D2/Mosaic",                          new MaterialFlag(PropNamesEmpty, true, true, false, false, false, false)},
            {"CM3D2/Lighted",                         new MaterialFlag(PropNamesColored, true, true, false, false, false, false)},
            {"CM3D2/Lighted_Trans",                   new MaterialFlag(PropNamesColored, true, true, false, false, true, false)},
            {"CM3D2/Toony_Lighted",                   new MaterialFlag(PropNamesColored, true, true, true, false, false, false)},
            {"CM3D2/Toony_Lighted_Trans",             ToonyLightedTrans},
            {"CM3D2/Toony_Lighted_Trans_NoZ",         ToonyLightedTrans},
            {"CM3D2/Toony_Lighted_Outline",           ToonyLightedOutlined},
            {"CM3D2/Toony_Lighted_Outline_Trans",     ToonyLightedOutlinedTrans},
            {"CM3D2/Toony_Lighted_Outline_Trans_NoZ", ToonyLightedOutlinedTrans},
            {"CM3D2/Toony_Lighted_Hair",              new MaterialFlag(PropNamesHair, true, true, true, false, false, true)},
            {"CM3D2/Toony_Lighted_Hair_Outline",      new MaterialFlag(PropNamesHair, true, true, true, true, false, true)},
            {"Unlit/Texture",                         new MaterialFlag(PropNamesColored, false, false, false, false, false, false)},
            {"Unlit/Transparent",                     new MaterialFlag(PropNamesColored, false, false, false, false, false, false)},
            {"Diffuse",                               new MaterialFlag(PropNamesColored, true, false, false, false, false, false)},
            {"Transparent/Diffuse",                   new MaterialFlag(PropNamesColored, true, false, false, false, true, false)},
        };




        public class MaterialFlag {
            public MaterialFlag(String[] propNames, bool hasColor, bool isLighted, bool isToony, bool isOutlined, bool isTrans, bool isHair) {
                this.propNames = propNames;
                this.hasColor   = hasColor;
                this.isLighted  = isLighted;
                this.isToony    = isToony;
                this.isOutlined = isOutlined;
                this.isHair     = isHair;
                this.isTrans    = isTrans;
                
            }
            public String[] propNames  { get; set; }
            public bool hasColor       { get; set; }
            public bool isLighted      { get; set; }
            public bool isOutlined     { get; set; }
            public bool isToony        { get; set; }
            public bool isHair         { get; set; }
            public bool isTrans        { get; set; }
        }
        
    }
}
