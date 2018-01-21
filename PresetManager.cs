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
                    var type = ShaderType.Resolve(material.shader.name);
                    if (type == ShaderType.UNKNOWN) continue;
                    var cmat = new CCMaterial(material, type);
                    slotItem.Add(cmat);
                    foreach (var texProp in type.texProps) {
                        var tex2d = material.GetTexture(texProp.propId) as Texture2D;
                        if (tex2d == null || string.IsNullOrEmpty(tex2d.name)) continue;

                        var ti = new TextureInfo();
                        cmat.Add(ti);
                        ti.propName = texProp.keyName;
                        ti.texFile = tex2d.name;
                        var fp = texModifier.GetFilter(maid, slotInfo.Id.ToString(), material.name, tex2d.name);
                        if (fp != null && !fp.hasNotChanged()) ti.filter = new TexFilter(fp);
                    }
                }
                preset.slots.Add(slotItem);
            }

            for (int i = TypeUtil.BODY_START; i <= TypeUtil.BODY_END; i++) {
                var mpn = (MPN)Enum.ToObject(typeof(MPN), i);
                MaidProp mp = maid.GetProp(mpn);
                if (mp != null) {
                    if (!String.IsNullOrEmpty(mp.strFileName)) {
                        preset.mpns.Add(new CCMPN(mpn, mp.strFileName));
                    } else {
                        preset.mpnvals.Add(new CCMPNValue(mpn, mp.value, mp.min, mp.max));
                    }
                }
            }

            for (int i = TypeUtil.WEAR_START; i <= TypeUtil.WEAR_END; i++) {
                var mpn = (MPN)Enum.ToObject(typeof(MPN), i);
                MaidProp mp = maid.GetProp(mpn);
                if (mp != null && !String.IsNullOrEmpty(mp.strFileName)) {
                    preset.mpns.Add(new CCMPN(mpn, mp.strFileName));
                }
            }
