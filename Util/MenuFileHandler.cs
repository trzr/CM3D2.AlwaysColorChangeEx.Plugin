using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {
    public class MenuFileHandler {

        public List<ChangeInfo> Parse(string filename) {
            if (!GameUty.FileSystem.IsExistentFile(filename)) return null;

            using (var reader = new BinaryReader(FileUtilEx.Instance.GetStream(filename), Encoding.UTF8)) {
                var header = reader.ReadString();
                if (header != "CM3D2_MENU") {
                    throw new Exception("header invalid. " + filename);
                }
                reader.ReadInt32();  // ver
                reader.ReadString(); // txtpath

                reader.ReadString(); // header name
                reader.ReadString(); // header category
                reader.ReadString(); // header desc

                reader.ReadInt32(); // length

                var changeItems = new List<ChangeInfo>();

//                string category;
                var loop = true;
                while (loop) {
                    var size = (int) reader.ReadByte();
                    if (size == 0) break;

                    var key = reader.ReadString();
                    var param = new string[size-1];
                    for (var i = 0; i < size-1; i++) {
                        param[i] = reader.ReadString();
                    }
                    key = key.ToLower();
                    switch(key) {
                        case "end":
                            loop = false;
                            break;
                        case "name":
                            // param[0] name
                            break;
                        case "setumei":
                            // parma[0] desc //《改行》
                            break;
                        case "color_set":
                            // parma[0] MPN (ToLower())
                            // param[1] m_strMenuNameInColorSet  [option](ToLower())
                            break;
                        case "icon":
                        case "icons":
                            // parma[0] file (tex)
                            break;
                        case "priority":
                            // param[0] float
                            break;
                        case "unsetitem":
                            // mi.m_boDelOnly = true
                            break;
                        case "メニューフォルダ":
                            // param[0] == "man" ToLower()
                            // mi.m_bMan = true
                            break;
                        case "アイテム":
                            // var filename = param[0]; // menu file
                            break;
                        case "アイテム条件":
                            // param[0] : slotname

                            // param[1] : に何か
                              // param[2] : 有る/無い
                              // param[3] : なら

                            // param[1] : が
                              // param[2] : modelファイル (存在しなくともよいが…）
                              // param[3] : なら
                              // param[4] : menu file

                            // param[1] : のアイテムパラメータの
                              // param[2] : tag . ToLower()
                              // param[3] : が
                              // param[4] : value
                              // param[5] : なら
                              // param[6] : menu file
                            break;
                        case "アイテムパラメータ":
                            if (param.Length == 3) {
                                // slot, tag, value
                                // ex) wear  特殊衣裳_メイド服 1
                            }
                            break;
                        case "半脱ぎ":
                            // key= 半脱ぎ
                            // param[0] value (menu file)
                            break;
                        case "リソース参照":
                            // param[0] key
                            // param[1] value (menu file)
                            break;
                        case "setslotitem":
                            // param[0] tag
                            // param[1] num(uint)
                            // maid.SetProp(tag, num, false)
                            break;
                        case "prop":
                            // param[0] tag
                            // param[1] num(int)
                            // maid.SetProp(tag, num, false)
                            break;
                        case "additem":
                            // param[0] model file
                            // param[1] slot

                            // param[2] アタッチ
                            // param[3] attachSlot
                            // param[4] attachName

                            // param[2] ボーンにアタッチ
                            // param[3] attachName [option]
                            // slotがhanditemr handitemlのいずれかであれば
                            // attachSlotとattachNameが自動で決定
//                            var modelitem = param[0].ToLower();
//                            var slot = param[1].ToLower();
//                            var model = fileMgr.GetOrAdd(modelitem, TargetExt.model);
//                            menu.AddChild(model, key + " (" + slot + ")");
//
//                            if (slot == "head") {
//                                var baseName = Path.GetFileNameWithoutExtension(model.filename);
//                                CheckSkinTex(baseName, model);
//                            }
//                            // slotとmodel fileの関係を保持 => texのRegex *の解決に利用
//                            modelDic[slot] = modelitem;

                            Add(changeItems, param[0]);
                            break;
                        case "saveitem":
                            // param[0] log出力の以前の行
                            break;
                        case "category":
                            // param[0] category
                            //   SceneEditではToLower()に対して、MPN.Parseを行う
//                            category = param[0];
                            break;
                        case "maskitem":
                            // param[0] maskSlot
                            // body.AddMask(category, maskSlot)
                            break;
                        case "delitem":
                            // param[0] slot [option] param[0]がない場合はcategoryのアイテムを削除
                            // mi.m_boDelOnly = true(sceneEdit)
                            Add(changeItems, param[0]);
                            break;
                        case "node消去":
                        case "node表示":
                            // param[0] nodeSlot
                            break;
                        case "パーツnode消去":
                        case "パーツnode表示":
                            // param[0] nodeSlot
                            // param[1] bone name
                            break;
                        case "color":
                            // param[0] name
                            // param[1] matNo
                            // param[2] propName
                            // param[3] color.R (float)
                            // param[4] color.G (float)
                            // param[5] color.B (float)
                            // param[6] color.A (float)
                            break;
                        case "mancolor":
                            // param[0] color.R (float)
                            // param[1] color.G (float)
                            // param[2] color.B (float)
                            break;

                        case "tex":
                        case "テクスチャ変更":
                            // param[0] slot
                            // param[1] matNo
                            // param[2] propName
                            // param[3] filename (tex)
                            // param[4] 無限色ID [option]
                            //  NONE/EYE_L/EYE_R/HAIR/EYE_BROW/UNDER_HAIR/SKIN/NIPPLE/MAX
                            //  NONE指定は param[4]無しと同様
//                            var texfile = param[3];
//                            if (!texfile.Contains("*")) {
//                                var tex1 = fileMgr.GetOrAdd(texfile, TargetExt.tex);
//                                menu.AddChild(tex1, key);
//                            } else {
//                                regTexes.Add(new RegTex(param[0], texfile, key));
//                                // すべて走査が終わった段階でモデルファイルとの対応付けを確認
//                            }
                            Add(changeItems, param[0], int.Parse(param[1]), param[2]);
                            break;
                        case "テクスチャ合成":
                        case "テクスチャセット合成":
                            // param[0] slot
                            // param[1] matNo(int)
                            // param[2] propName
                            // param[3] layerNo(int)
                            // param[4] file (tex)
                            // param[5] 合成指定(blendMode)
                            //     Alpha/Multiply/InfinityColor/TexTo8bitTex/Max
//                            var tex = fileMgr.GetOrAdd(param[4], TargetExt.tex);
//                            menu.AddChild(tex, key + " (" + param[0] + "[" + param[1] + "] : " + param[2] + ")");
//                            Add(changeItems, param[0], int.Parse(param[1]), param[2]);

                            break;
                        case "マテリアル変更":
                            // param[0] slot
                            // param[1] matNo(int)
                            // param[2] file (mate)
                            Add(changeItems, param[0], int.Parse(param[1]));

//                            var mate = fileMgr.GetOrAdd(param[2], TargetExt.mate);
//                            menu.AddChild(mate, key);
                            break;
                        case "shader":
                            // param[0] slot
                            // param[1] matNo
                            // param[2] shaderFileName
                            // TODO
                            Add(changeItems, param[0], int.Parse(param[1]));

                            break;
                        case "アタッチポイントの設定":
                            // param[0] アタッチポイント名
                            // param[1] vec.x (float)
                            // param[2] vec.y (float)
                            // param[3] vec.z (float)
                            // param[4] q.x (float)
                            // param[5] q.y (float)
                            // param[6] q.z (float)
                            // additemで指定したスロットへのアタッチ
                            break;
                        case "blendset":
                            // param[0] blendset名
                            // param... ブレンドセット分
                            break;
                        case "paramset":
                            // param[0] key
                            //
                            // body.Face.NewParamSet("param[0]" "param[1]" ...);
                            break;
                        case "commenttype":
                            // param[0] key
                            // param[1] val
                            break;
                        case "useredit":
                            // param[0] (unused)
                            // param[1] "material" であればマテリアルプロパティを設定
                            // param[2] slot
                            // param[3] matNo
                            // param[4] propName
                            // param[5] typeName
                            // param[6] value
                            // body.SetMaterialProperty(category,  slot, mateNo, propName, typeName, value, bool)
                            var slot = param[2];
                            Add(changeItems, slot, int.Parse(param[3]), param[4]);
                            break;
                        case "bonemorph":
                            // param[0] propName
                            // param[1] boneName
                            // param[2] min.x
                            // param[3] min.y
                            // param[4] min.z
                            // param[5] max.x
                            // param[6] max.y
                            // param[7] max.z
                            // body.bonemorph.ChangeMorphPosValue(propName, boneName, vec3Min, vec3Max)
                            break;
                        case "length":
                            // param[0] slot
                            // param[1] groupName
                            // param[2] boneSearchType
                            // param[3] boneName
                            // param[4] min.x
                            // param[5] mix.y
                            // param[6] min.z
                            // param[7] max.x
                            // param[8] max.y
                            // param[9] max.z
                            // body.SetHairLengthDataList(slot, groupName, boneSearchType, boneName, scaleMin, scaleMax)
                            break;
                        case "anime":
                            // param[0] slot
                            // param[1] anm file name
                            // (param[2] "loop" option)
//                            var anim = param[1];
//                            if (!anim.ToLower().EndsWith(".anm")) {
//                                anim += ".anm";
//                            }
//                            var anm = fileMgr.GetOrAdd(anim, TargetExt.anm);
//                            menu.AddChild(anm, key);
                            break;
                        // below: COM GP01?
                        case "param2":
                            // param[1] slot
                            // param[2] tagName
                            // param[3] tagValue
                            break;
                        case "animematerial":
                            // param[0] slot
                            // param[1] mateNo (int)
                            break;
                        case "ver":
                            // param[0] slot (unused)
                            // param[1] ver (int)
                            break;
                        case "if":
                            // param[0] maidprop[... or
                            // param[1] ==
                            // param[2] nothing
                            // param[3] ?
                            // param[4] setprop[...
                            // param[5] =
                            // param[6] getprop[...
                            break;
                        case "set":
                            break;
                        case "nofloory":
                            // param[0] slot

                            break;
                    }
                }
                return changeItems;
            }
        }
        public void Add(List<ChangeInfo> items, string slot, int matNo=-1, string propName=null) {

            foreach (var item in items) {
                if (item.slot == slot) {
                    item.Add(matNo, propName);
                    return;
                }
            }
            items.Add(new ChangeInfo(slot, matNo, propName));
        }

        public class ChangeInfo {
            public string slot;
            public List<MateInfo> matInfos;

            public ChangeInfo(string slot, int matNo=-1, string propName=null) {
                this.slot = slot;
                if (matNo != -1) {
                    matInfos = new List<MateInfo> {new MateInfo(matNo, propName)};
                }
            }

            public void Add(int matNo, string propName = null) {
                if (matInfos == null) return;

                if (matNo == -1) {
                    matInfos = null;
                    return;
                }

                foreach (var mate in matInfos) {
                    if (mate.matNo == matNo) {
                        mate.Add(propName);
                        return;
                    }
                }
                matInfos.Add(new MateInfo(matNo, propName));
            }
        }

        public class MateInfo {
            public int matNo;
            public List<string> propNames;
            public MateInfo(int matNo, string propName = null) {
                this.matNo = matNo;
                if (propName != null) {
                    propNames = new List<string> {propName};
                }
            }

            public void Add(string propName) {
                if (propNames == null) return;

                if (propName == null) {
                    propNames = null;
                } else {
                    if (propNames == null) {
                        propNames = new List<string>();
                    }
                    propNames.Add(propName);
                }
            }

            public override string ToString() {
                var sb = new StringBuilder("matNo=");
                sb.Append(matNo).Append(", propNames=").Append(propNames);
                return sb.ToString();
            }

        }
    }
}
