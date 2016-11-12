/*
 * シェーダ名のマッピング解決用静的クラス
 * シェーダ名と各シェーダの編集項目のフラグの対応付けを管理する
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;

#pragma warning disable 0168
namespace CM3D2.AlwaysColorChangeEx.Plugin.Util
{
    /// <summary>
    /// Description of ShaderMapper.
    /// シェーダ名とシェーダの対応付け、
    /// </summary>
    public static class ShaderMapper
    {
        public static ShaderName ParseShader(string name) {
            return Array.Find(ShaderNames, x => x.Name == name);
        }
        internal enum ShaderTypes {
            Toony_Lighted,
            Toony_Lighted_Trans,
            Toony_Lighted_Trans_NoZ,
            Toony_Lighted_Outline,
            Toony_Lighted_Outline_Trans,
            Toony_Lighted_Hair,
            Toony_Lighted_Hair_Outline,
            Lighted,
            Lighted_Trans,
            Unlit__Texture,
            Unlit__Transparent,
            Diffuse,
            Transparent__Diffuse,
            Mosaic,
            Man,
            CM3D2_Debug__Debug_CM3D2_Normal2Color,
            BuiltIn,
            Custom,
        }
        internal static int ShaderLength = (int)ShaderTypes.CM3D2_Debug__Debug_CM3D2_Normal2Color;

        public static readonly ShaderName[] ShaderNames = {
            new ShaderName("CM3D2/Toony_Lighted","トゥーン", ShaderTypes.Toony_Lighted),
            new ShaderName("CM3D2/Toony_Lighted_Trans","トゥーン 透過", ShaderTypes.Toony_Lighted_Trans),
            new ShaderName("CM3D2/Toony_Lighted_Trans_NoZ","トゥーン 透過 NoZ", ShaderTypes.Toony_Lighted_Trans_NoZ),
            new ShaderName("CM3D2/Toony_Lighted_Outline","トゥーン 輪郭線", ShaderTypes.Toony_Lighted_Outline),
            new ShaderName("CM3D2/Toony_Lighted_Outline_Trans","トゥーン 輪郭線 透過", ShaderTypes.Toony_Lighted_Outline_Trans),
            new ShaderName("CM3D2/Toony_Lighted_Hair","トゥーン 髪", ShaderTypes.Toony_Lighted_Hair),
            new ShaderName("CM3D2/Toony_Lighted_Hair_Outline","トゥーン 髪 輪郭線", ShaderTypes.Toony_Lighted_Hair_Outline),
            new ShaderName("CM3D2/Lighted","非トゥーン", ShaderTypes.Lighted),
            new ShaderName("CM3D2/Lighted_Trans","透過", ShaderTypes.Lighted_Trans),
            new ShaderName("Unlit/Texture","発光", ShaderTypes.Unlit__Texture),
            new ShaderName("Unlit/Transparent","発光 透過", ShaderTypes.Unlit__Transparent),
            new ShaderName("Diffuse","リアル", ShaderTypes.Diffuse),
            new ShaderName("Transparent/Diffuse","リアル 透過", ShaderTypes.Transparent__Diffuse),
            new ShaderName("CM3D2/Mosaic","モザイク", ShaderTypes.Mosaic),
            new ShaderName("CM3D2/Man","ご主人様", ShaderTypes.Man),
            new ShaderName("CM3D2_Debug/Debug_CM3D2_Normal2Color","法線", ShaderTypes.CM3D2_Debug__Debug_CM3D2_Normal2Color),
        };

//        // シェーダ名の最大文字数を取得
//        public static int MaxNameLength() {
//            return ShaderNames[ShaderNames.Length-1].Name.Length;
//        }

//        public static int getTypeIndex(string shaderName) {
//            MaterialType mt;
//            return shaderMap.TryGetValue(shaderName, out mt) ? (int)mt.shader.Type : -1;
//            
//        }
//        public static MaterialType resolve(int shaderIdx) {
//            return shaderIdx < matTypes.Length ? matTypes[shaderIdx] : null;
//        }
//        public static MaterialType resolve(string shaderName) {
//            try {
//                return shaderMap[shaderName];
//            } catch(KeyNotFoundException e) {
//                LogUtil.Log("未対応シェーダのため、マテリアルに関するフラグが解決できません。", shaderName);
//                return null;
//            }
//        }

        //        string[] propNames = new string[] { "_MainTex", "_ShadowTex", "_ToonRamp", "_ShadowRateToon", "Alpha", "Multiply", "InfinityColor", "TexTo8bitTex", "Max" };
        internal static readonly PropName[] PropNamesEmpty     = new PropName[] {  };
        internal static readonly PropName[] PropNamesRender    = new PropName[] { PropName._RenderTex };
        internal static readonly PropName[] PropNamesColored   = new PropName[] { PropName._MainTex };
        internal static readonly PropName[] PropNames          = new PropName[] { PropName._MainTex, PropName._ToonRamp, PropName._ShadowTex, PropName._ShadowRateToon };
        internal static readonly PropName[] PropNamesHair      = new PropName[] { PropName._MainTex, PropName._ToonRamp, PropName._ShadowTex, PropName._ShadowRateToon, PropName._HiTex };
        internal const int COLOR     = 0x001;
        internal const int LIGHT     = 0x002;
        internal const int TOONY     = 0x004;
        internal const int OUTLINE   = 0x008;
        internal const int TRANS     = 0x010;
        internal const int HAIR      = 0x020;
        internal const int FLOATVAL1 = 0x040;
        internal const int FLOATVAL2 = 0x080;
        internal const int FLOATVAL3 = 0x100;

        internal const int CUTTOFF   = 0x200;

        private readonly static MaterialType[] matTypes = {
            // CM3D2/Toony_Lighted
            new MaterialType(ShaderNames[0], PropNamesColored,  COLOR+LIGHT+TOONY),               //   0000 0111
            // CM3D2/Toony_Lighted_Trans
            new MaterialType(ShaderNames[1], PropNames,         COLOR+LIGHT+TOONY+TRANS),         //   0001 0111
            // CM3D2/Toony_Lighted_Trans_NoZ
            new MaterialType(ShaderNames[2], PropNames,         COLOR+LIGHT+TOONY+TRANS),         //   0001 0111
            // CM3D2/Toony_Lighted_Outline 
            new MaterialType(ShaderNames[3], PropNames,         COLOR+LIGHT+TOONY+OUTLINE),       //   0000 1111
            // CM3D2/Toony_Lighted_Outline_Trans
            new MaterialType(ShaderNames[4], PropNames,         COLOR+LIGHT+TOONY+OUTLINE+TRANS), //   0001 1111
            // CM3D2/Toony_Lighted_Hair
            new MaterialType(ShaderNames[5], PropNamesHair,     COLOR+LIGHT+TOONY+HAIR),          //   0010 0111
            // CM3D2/Toony_Lighted_Hair_Outline
            new MaterialType(ShaderNames[6], PropNamesHair,     COLOR+LIGHT+TOONY+OUTLINE+HAIR),  //   0010 1111
            // CM3D2/Lighted 
            new MaterialType(ShaderNames[7], PropNamesColored,  COLOR+LIGHT),                     //   0000 0011
            // CM3D2/Lighted_Trans
            new MaterialType(ShaderNames[8], PropNamesColored,  COLOR+LIGHT+TRANS),               //   0001 0011
            // Unlit/Texture
            new MaterialType(ShaderNames[9], PropNamesColored,  0x000), //   0000 0000
            // Unlit/Transparent
            new MaterialType(ShaderNames[10], PropNamesColored, TRANS), //   0000 0000
            // Diffuse 
            new MaterialType(ShaderNames[11], PropNamesColored, COLOR),                           //   0000 0001
            // Transparent/Diffuse 
            new MaterialType(ShaderNames[12], PropNamesColored, COLOR+TRANS),                     //   0001 0001
            // CM3D2/Mosaic
            new MaterialType(ShaderNames[13], PropNamesEmpty,   FLOATVAL1),                       // 0 0100 0000
            // CM3D2/Man
            new MaterialType(ShaderNames[14], PropNamesEmpty,   COLOR+FLOATVAL2+FLOATVAL3),       // 1 1000 0001
            // CM3D2_Debug/Debug_CM3D2_Normal2Color
            new MaterialType(ShaderNames[15], PropNamesEmpty,   COLOR),                           // 0 0000 0001?
        };
        private readonly static Dictionary<string, MaterialType> shaderMap = new Dictionary<string, MaterialType>(16) {
            {matTypes[0].shader.Name, matTypes[0]},
            {matTypes[1].shader.Name, matTypes[1]},
            {matTypes[2].shader.Name, matTypes[2]},
            {matTypes[3].shader.Name, matTypes[3]},
            {matTypes[4].shader.Name, matTypes[4]},
            {matTypes[5].shader.Name, matTypes[5]},
            {matTypes[6].shader.Name, matTypes[6]},
            {matTypes[7].shader.Name, matTypes[7]},
            {matTypes[8].shader.Name, matTypes[8]},
            {matTypes[9].shader.Name, matTypes[9]},
            {matTypes[10].shader.Name, matTypes[10]},
            {matTypes[11].shader.Name, matTypes[11]},
            {matTypes[12].shader.Name, matTypes[12]},
            {matTypes[13].shader.Name, matTypes[13]},
            {matTypes[14].shader.Name, matTypes[14]},
            {matTypes[15].shader.Name, matTypes[15]},
        };
//        private static Dictionary<string, string> Shader2Dic;
//        /// <summary>
//        /// シェーダ1から対応するシェーダ2を取得する
//        /// </summary>
//        /// <param name="shader1">シェーダ1</param>
//        /// <returns>対応するシェーダ2</returns>
//        public static string GatShader2(string shader1) {
//            if (Shader2Dic == null) {
//                Shader2Dic = new Dictionary<string, string>(ShaderType.Shaders.Length);
//                foreach (var shader in ShaderType.Shaders) {
//                    Shader2Dic[shader.name] = shader.name.Replace("/", "__");
//                }
//            }
//            string ret;
//            return Shader2Dic.TryGetValue(shader1, out ret) ? ret : string.Empty;
//        }
 
//        static List<PropName> PropNamesList = Enum.GetValues(typeof(PropName)).Cast<PropName>().ToList();
//           
//        public static void Exec(Action<PropName> action) {
//            foreach (var val in PropNamesList) {
//                action((PropName)val);
//            }
//        }

        public static PropType GetType(PropName prop) {
            switch(prop) {
            case PropName._MainTex:
            case PropName._ToonRamp:
            case PropName._ShadowTex:
            case PropName._ShadowRateToon:
            case PropName._HiTex:
            case PropName._RenderTex:
                return PropType.tex;

            case PropName._Color:
            case PropName._ShadowColor:
            case PropName._OutlineColor:
            case PropName._RimColor:
                return PropType.col;

            case PropName._Shininess:
            case PropName._HiRate:
            case PropName._HiPow:
            case PropName._OutlineWidth:
            case PropName._RimShift:
            case PropName._FloatValue1:
            case PropName._FloatValue2:
            case PropName._FloatValue3:
            case PropName._Cutoff:
                return PropType.f;
            default:
                throw new ACCException("input unsupported propName" + prop);
            }
        }
    }
    


    // シェーダ名として、識別名と表示名を保持するデータクラス
    public class ShaderName {
        internal static int count = 0;
        public int idx            {get; private set;}
        public string Name        {get; private set;}
        public string DisplayName {get; private set;}
        internal ShaderMapper.ShaderTypes Type    {get; private set;}
        internal ShaderName(string name, string displayName, ShaderMapper.ShaderTypes type) {
            Name = name;
            DisplayName = displayName;
            Type = type;
            idx = count++;
        }
    }

    public class MaterialType {
        public MaterialType(ShaderName shader, string[] texPropNames, 
                            bool hasColor, bool isLighted, bool isToony,
                            bool isOutlined, bool isTrans, bool isHair) {
            this.shader = shader;
            this.texPropNames = texPropNames;
            this.hasColor   = hasColor;
            this.isLighted  = isLighted;
            this.isToony    = isToony;
            this.isOutlined = isOutlined;
            this.isHair     = isHair;
            this.isTrans    = isTrans;
            Init();
        }
        public MaterialType(ShaderName shader, PropName[] propNames, int flag) {
            this.shader = shader;
            this.texPropNames = Array.ConvertAll(propNames, value => value.ToString());
            if (propNames.Length == 1) {
                hasRenderTex = (propNames[0] == PropName._RenderTex);
            }

            this.hasColor   = ((flag & ShaderMapper.COLOR)   != 0);
            this.isLighted  = ((flag & ShaderMapper.LIGHT)   != 0);
            this.isToony    = ((flag & ShaderMapper.TOONY)   != 0);
            this.isOutlined = ((flag & ShaderMapper.OUTLINE) != 0);
            this.isHair     = ((flag & ShaderMapper.HAIR)    != 0);
            this.isTrans    = ((flag & ShaderMapper.TRANS)   != 0);
            this.hasFloat1  = ((flag & ShaderMapper.FLOATVAL1) != 0);
            this.hasFloat2  = ((flag & ShaderMapper.FLOATVAL2) != 0);
            this.hasFloat3  = ((flag & ShaderMapper.FLOATVAL3) != 0);
            this.hasCutoff  = ((flag & ShaderMapper.CUTTOFF) != 0);
            Init();
        }

        public HashSet<PropName> propNameSet { get; private set; }
        public ShaderName shader             { get; private set; }
        public String[] texPropNames         { get; private set; }
        public bool hasColor       { get; private set; }
        public bool isLighted      { get; private set; }
        public bool isOutlined     { get; private set; }
        public bool isToony        { get; private set; }
        public bool isHair         { get; private set; }
        public bool isTrans        { get; private set; }
        public bool hasFloat1      { get; private set; }
        public bool hasFloat2      { get; private set; }
        public bool hasFloat3      { get; private set; }
        public bool hasCutoff      { get; private set; }
        public bool hasRenderTex   { get; private set; }
        
        private void Init() {
            propNameSet = new HashSet<PropName>();
            if (hasColor) {
                propNameSet.Add(PropName._Color);
            }
            if (isLighted) {
                propNameSet.Add(PropName._ShadowColor);
                propNameSet.Add(PropName._Shininess);
            }
            if (isOutlined) {
                propNameSet.Add(PropName._OutlineColor);
                propNameSet.Add(PropName._OutlineWidth);
            }
            if (isToony) {
                propNameSet.Add(PropName._ToonRamp);
                propNameSet.Add(PropName._ShadowTex);
                propNameSet.Add(PropName._ShadowRateToon);
                propNameSet.Add(PropName._RimColor);
                propNameSet.Add(PropName._RimPower);
                propNameSet.Add(PropName._RimShift);
            }
            if (texPropNames.Length > 0) {
                propNameSet.Add(PropName._MainTex);
            }
            if (isHair) {
                propNameSet.Add(PropName._HiTex);
                propNameSet.Add(PropName._HiRate);
                propNameSet.Add(PropName._HiPow);
            }
            if (hasFloat1) propNameSet.Add(PropName._FloatValue1);
            if (hasFloat2) propNameSet.Add(PropName._FloatValue2);
            if (hasFloat3) propNameSet.Add(PropName._FloatValue3);
            if (hasCutoff) propNameSet.Add(PropName._Cutoff);
        }
//
//        public bool IsValidProp(string prop) {
//            
//            try {
//                var pn = (PropName)Enum.Parse(typeof(PropName), prop);
//                return IsValidProp(pn);
//            } catch(Exception e) {
//                LogUtil.Debug(e);
//                return false;
//            }
//        }
//        public bool IsValidProp(PropName pn) {
//            
//            try {
//                switch(pn) {
//                    case PropName._MainTex:
//                        if (!hasRenderTex) {
//                            return texPropNames.Length > 0;
//                        }
//                        return false;
//                    case PropName._Color:
//                        return hasColor;
//                    case PropName._ShadowColor:
//                    case PropName._Shininess:
//                        return isLighted;
//                    case PropName._OutlineColor:
//                    case PropName._OutlineWidth:
//                        return isOutlined;
//                    case PropName._ToonRamp:
//                    case PropName._ShadowTex:
//                    case PropName._ShadowRateToon:
//                    case PropName._RimColor:
//                    case PropName._RimPower:
//                    case PropName._RimShift:
//                        return isToony;
//                    case PropName._HiTex:
//                    case PropName._HiRate:
//                    case PropName._HiPow:
//                        return isHair;
//                    case PropName._FloatValue1:
//                        return hasFloat1;
//                    case PropName._FloatValue2:
//                        return hasFloat2;
//                    case PropName._FloatValue3:
//                        return hasFloat3;
//                    case PropName._Cutoff:
//                        return hasCutoff;
//
//                    // 特殊プロパティ
//                    case PropName._RenderTex:
//                        return hasRenderTex;
//                    default:
//                        return false;
//                }
//                    
//            } catch(Exception e) {
//                LogUtil.Debug(e);
//                return false;
//            }
//        }
        public static float GetRenderQueue(string matName) {

            try {
                var priorityMaterials = PrivateAccessor.Get<Dictionary<int, KeyValuePair<string, float>>>(typeof(ImportCM), "m_hashPriorityMaterials");
                KeyValuePair<string, float> kvPair;
                var hashCode = matName.GetHashCode();
                if (priorityMaterials != null && priorityMaterials.TryGetValue(hashCode, out kvPair) ) {
                    if (kvPair.Key == matName) return kvPair.Value;
                }
                return -1f;
            } catch(Exception e) {
                LogUtil.Error("failed to get pmat field.", e);
                return 0f;
            }
        }
    }
    public enum PropName {
        _MainTex,
        _ToonRamp,
        _ShadowTex,
        _ShadowRateToon,
        _HiTex,
        _RenderTex,

        _Color,
        _ShadowColor,
        _OutlineColor,
        _RimColor,

        _Shininess,
        _OutlineWidth,
        _RimPower,
        _RimShift,
        _HiRate,
        _HiPow,
        _FloatValue1,
        _FloatValue2,
        _FloatValue3,
        _Cutoff,
    }

}
