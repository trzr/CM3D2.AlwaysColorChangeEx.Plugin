using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using JsonFx.Json;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using CM3D2.AlwaysColorChangeEx.Plugin.Util.Json;

namespace CM3D2.AlwaysColorChangeEx.Plugin
{
    /// <summary>
    /// プリセットマネージャクラス
    /// </summary>
    public class PresetManager
    {
        private Settings settings = Settings.Instance;
        private MaidHolder holder = MaidHolder.Instance;
        private TextureModifier texModifier = TextureModifier.Instance;
        private FileUtilEx fileUtil = FileUtilEx.Instance;

        public string GetPresetFilepath(string presetName) {

            return Path.Combine(settings.presetDirPath, presetName + FileConst.EXT_JSON);
        }
        public PresetData Load(string fileName) {
            // ファイル読み込み
            try {
                using (FileStream fs = File.OpenRead(fileName))  {
                    var reader = new JsonFx.Json.JsonReader(fs);
                    return (PresetData)reader.Deserialize(typeof(PresetData));
                }
            } catch(Exception e) {
                LogUtil.Log("ACCプリセットの読み込みに失敗しました", e);
                return null;
            }
        }

        public void Save(string fileName,  string presetName, Dictionary<string, bool> dDelNodes) {
            Maid maid = holder.currentMaid;
            // カレントのメイドデータからプリセットデータを抽出
            var preset = new PresetData();
            preset.name = presetName;
            foreach (SlotInfo slotInfo in ACConstants.SlotNames.Values) {
                if (!slotInfo.enable) continue;

                TBodySkin slot = maid.body0.GetSlot((int)slotInfo.Id);
                // マスク情報を抽出
                SlotState maskState;
                if (slot.obj == null) {
                    maskState = SlotState.NotLoaded;
                } else if (!slot.boVisible) {
                    maskState = SlotState.Masked;
                } else {
                    maskState = SlotState.Displayed;
                }

                Material[] materialList = holder.GetMaterials(slot);
                if (materialList.Length == 0) continue;

                var slotItem = new CCSlot(slotInfo.Id);
                slotItem.mask = maskState;

                foreach (Material material in materialList) {
                    string shaderName = material.shader.name;
                    MaterialType mate = ShaderMapper.resolve(shaderName);
                    if (mate == null) continue;

                    var cmat = new CCMaterial(material, mate);
                    slotItem.Add(cmat);
                    foreach (var propName in mate.texPropNames) {
                        var tex2d = material.GetTexture(propName) as Texture2D;
                        if (tex2d == null || string.IsNullOrEmpty(tex2d.name)) continue;

                        var ti = new TextureInfo();
                        cmat.Add(ti);
                        ti.propName = propName;
                        ti.texFile = tex2d.name;
                        var fp = texModifier.GetFilter(maid, slotInfo.Id.ToString(), material.name, tex2d.name);
                        if (fp != null && !fp.hasNotChanged()) ti.filter = new TexFilter(fp);
                    }
                }
                preset.slots.Add(slotItem);
            }

            for (int i = (int)MPN_TYPE_RANGE.BODY_START; i <= (int)MPN_TYPE_RANGE.BODY_END; i++) {
                var mpn = (MPN)Enum.ToObject(typeof(MPN), i);
                MaidProp mp = maid.GetProp(mpn);
                if (mp != null && !String.IsNullOrEmpty(mp.strFileName)) {
                    preset.mpns.Add(new CCMPN(mpn, mp.strFileName));
                }
            }

            for (int i = (int)MPN_TYPE_RANGE.WEAR_START; i <= (int)MPN_TYPE_RANGE.WEAR_END; i++) {
                var mpn = (MPN)Enum.ToObject(typeof(MPN), i);
                MaidProp mp = maid.GetProp(mpn);
                if (mp != null && !String.IsNullOrEmpty(mp.strFileName)) {
                    preset.mpns.Add(new CCMPN(mpn, mp.strFileName));
                }
            }

            // 表示ノード
            preset.delNodes = new Dictionary<string, bool>(dDelNodes);
            
            LogUtil.DebugLog("create preset...", fileName);
            SavePreset(fileName, preset);
        }

