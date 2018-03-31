using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JsonFx.Json;
using UnityEngine;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using CM3D2.AlwaysColorChangeEx.Plugin.Util.Json;

namespace CM3D2.AlwaysColorChangeEx.Plugin {
    /// <summary>
    /// プリセットマネージャクラス
    /// </summary>
    public class PresetManager {
        private readonly Settings _settings = Settings.Instance;
        private readonly MaidHolder _holder = MaidHolder.Instance;
        private readonly TextureModifier _texModifier = TextureModifier.Instance;
        private readonly FileUtilEx _fileUtil = FileUtilEx.Instance;

        public string GetPresetFilepath(string presetName) {
            return Path.Combine(_settings.presetDirPath, presetName + FileConst.EXT_JSON);
        }

        public PresetData Load(string fileName) {
            // ファイル読み込み
            try {
                using (var fs = File.OpenRead(fileName)) {
                    var reader = new JsonReader(fs);
                    return (PresetData)reader.Deserialize(typeof(PresetData));
                }
            } catch (Exception e) {
                LogUtil.Log("ACCプリセットの読み込みに失敗しました", e);
                return null;
            }
        }

        public void Save(string fileName, string presetName, Dictionary<string, bool> dDelNodes) {
            var maid = _holder.CurrentMaid;
            // カレントのメイドデータからプリセットデータを抽出
            var preset = new PresetData {name = presetName};
            foreach (var slotInfo in ACConstants.SlotNames.Values) {
                if (!slotInfo.enable) continue;

                var slot = maid.body0.GetSlot((int)slotInfo.Id);
                // マスク情報を抽出
                SlotState maskState;
                if (slot.obj == null) {
                    maskState = SlotState.NotLoaded;
                } else if (!slot.boVisible) {
                    maskState = SlotState.Masked;
                } else {
                    maskState = SlotState.Displayed;
                }

                var materialList = _holder.GetMaterials(slot);
                if (materialList.Length == 0) continue;

                var slotItem = new CCSlot(slotInfo.Id) {mask = maskState};

                foreach (var material in materialList) {
                    var type = ShaderType.Resolve(material.shader.name);
                    if (type == ShaderType.UNKNOWN) continue;
                    var cmat = new CCMaterial(material, type);
                    slotItem.Add(cmat);
                    foreach (var texProp in type.texProps) {
                        var tex2D = material.GetTexture(texProp.propId) as Texture2D;
                        if (tex2D == null || string.IsNullOrEmpty(tex2D.name)) continue;

                        var ti = new TextureInfo();
                        cmat.Add(ti);
                        ti.propName = texProp.keyName;
                        ti.texFile = tex2D.name;
                        var fp = _texModifier.GetFilter(maid, slotInfo.Id.ToString(), material.name, tex2D.name);
                        if (fp != null && !fp.HasNotChanged()) ti.filter = new TexFilter(fp);

                        var offset = material.GetTextureOffset(texProp.propId);
                        if (Math.Abs(offset.x) > ConstantValues.EPSILON_3) {
                            ti.offsetX = offset.x;
                        }
                        if (Math.Abs(offset.y) > ConstantValues.EPSILON_3) {
                            ti.offsetY = offset.y;
                        }

                        var scale = material.GetTextureScale(texProp.propId);
                        if (Math.Abs(scale.x) > ConstantValues.EPSILON_3) {
                            ti.scaleX = scale.x;
                        }
                        if (Math.Abs(scale.y) > ConstantValues.EPSILON_3) {
                            ti.scaleY = scale.y;
                        }
                    }
                }
                preset.slots.Add(slotItem);
            }

            for (var i = MPN_TYPE_RANGE.BODY_START; i <= MPN_TYPE_RANGE.BODY_END; i++) {
                var mpn = (MPN)Enum.ToObject(typeof(MPN), i);
                var mp = maid.GetProp(mpn);
                if (mp == null) continue;
                // 身体パラメータ
                if (mp.type == 1 || mp.type == 2) {
                    preset.mpnvals.Add(new CCMPNValue(mpn, mp.value, mp.min, mp.max));
                    continue;
                }
                // スロットアイテム
                if (mp.type == 3 && mp.nFileNameRID != 0) {
                    preset.mpns.Add(new CCMPN(mpn, mp.strFileName));
                }
            }

            // FOLDER_BODYは自動で0にリセットされるためプリセットの保持する必要はない
            // for (var i = MPN_TYPE_RANGE.FOLDER_BODY_START; i <= MPN_TYPE_RANGE.FOLDER_BODY_END; i++) {
            //     var mpn = (MPN)Enum.ToObject(typeof(MPN), i);
            //     var mp = maid.GetProp(mpn);
            //     if (mp == null || mp.nFileNameRID == 0) continue;
            //     preset.mpns.Add(new CCMPN(mpn, mp.strFileName));
            // }
            for (var i = MPN_TYPE_RANGE.WEAR_START; i <= MPN_TYPE_RANGE.WEAR_END; i++) {
                var mpn = (MPN)Enum.ToObject(typeof(MPN), i);
                var mp = maid.GetProp(mpn);
                if (mp != null && mp.nFileNameRID != 0) {
                    preset.mpns.Add(new CCMPN(mpn, mp.strFileName));
                }
            }

            // 無限色
            for (var pcEnum = MaidParts.PARTS_COLOR.NONE+1; pcEnum < MaidParts.PARTS_COLOR.MAX; pcEnum++) {
                var part = maid.Parts.GetPartsColor(pcEnum);
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
            var jws = new JsonWriterSettings {
                MaxDepth = 200,
                PrettyPrint = true
            };
            using (var fs = File.OpenWrite(fileName))
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
                // menuファイルが存在しない場合はスキップ
                if (!_fileUtil.Exists(mpn.filename)) continue;

                var prop = maid.GetProp(mpn.name);
                if (mpn.filename.Equals(prop.strFileName, StringComparison.OrdinalIgnoreCase)) {
                    LogUtil.Debug("apply preset skip. mpn:", mpn.name, ", file:", mpn.filename);
                    continue;
                }

                if (mpn.name == MPN.body) {
                    LogUtil.Log("ACCexプリセットのbodyメニューの適用は現在未対応です。スキップします。", mpn.filename);
                    continue;
                }

                if (mpn.filename.EndsWith("_del.menu", StringComparison.OrdinalIgnoreCase)) {
                    if (castoff) {
                        // 対象のMPNが空でかつ、指定アイテムが削除アイテムと同一であればスキップ
                        if (prop.nFileNameRID == 0) {
                            if (CM3.dicDelItem[mpn.name].Equals(mpn.filename, StringComparison.OrdinalIgnoreCase)) {
                                continue;
                            }
                        // 空でなくとも同じアイテムであればスキップ
                        } else if (mpn.filename.Equals(prop.strFileName, StringComparison.OrdinalIgnoreCase)) {
                            continue;
                        }
                        // LogUtil.Debug("apply prop(del): ", mpn.filename, ", old:", prop.strFileName);
                        if (SetProp != null) SetProp(maid, mpn.name, mpn.filename, 0);
                    }
                    continue;
                    // } else if (mpn.filename.EndsWith(".mod", StringComparison.OrdinalIgnoreCase)) {
                }

                // LogUtil.Debug("apply prop: ", mpn.filename, ", old:", prop.strFileName);
                if (SetProp != null) SetProp(maid, mpn.name, mpn.filename, 0);
            }
        }

