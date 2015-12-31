using System;
using System.Collections.Generic;

namespace CM3D2.AlwaysColorChange.Plugin.Data
{
    /// <summary>
    /// Description of CCPreset.
    /// </summary>
    public class CCPreset
    {
        public string name;
        public bool clearMask = false;

        public Dictionary<string, CCSlot> slots;

        public Dictionary<string, string> mpns = new Dictionary<string, string>();

        public Dictionary<string, bool> delNodes = new Dictionary<string, bool>();
    }
}