//            for (int i = (int)MPN_TYPE_RANGE.FOLDER_BODY_START; i <= (int)MPN_TYPE_RANGE.FOLDER_BODY_END; i++) {
//                var mpn = (MPN)Enum.ToObject(typeof(MPN), i);
//                MaidProp mp = maid.GetProp(mpn);
//                if (mp != null) {
//                    LogUtil.Debug(mpn,":", mp.type, ", value=", mp.value, ", temp=", mp.temp_value, ", file=", mp.strFileName);
//                }
//            }

            // 無限色
            for (int j = TypeUtil.PARTSCOLOR_START; j <= TypeUtil.PARTSCOLOR_END; j++) {
                var pcEnum = (MaidParts.PARTS_COLOR)j;
                MaidParts.PartsColor part = maid.Parts.GetPartsColor(pcEnum);
                preset.partsColors[pcEnum.ToString()] = new CCPartsColor(part);
            }

            // 表示ノード
            preset.delNodes = new Dictionary<string, bool>(dDelNodes);
            
            LogUtil.Debug("create preset...", fileName);
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
            LogUtil.Debug("preset saved...", fileName);
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
                        if (SetProp != null) SetProp(maid, mpn.name, mpn.filename, 0);
                        else LogUtil.Error("failed to apply preset. mpn=", mpn.name);
                    }
                    continue;
                // } else if (mpn.filename.EndsWith(".mod", StringComparison.OrdinalIgnoreCase)) {
                }
                // menuファイルが存在しない場合はスキップ
                if (!fileUtil.Exists(mpn.filename)) continue;

                if (SetProp != null) SetProp(maid, mpn.name, mpn.filename, 0);
                else LogUtil.Error("failed to apply preset. mpn=", mpn.name);
            }

            if (applyBody) {
                // 設定プロパティ反映
                foreach (var mpn in preset.mpnvals) {
                    var mp = maid.GetProp(mpn.name);
                    if (mp != null) {
                        mp.value = mpn.value;
                        if (mp.min > mpn.min)  {mp.min = mpn.min;}
                        if (mp.max < mpn.max)  {mp.max = mpn.max;}
                    } else {
                        LogUtil.Debug("failed to apply MaidProp. mpn:", mpn.name);
                    }
                }
            }
            //maid.AllProcPropSeqStart();
        }
        public void ApplyPresetMPNProp(Maid maid, PresetData preset) {
            // 設定プロパティ反映
            foreach (var mpn in preset.mpnvals) {
                var mp = maid.GetProp(mpn.name);
                if (mp != null) {
                    mp.value = mpn.value;
                    if (mp.min > mpn.min)  {mp.min = mpn.min;}
                    if (mp.max < mpn.max)  {mp.max = mpn.max;}
                } else {
                    LogUtil.Debug("failed to apply MaidProp. mpn:", mpn.name);
                }
            }
        }
        public void ApplyPresetMaterial(Maid maid, PresetData preset) 
        {
            if (maid == null) maid = holder.currentMaid;
            if (maid == null) return;

            foreach (var ccslot in preset.slots) {
                int slotNo = (int)ccslot.id;
                if (slotNo >= maid.body0.goSlot.Count) continue; // スロットがないケースはスキップ

                // スロット上のマテリアル番号での判断に変更
                TBodySkin slot = maid.body0.GetSlot(slotNo);
                Material[] materials = holder.GetMaterials(slot);
                if (slot.obj == null) {
                    LogUtil.Debug("slot.obj null. name=", ccslot.id);
                }
                if (!materials.Any()) continue; // 未装着スロットはスキップ

                var slotName = ccslot.id.ToString();
                int matNo=-1;
                foreach (CCMaterial cmat in ccslot.materials) {
                    if (++matNo < materials.Length) {
                        Material m = materials[matNo];
                        if (cmat.name != m.name) {
                            LogUtil.DebugF("Material name mismatched. skipping apply preset-slot={0}, matNo={1}, name=({2}<=>{3})", 
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
                                    LogUtil.Debug("texture file not found. file=", filename);
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
        // disable once MemberCanBeMadeStatic.Local
        public void ApplyPresetPartsColor(Maid maid, PresetData preset) {
            foreach(var pc in preset.partsColors) {
                MaidParts.PARTS_COLOR partsColor;
                try {
                    partsColor = (MaidParts.PARTS_COLOR)Enum.Parse(typeof(MaidParts.PARTS_COLOR), pc.Key);
                    maid.Parts.SetPartsColor(partsColor, pc.Value.toStruct());
                } catch(ArgumentException e) {
                    LogUtil.Debug(e);
                }
            }
        }
        private static Action<Maid, MPN, string, int> SetProp;
        static PresetManager()
        {
            Type typeObj = typeof(Maid);
            // 1.56以降
            var method = typeObj.GetMethod("SetProp", new[] { typeof(MPN), typeof(string), typeof(int), typeof(bool), typeof(bool) });
            if (method != null) {
                SetProp = (maid, mpn, str, id) => {
                    method.Invoke(maid, new object[] { mpn, str, id, false, false });
                };
                return;
            }
            // 1.xx　～ 1.55.1
            method = typeObj.GetMethod("SetProp", new[] { typeof(MPN), typeof(string), typeof(int), typeof(bool), });
            if (method != null) {
                SetProp = (maid, mpn, str, id) => {
                    method.Invoke(maid, new object[] { mpn, str, id, false, });
                };
                return;
            }

            // 1.xx 以前
            method = typeObj.GetMethod("SetProp", new[] { typeof(MPN), typeof(string), typeof(int), });
            if (method != null) {
                SetProp = (maid, mpn, str, id) => {
                    method.Invoke(maid, new object[] { mpn, str, id, });
                };
                return;
            }

            LogUtil.Error("failed to load Maid#SetProp method. Preset-feature dose not work properly. please rebuild dll! ");
        }
    }
}