        public void ApplyPresetMPNProp(Maid maid, PresetData preset) {
            // 設定プロパティ反映
            foreach (var mpn in preset.mpnvals) {
                var mp = maid.GetProp(mpn.name);
                if (mp != null) {
                    mp.value = mpn.value;
                    if (mp.min > mpn.min) { mp.min = mpn.min; }
                    if (mp.max < mpn.max) { mp.max = mpn.max; }
                } else {
                    LogUtil.Debug("failed to apply MaidProp. mpn:", mpn.name);
                }
            }
        }

        public void ApplyPresetMaterial(Maid maid, PresetData preset) {
            if (maid == null) maid = _holder.CurrentMaid;
            if (maid == null) return;

            foreach (var ccslot in preset.slots) {
                var slotNo = (int)ccslot.id;
                if (slotNo >= maid.body0.goSlot.Count) continue; // スロットがないケースはスキップ

                // スロット上のマテリアル番号での判断に変更
                var slot = maid.body0.GetSlot(slotNo);
                var materials = _holder.GetMaterials(slot);
                if (slot.obj == null) {
                    LogUtil.Debug("slot.obj null. name=", ccslot.id);
                }
                if (!materials.Any()) continue; // 未装着スロットはスキップ

                var slotName = ccslot.id.ToString();
                var matNo = -1;
                foreach (var cmat in ccslot.materials) {
                    if (++matNo < materials.Length) {
                        var m = materials[matNo];
                        if (cmat.name != m.name) {
                            LogUtil.DebugF("Material name mismatched. skipping apply preset-slot={0}, matNo={1}, name=({2}<=>{3})",
                                     ccslot.id, matNo, cmat.name, m.name);
                            continue;
                        }
                        cmat.Apply(m);

                        // テクスチャ適用
                        var texes = cmat.texList;
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
                                if (_fileUtil.Exists(filename)) {
                                    maid.body0.ChangeTex(slotName, matNo, texInfo.propName, filename, null);

                                    // ChangeTexは、nameにファイル名が設定されてしまうため、拡張子を除いた名前を再設定
                                    var changedTex = m.GetTexture(texInfo.propName);
                                    if (changedTex != null) {
                                        changedTex.name = texInfo.texFile;
                                    }
                                } else {
                                    LogUtil.Debug("texture file not found. file=", filename);
                                }
                            }

                            if (texInfo.offsetX.HasValue || texInfo.offsetY.HasValue) {
                                var offset = m.GetTextureOffset(texInfo.propName);
                                if (texInfo.offsetX.HasValue) {
                                    offset.x = texInfo.offsetX.Value;
                                }
                                if (texInfo.offsetY.HasValue) {
                                    offset.y = texInfo.offsetY.Value;
                                }
                                m.SetTextureOffset(texInfo.propName, offset);
                            }

                            if (texInfo.scaleX.HasValue || texInfo.scaleY.HasValue) {
                                var scale = m.GetTextureScale(texInfo.propName);
                                if (texInfo.scaleX.HasValue) {
                                    scale.x = texInfo.scaleX.Value;
                                }
                                if (texInfo.scaleY.HasValue) {
                                    scale.y = texInfo.scaleY.Value;
                                }
                                m.SetTextureScale(texInfo.propName, scale);
                            }

                            // フィルタ適用
                            if (texInfo.filter == null) continue;
                            var fp = texInfo.filter.ToFilter();
                            _texModifier.ApplyFilter(maid, slotName, m, texInfo.propName, fp);
                        }
                    } else {
                        LogUtil.LogF("ACCPresetに指定されたマテリアル番号に対応するマテリアルが見つかりません。スキップします。 slot={0}, matNo={1}, name={2}",
                                     ccslot.id, matNo, cmat.name);
                        break;
                    }
                }
            }
        }
        // disable once MemberCanBeMadeStatic.Local
        public void ApplyPresetPartsColor(Maid maid, PresetData preset) {
            foreach (var pc in preset.partsColors) {
                try {
                    var partsColor = (MaidParts.PARTS_COLOR)Enum.Parse(typeof(MaidParts.PARTS_COLOR), pc.Key);
                    maid.Parts.SetPartsColor(partsColor, pc.Value.ToStruct());
                } catch (ArgumentException e) {
                    LogUtil.Debug(e);
                }
            }
        }

#if COM3D2
        private static readonly Action<Maid, MPN, string, int> SetProp = (maid, mpn, str, id) => {
            maid.SetProp(mpn, str, id);
        };
#else
        private static Action<Maid, MPN, string, int> SetProp;
        static PresetManager() {
            var typeObj = typeof(Maid);
            // 1.56以降
            var method = typeObj.GetMethod("SetProp", new[] { typeof(MPN), typeof(string), typeof(int), typeof(bool), typeof(bool) });
            if (method != null) {
                SetProp = (maid, mpn, str, id) => {
                    method.Invoke(maid, new object[] { mpn, str, id, false, false });
                };
                return;
            }
            // 1.xx ～ 1.55.1
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

            SetProp = (maid, mpn, str, id) => {
                LogUtil.Info("failed to apply preset(SetProp method) mpn=", mpn.name);
            };

            LogUtil.Error("failed to load Maid#SetProp method. Preset-feature dose not work properly.");
        }
#endif
    }
}