        public void SavePreset(string fileName, PresetData preset) {
            if (File.Exists(fileName)) {
                File.Delete(fileName);
            }

            // ファイル出力
            var jws = new JsonWriterSettings();
            //jws.Tab = "  ";
            jws.MaxDepth = 200;
            jws.PrettyPrint = true;
            using (FileStream fs = File.OpenWrite(fileName))
            using (var writer = new CustomJsonWriter(fs, jws)) {
                writer.ignoreNull = true;
                writer.Write(preset);
            }
            LogUtil.DebugLog("preset saved...", fileName);
        }
        public void ApplyPresetMPN(Maid maid, PresetData preset, bool applyBody, bool applyWear, bool castoff) {
            // 衣装チェンジ
            foreach (var mpn in preset.mpns) {
                if (!applyBody) {
                    // bodyのMPNをスキップ
                    if (TypeUtil.IsBody(mpn.name)) continue;
                }
                if (!applyWear) {
                    // wearのMPNをスキップ
                    if (TypeUtil.IsWear(mpn.name)) continue;
                }

                if (mpn.filename.EndsWith("_del.menu", StringComparison.OrdinalIgnoreCase)) {
                    if (castoff) {
                        maid.SetProp(mpn.name, mpn.filename, 0, false);
                    }
                    continue;
                // } else if (mpn.filename.EndsWith(".mod", StringComparison.OrdinalIgnoreCase)) {
                }
                // MODなどが存在しない場合はスキップ
                if (!fileUtil.Exists(mpn.filename)) continue;

                maid.SetProp(mpn.name, mpn.filename, 0, false);
            }
            maid.AllProcPropSeqStart();
        }
        public void ApplyPresetMaterial(Maid maid, PresetData preset) 
        {
            if (maid == null) maid = holder.currentMaid;
            if (maid == null) return;

            foreach (var ccslot in preset.slots) {
                // スロット上のマテリアル番号での判断に変更
                TBodySkin slot = maid.body0.GetSlot((int)ccslot.id);
                Material[] materials = holder.GetMaterials(slot);
                if (slot.obj == null) {
                    LogUtil.DebugLog("slot.obj null. name=", ccslot.id);
                }
                if (!materials.Any()) continue; // 未装着スロットはスキップ

                var slotName = ccslot.id.ToString();
                int matNo=-1;
                foreach (CCMaterial cmat in ccslot.materials) {
                    if (++matNo < materials.Length) {
                        Material m = materials[matNo];
                        if (cmat.name != m.name) {
                            LogUtil.DebugLogF("マテリアル名が一致しないため、適用しません。 slot={0}, matNo={1}, name=({2}<=>{3})", 
                                     ccslot.id, matNo, cmat.name, m.name);
                            continue;
                        }
                        cmat.Apply(m);

                        // テクスチャ適用
                        List<TextureInfo> texes = cmat.texList;
                        if (texes == null) continue;

                        foreach (var texInfo in texes) {
                            var tex = m.GetTexture(texInfo.propName);
                            // テクスチャファイルの変更
                            if (tex == null || tex.name != texInfo.texFile) {
                                var filename = texInfo.texFile;
                                if (filename.LastIndexOf('.') == -1) {
                                    filename += FileConst.EXT_TEXTURE;
                                }
                                // if (!filename.EndsWith(FileConst.EXT_TEXTURE, StringComparison.OrdinalIgnoreCase)) {
                                // ファイルが存在する場合にのみ適用
                                if (fileUtil.Exists(filename)) {
                                    maid.body0.ChangeTex(slotName, matNo, texInfo.propName, filename, null, MaidParts.PARTS_COLOR.NONE);

                                    // ChangeTexは、nameにファイル名が設定されてしまうため、拡張子を除いた名前を再設定
                                    var changedTex = m.GetTexture(texInfo.propName);
                                    if (changedTex != null) {
                                        changedTex.name = texInfo.texFile;
                                    }
                                } else {
                                    LogUtil.DebugLog("texture file not found. file=", filename);
                                }
                            }

                            // フィルタ適用
                            if (texInfo.filter != null) {
                                var fp = texInfo.filter.ToFilter();
                                texModifier.ApplyFilter(maid, slotName, m, texInfo.propName, fp);
                            }
                        }
                    } else {
                        LogUtil.LogF("マテリアル番号に一致するマテリアルが見つかりません。 slot={0}, matNo={1}, name={2}", 
                                     ccslot.id, matNo, cmat.name);
                        break;
                    }
                }
            }
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
        public Dictionary<string, PresetData> LoadXML(string fileName) {

            var xdoc = XDocument.Load(fileName);
            var presetNodes = xdoc.Descendants("preset");
            if (!presetNodes.Any()) {
                return null;
            }
            var presets = new Dictionary<string, PresetData>();
            try {
                foreach (var presetNode in presetNodes) {
                    var preset = new PresetData();
                    preset.name = GetAttribute(presetNode, "name");
                    
                    var slotsNode = presetNode.Element("slots");
                    if (slotsNode != null) {
                        var slotNodes = slotsNode.Elements("slot");
                        foreach (var slotNode in slotNodes) {
                            CCSlot slot = null;
                            var slotName = GetAttribute(slotNode, "slotname");
                            try {
                                slot = new CCSlot(slotName);
                            } catch(Exception e) {
                                LogUtil.Log("未対応のスロットをスキップします.", slotName, e);
                                continue;
                            }
                            var materialNodes = slotNode.Elements("material");
                            foreach (var materialNode in materialNodes) {
                                var cmat = new CCMaterial();
                                cmat.name = GetElementVal(materialNode, "name");
                                cmat.shader = GetElementVal(materialNode, "shader");
                                var colorNode = materialNode.Element("color");
                                if (colorNode != null) cmat.color = loadColor(colorNode);
                                colorNode = materialNode.Element("shadowColor");
                                if (colorNode != null) cmat.shadowColor = loadColor(colorNode);
                                colorNode = materialNode.Element("rimColor");
                                if (colorNode != null) cmat.rimColor = loadColor(colorNode);
                                colorNode = materialNode.Element("outlineColor");
                                if (colorNode != null) cmat.outlineColor = loadColor(colorNode);
                                var f = materialNode.Element("shininess");
                                if (f != null) cmat.shininess = (float)f;
                                f = materialNode.Element("outlineWidth");
                                if (f != null)  cmat.outlineWidth = (float)f;
                                f = materialNode.Element("rimPower");
                                if (f != null)  cmat.rimPower = (float)f;
                                f = materialNode.Element("rimShift");
                                if (f != null)  cmat.rimShift = (float)f;
                                f = materialNode.Element("hiRate");
                                if (f != null) cmat.hiRate = (float)f;
                                f = materialNode.Element("hiPow");
                                if (f != null) cmat.hiPow = (float)f;
                                f = materialNode.Element("floatValue1");
                                if (f != null) cmat.floatVal1 = (float)f;
                                f = materialNode.Element("floatValue2");
                                if (f != null) cmat.floatVal2 = (float)f;
                                f = materialNode.Element("floatValue3");
                                if (f != null) cmat.floatVal3 = (float)f;
                                slot.Add(cmat);
                            }
                            //preset.slots.Add(slot.name, slot);
                            preset.slots.Add(slot);
                        }
                    }
                    var mpnsNode = presetNode.Element("mpns");
                    if (mpnsNode != null) {
                        var mpnNodes = mpnsNode.Elements("mpn");
                        foreach (var mpnNode in mpnNodes) {
                            preset.mpns.Add(new CCMPN(GetAttribute(mpnNode, "name"), mpnNode.Value));
                        }
                    }
                    var nodesNode = presetNode.Element("nodes");
                    if (nodesNode != null) {
                        var nodes = nodesNode.Elements("node");
                        if (nodes.Any()) {
                            var delNodes = new Dictionary<string, bool>();
                            foreach (var node in nodes) {
                                var nodeName = GetAttribute(node, "name");
                                if (nodeName != null) {
                                    bool v = GetBoolAttribute(node, "visible");
                                    // 対応するノード名をそのまま使用
                                    if (ACConstants.NodeNames.ContainsKey(nodeName)) {
                                        delNodes.Add(nodeName, v);
                                    }
                                }
                            }
                            preset.delNodes = delNodes;
                        }
                    }
                    
                    presets.Add(preset.name, preset);
                }
            } catch(Exception e) {
                LogUtil.ErrorLog("failed to load presets. file=",fileName, ". ", e);
                return null; 
            }
            
            return presets;
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
        private CCColor loadColor(XElement colorNode) {
            var r = (float)colorNode.Attribute("R");
            var g = (float)colorNode.Attribute("G");
            var b = (float)colorNode.Attribute("B");
            var a = (float)colorNode.Attribute("A");
            return new CCColor(r, g, b, a);
        }
    }
}
