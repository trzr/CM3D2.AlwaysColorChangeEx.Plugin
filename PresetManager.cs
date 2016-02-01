using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;

namespace CM3D2.AlwaysColorChangeEx.Plugin
{
    /// <summary>
    /// Description of SaveLoader.
    /// </summary>
    public class PresetManager
    {
        private Settings settings = Settings.Instance;
        private MaidHolder holder = MaidHolder.Instance;
        private readonly Dictionary<string, CCPreset> empty = new Dictionary<string, CCPreset>();

        public Dictionary<string, CCPreset> Load(string fileName) {

            var xdoc = XDocument.Load(fileName);
            var presetNodes = xdoc.Descendants("preset");
            if (!presetNodes.Any()) {
                return empty;
            }
            var presets = new Dictionary<string, CCPreset>();
            try {
                foreach (var presetNode in presetNodes) {
                    var preset = new CCPreset();
                    preset.name = GetAttribute(presetNode, "name");
                    preset.clearMask = GetBoolAttribute(presetNode, "clearMask");
                    
                    preset.slots = new Dictionary<string, CCSlot>();
                    var slotsNode = presetNode.Element("slots");
                    if (slotsNode != null) {
                        var slotNodes = slotsNode.Elements("slot");
                        foreach (var slotNode in slotNodes) {
                            var slot = new CCSlot();
                            slot.name = GetAttribute(slotNode, "slotname");
                            slot.materials = new Dictionary<string, CCMaterial>();
                            var materialNodes = slotNode.Elements("material");
                            foreach (var materialNode in materialNodes) {
                                var material = new CCMaterial();
                                material.name = GetElementVal(materialNode, "name");
                                material.shader = GetElementVal(materialNode, "shader");
                                var colorNode = materialNode.Element("color");
                                if (colorNode != null) {
                                    material.color = loadColor(colorNode);
                                }
                                colorNode = materialNode.Element("shadowColor");
                                if (colorNode != null) {
                                    material.shadowColor = loadColor(colorNode);
                                }
                                colorNode = materialNode.Element("rimColor");
                                if (colorNode != null) {
                                    material.rimColor = loadColor(colorNode);
                                }
                                colorNode = materialNode.Element("outlineColor");
                                if (colorNode != null) {
                                    material.outlineColor = loadColor(colorNode);
                                }
                                
                                var f = materialNode.Element("shininess");
                                if (f != null) {
                                    material.shininess = (float)f;
                                }
                                f = materialNode.Element("outlineWidth");
                                if (f != null) {
                                    material.outlineWidth = (float)f;
                                }
                                f = materialNode.Element("rimPower");
                                if (f != null) {
                                    material.rimPower = (float)f;
                                }
                                f = materialNode.Element("rimShift");
                                if (f != null) {
                                    material.rimShift = (float)f;
                                }
                                f = materialNode.Element("hiRate");
                                if (f != null) {
                                    material.hiRate = (float)f;
                                }
                                f = materialNode.Element("hiPow");
                                if (f != null) {
                                    material.hiPow = (float)f;
                                }
                                f = materialNode.Element("floatValue1");
                                if (f != null) {
                                    material.floatVal1 = (float)f;
                                }
                                f = materialNode.Element("floatValue2");
                                if (f != null) {
                                    material.floatVal2 = (float)f;
                                }
                                f = materialNode.Element("floatValue3");
                                if (f != null) {
                                    material.floatVal3 = (float)f;
                                }
                                //slot.materials.Add(material.name, material);
                                slot.materials[material.name] = material;
                            }
                            //preset.slots.Add(slot.name, slot);
                            preset.slots[slot.name] = slot;
                        }
                    }
                    
                    var mpnsNode = presetNode.Element("mpns");
                    if (mpnsNode != null) {
                        var mpnNodes = mpnsNode.Elements("mpn");
                        foreach (var mpnNode in mpnNodes) {
                            preset.mpns.Add(GetAttribute(mpnNode, "name"), mpnNode.Value);
                        }
                    }
                    var nodesNode = presetNode.Element("nodes");
                    if (nodesNode != null) {
                        var nodes = nodesNode.Elements("node");
                        foreach (var node in nodes) {
                            var nodeName = GetAttribute(node, "name");
                            if (nodeName != null) {
                                bool v = GetBoolAttribute(node, "visible");
                                // 対応するノード名をそのまま使用
                                if (ACConstants.NodeNames.ContainsKey(nodeName)) {
                                    preset.delNodes.Add(nodeName, v);
                                } else {
                                    // 旧版の表示名の場合は、ノード名に変換
                                    string key = ACConstants.Nodenames[nodeName];
                                    preset.delNodes.Add(key, v);
                                }
                            }
                        }
                    }
                    
                    presets.Add(preset.name, preset);
                }
            } catch(Exception e) {
                LogUtil.ErrorLog("failed to load presets",fileName, e);
                return empty; 
            }
            
            return presets;
        }

