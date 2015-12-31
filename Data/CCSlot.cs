using System;
using System.Collections.Generic;

namespace CM3D2.AlwaysColorChange.Plugin.Data
{
    /// <summary>
    /// Description of CCSlot.
    /// </summary>
    public class CCSlot
    {
        public string name;

        public bool enabled = false;

        public Dictionary<string, CCMaterial> materials;
    }
}
