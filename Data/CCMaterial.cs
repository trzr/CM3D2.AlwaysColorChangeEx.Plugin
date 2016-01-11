using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CM3D2.AlwaysColorChange.Plugin.Data {
    public class CCMaterial
    {
        public string name;
        public string shader;
        public Color color        = Color.white;
        public Color shadowColor  = Color.white;
        public Color rimColor     = Color.white;
        public Color outlineColor = Color.black;
        public float shininess = 0f;
        public float outlineWidth = 0f;
        public float rimPower  = 0f;
        public float rimShift  = 0f;
        public float hiRate    = 0f;
        public float hiPow     = 0.001f;
        public float floatVal1 = 0f;
        public float floatVal2 = 0.001f;
        public float floatVal3 = 0.001f;
    }
}