        public void Save(string fileName,  string presetName, bool bClearMaskEnable, bool bSaveBodyPreset, Dictionary<string, bool> dDelNodes) {

            XDocument xdoc;
            if (File.Exists(fileName)) {
                RemovePreset(fileName, presetName);
                xdoc = XDocument.Load(fileName);
                LogUtil.DebugLog("preset override. file", fileName);
            } else {
                LogUtil.DebugLog("save preset. file", fileName);
                xdoc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "true"),
                    new XElement("ColorChange")
                    //                        new XAttribute("toggleKey",settings.toggleKey)
                    
                   );
                xdoc.Save(fileName);
            }

            LogUtil.DebugLog("create preset...");

            var preset = new XElement("preset",
                                      new XAttribute("name", presetName),
                                      new XAttribute("clearMask", bClearMaskEnable)
                                     );
            var slots = new XElement("slots");
            foreach (SlotInfo slot in ACConstants.SlotNames.Values) {
                Material[] materialList = holder.GetMaterials(slot);
                if (materialList.Length == 0) continue;

                var slotDoc = new XElement("slot", new XAttribute("slotname", slot.Name) );

                foreach (Material material in materialList) {
                    string shaderName = material.shader.name;
                    MaterialType mate = ShaderMapper.resolve(shaderName);
                    if (mate == null) continue;

                    var materialNode = new XElement("material",
                                                    new XElement("name", material.name),
                                                    new XElement("shader", material.shader.name));
                    
                    if (mate.hasColor) {
                        Color color        = material.GetColor("_Color");
                        materialNode.Add(new XElement("color",
                                new XAttribute("R", color.r),
                                new XAttribute("G", color.g),
                                new XAttribute("B", color.b),
                                new XAttribute("A", color.a)));
                    }
                    if (mate.isLighted) {
                        Color shadowColor  = material.GetColor("_ShadowColor");
                        materialNode.Add(new XElement("shadowColor",
                                new XAttribute("R", shadowColor.r),
                                new XAttribute("G", shadowColor.g),
                                new XAttribute("B", shadowColor.b),
                                new XAttribute("A", shadowColor.a)));
                    }
                    if (mate.isToony) {
                        Color rimColor     = material.GetColor("_RimColor");
                        materialNode.Add(new XElement("rimColor",
                            new XAttribute("R", rimColor.r),
                            new XAttribute("G", rimColor.g),
                            new XAttribute("B", rimColor.b),
                            new XAttribute("A", rimColor.a)));
                    }
                    if (mate.isOutlined) {
                        Color outlineColor = material.GetColor("_OutlineColor");
                        materialNode.Add(new XElement("outlineColor",
                            new XAttribute("R", outlineColor.r),
                            new XAttribute("G", outlineColor.g),
                            new XAttribute("B", outlineColor.b),
                            new XAttribute("A", outlineColor.a)));
                    }
                    if (mate.isLighted) {
                        float shininess    = material.GetFloat("_Shininess");
                        materialNode.Add(new XElement("shininess", shininess));
                    }
                    if (mate.isOutlined) {
                        float outlineWidth = material.GetFloat("_OutlineWidth");
                        materialNode.Add(new XElement("outlineWidth", outlineWidth));
                    }
                    if (mate.isToony) {
                        float rimPower     = material.GetFloat("_RimPower");
                        float rimShift     = material.GetFloat("_RimShift");
                        materialNode.Add(new XElement("rimPower", rimPower),
                                        new XElement("rimShift", rimShift));
                    }
                    if (mate.isHair) {
                        float hiRate       = material.GetFloat("_HiRate");
                        float hiPow        = material.GetFloat("_HiPow");
                        materialNode.Add(new XElement("hiRate", hiRate));
                        materialNode.Add(new XElement("hiPow", hiPow));
                    }
                    if (mate.hasFloat1) {
                        float floatVal1    = material.GetFloat("_FloatValue1");
                        materialNode.Add(new XElement("floatVal1", floatVal1));
                    }
                    if (mate.hasFloat2) {
                        float floatVal2    = material.GetFloat("_FloatValue2");
                        materialNode.Add(new XElement("floatVal2", floatVal2));
                    }
                    if (mate.hasFloat3) {
                        float floatVal3    = material.GetFloat("_FloatValue3");
                        materialNode.Add(new XElement("floatVal3", floatVal3));
                    }

                    slotDoc.Add(materialNode);
                }
                slots.Add(slotDoc);

            }
            preset.Add(slots);

