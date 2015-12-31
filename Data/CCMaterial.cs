using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CM3D2.AlwaysColorChange.Plugin.Data {
    public class CCMaterial
    {
        public string name;
        public string shader;
        public Color color        = new Color(1, 1, 1, 1);
        public Color shadowColor  = new Color(1, 1, 1, 1);
        public Color rimColor     = new Color(1, 1, 1, 1);
        public Color outlineColor = new Color(0, 0, 0, 1);
        public float shininess = 0f;
        public float outlineWidth = 0f;
        public float rimPower = 0f;
        public float rimShift = 0f;
        public float hiRate = 0f;
        public float hiPow = 0.001f;
    }
}