            var mpns = new XElement("mpns");
            if (bSaveBodyPreset) {
                for (int i = (int)MPN_TYPE_RANGE.BODY_START; i <= (int)MPN_TYPE_RANGE.BODY_END; i++) {
                    var mpn = (MPN)Enum.ToObject(typeof(MPN), i);
                    MaidProp mp = holder.currentMaid.GetProp(mpn);
                    if (mp != null && !String.IsNullOrEmpty(mp.strFileName)) {
                        var mpnNode = new XElement("mpn",
                            new XAttribute("name", Enum.GetName(typeof(MPN), mpn)),
                            mp.strFileName);
                        mpns.Add(mpnNode);
                    }
                }
            }

            for (int i = (int)MPN_TYPE_RANGE.WEAR_START; i <= (int)MPN_TYPE_RANGE.WEAR_END; i++) {
                var mpn = (MPN)Enum.ToObject(typeof(MPN), i);
                MaidProp mp = holder.currentMaid.GetProp(mpn);
                if (mp != null && !String.IsNullOrEmpty(mp.strFileName)) {
                    var mpnNode = new XElement("mpn",
                        new XAttribute("name", Enum.GetName(typeof(MPN), mpn)),
                        mp.strFileName);
                    mpns.Add(mpnNode);
                }
            }
            preset.Add(mpns);

            var delNodes = new XElement("nodes");
            foreach (string key in ACConstants.NodeNames.Keys) {
                bool visible = true;
                if (dDelNodes != null && dDelNodes.ContainsKey(key)) {
                    visible = dDelNodes[key];
                }
                var node = new XElement("node",
                    new XAttribute("name", key),
                    new XAttribute("visible", visible));
                delNodes.Add(node);
            }
            preset.Add(delNodes);


            var presetNodes = xdoc.Descendants("preset");
            if (!presetNodes.Any()) {
                xdoc.Root.AddFirst(preset);
            } else {
                presetNodes.Last().AddAfterSelf(preset);
            }
            xdoc.Save(fileName);
            LogUtil.DebugLog("preset saved...", fileName);
        }
                
        private void RemovePreset(string fileName, string prstName)
        {
            var xdoc = XDocument.Load(fileName);
            IEnumerable<XElement> removeTarget =
                from el in xdoc.Descendants("preset")
                    where (string)el.Attribute("name") == prstName
                select el;

            if (removeTarget.Any()) {
                foreach (var elem in removeTarget.ToList()) {
                    //LogUtil.DebugLog("remove preset", elem.ToString());
                    elem.Remove();
                }
                xdoc.Save(fileName);
            }
        }

        private string GetAttribute(XElement elem, string attrName) {
            var attr = elem.Attribute(attrName);
            return (attr != null)? attr.Value : null;
        }
        private bool GetBoolAttribute(XElement elem, string attrName) {
            var attr = elem.Attribute(attrName);
            if (attr != null && !String.IsNullOrEmpty(attr.Value)) {
                return (bool)attr;
            }
            return false;
        }
        private string GetElementVal(XContainer elem, string name) {
            var target = elem.Element(name);
            return (target != null)? target.Value : null;
        }
        private Color loadColor(XElement colorNode) {
            var r = (float)colorNode.Attribute("R");
            var g = (float)colorNode.Attribute("G");
            var b = (float)colorNode.Attribute("B");
            var a = (float)colorNode.Attribute("A");
            return new Color(r, g, b, a);
        }

    }
}
