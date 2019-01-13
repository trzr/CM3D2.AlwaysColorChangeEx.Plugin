using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Render {
    ///
    /// ボーン描画クラス.
    ///  Updateにより位置を適宜更新する.
    ///
    public class CustomBoneRenderer {//: MonoBehaviour {
        #region Fields
        private const string NAME_LINE_PREFIX = "___LINE_";
        private const string NAME_SCL = "_SCL_";
        private readonly Vector3 UNIT_VECTOR3 = new Vector3(-0.1f, 0f, 0f);

        private readonly Dictionary<string, LineRenderer> _lineDict = new Dictionary<string, LineRenderer>();
        private readonly Dictionary<string, List<LineRenderer>> _offsetlineDict = new Dictionary<string, List<LineRenderer>>();
        private readonly List<GameObject> _cache = new List<GameObject>();

        private Material _lineMaterial;
        private Material _sublineMaterial;
        private readonly float _lineWidth = 0.006f;
        private Color _color = Color.white;
        public Color Color {
            get { return _color;}
            set {
                _color = value;
                _lineMaterial.color = _color;
                SetColor(ref _color);
            }
        }
        private Color _offColor = new Color(0.6f, 0.6f, 0.6f);
        public Color OffColor {
            get { return _offColor; }
            set { _offColor = value; }
        }
        public int ItemID { get; private set; }

        private SkinnedMeshRenderer _meshRenderer;
        private Transform _rootBone;
        private readonly HashSet<string> _boneNames = new HashSet<string>();
        private bool _isVisible;
        private bool _skipVisible;
        #endregion

        ~CustomBoneRenderer() {
            ClearCache();
        }

        /// 指定されたメイドのインスタンスID
        public int TargetId { get; set; }

        public bool IsEnabled() {
            return _meshRenderer != null && _rootBone != null;
        }

        public bool IsVisible() {
            return _isVisible;
        }

        public void SetVisible(bool visible) {
            if (_isVisible != visible) {
                SetVisibleAll(visible);
            }
            _isVisible = visible;
        }

        private void SetVisibleAll(bool visible) {
            foreach (var obj in _cache) {
                obj.SetActive(visible);
            }
        }

        public void Setup(GameObject go, int id=-1) {
            ItemID = id;
            Clear();
            _boneNames.Clear();

            _meshRenderer = go.GetComponentInChildren<SkinnedMeshRenderer>(false);
            if (_meshRenderer == null) return;
            if (_lineMaterial == null) {
                _lineMaterial = CreateMaterial();
                _lineMaterial.color = _color;
            }
            if (_sublineMaterial == null) {
                _sublineMaterial = CreateMaterial();
                _sublineMaterial.color = _offColor;
            }

            foreach (var bone in _meshRenderer.bones) {
                if (bone == null) continue;
                // 名前の末尾に‗_SCL_がついていた場合は、_SCL_を省略
                _boneNames.Add(bone.name.EndsWith(NAME_SCL)
                    ? bone.name.Substring(0, bone.name.Length - NAME_SCL.Length)
                    : bone.name);
            }
            var parentBone = _meshRenderer.rootBone;
            if (parentBone != null) {
                _rootBone = parentBone;
                SetupBone(parentBone);
            } else {
                parentBone = go.transform;
                foreach (Transform child in parentBone) {
                    if (child.childCount == 0) continue;

                    var before = _lineDict.Count;
                    SetupBone(child);
                    _rootBone = child;

                    if (_lineDict.Count - before > 1) break; // 有効な子ノードを抽出した場合に終了
                }
            }

            foreach (var obj in _cache) {
                obj.SetActive(_isVisible);
            }
        }

        private void SetupBone(Transform bone) {
            if (_lineDict.ContainsKey(bone.name)) return;

            var lineRenderer = CreateComponent(_lineMaterial, _lineWidth, _lineWidth * 0.2f);
            lineRenderer.gameObject.name = NAME_LINE_PREFIX + bone.name;
            _lineDict.Add(bone.name, lineRenderer);

            // meshRender.bonesに含まれないボーンを色替え
            if (!_boneNames.Contains(bone.name)) {
                lineRenderer.materials = new[] { _sublineMaterial, };
            }

            foreach (Transform child in bone) {
                if (child.childCount == 0) continue;
                if (child.name.StartsWith(ConstantValues.NAME_PREFIX)) continue;

                SetupBone(child);
            }
        }

        private void UpdateVisible(bool visible) {
            if (_skipVisible != visible) return;

            _skipVisible = !visible;
            SetVisibleAll(visible);
        }

        public void Update() {
            if (_rootBone == null) {
                if (_isVisible) SetVisible(false);
                return;
            }

            if (!_meshRenderer.gameObject.activeSelf) {
                // 一時非表示
                UpdateVisible(false);
                return;
            } 
            // 一時非表示からの復帰
            UpdateVisible(true);

            if (_rootBone.gameObject.activeSelf) {
                UpdatePosition(_rootBone, true, false);
            }

            foreach (Transform child in _rootBone) {
                if (child.childCount == 0) continue;

                if (child.gameObject.activeSelf) {
                    UpdatePosition(child, false, true);
                }
            }
        }

        private void EmptyBone(LineRenderer renderer) {
#if UNITY_5_6_OR_NEWER
            renderer.positionCount = 0;
#else
            renderer.SetVertexCount(0);
#endif
            renderer.enabled = false;
            // if (_isFirst) Log.Debug(renderer.name, " is leaf");
        }

        public void UpdatePosition(Transform bone, bool isRoot=false, bool isRecursive=false) {
            LineRenderer boneLine;
            if (!_lineDict.TryGetValue(bone.name, out boneLine)) return;

            if (bone.childCount == 0) {
                EmptyBone(boneLine);
                return;
            }
            boneLine.SetPosition(0, bone.position);

            Vector3? pos = null;
            if (bone.childCount == 1) {
                var child0 = bone.GetChild(0);
                if (child0.name.StartsWith(ConstantValues.NAME_PREFIX)) {
                    EmptyBone(boneLine);
                    return;
                }
                pos = child0.position;
                if (pos == bone.position) {
                    pos = bone.position + bone.rotation * UNIT_VECTOR3;
                }
            } else {
                if (bone.childCount == 2) {
                    var child0 = bone.GetChild(0);
                    var child1 = bone.GetChild(1);
                    if (child0.name.EndsWith(NAME_SCL) || child0.name.StartsWith(ConstantValues.NAME_PREFIX)) {
                        pos = child1.position;
                    } else if (child1.name.EndsWith(NAME_SCL) || child1.name.StartsWith(ConstantValues.NAME_PREFIX)) {
                        pos = child0.position;
                    }
                }
                if (!pos.HasValue) {
                    var maxLength = 0.1f;
                    if (!isRoot) {
                        foreach (Transform child in bone) {
                            if (child.name.StartsWith(ConstantValues.NAME_PREFIX)) continue;
                            var delta = child.position - bone.position;
                            var length = delta.magnitude;

                            if (length > maxLength) maxLength = length;
                        }
                    }

                    pos = bone.position + bone.rotation * new Vector3(-maxLength, 0f, 0f);

                    // オフセットラインを表示
                    List<LineRenderer> offsetLines;
                    if (!_offsetlineDict.TryGetValue(bone.name, out offsetLines)) {
                        offsetLines = new List<LineRenderer>();
                        _offsetlineDict[bone.name] = offsetLines;

                        foreach (Transform child in bone) {
                            if (child.name.StartsWith(ConstantValues.NAME_PREFIX)) continue;

                            var line = CreateComponent(_sublineMaterial, _lineWidth * 0.1f, _lineWidth * 0.1f);
                            offsetLines.Add(line);
                            line.gameObject.name = "offsetLine";
                            line.gameObject.SetActive(_isVisible);
                        }
                    }
                    var idx = 0;
                    foreach (Transform child in bone) {
                        if (child.name.StartsWith(ConstantValues.NAME_PREFIX)) continue;
                        var line = offsetLines[idx++];
                        line.SetPosition(0, pos.Value);
                        line.SetPosition(1, child.position);
                    }
                }
            }
            boneLine.SetPosition(1, pos.Value);

            if (!isRecursive) return;
            foreach (Transform childBone in bone) {
                UpdatePosition(childBone, false, true);
            }
        }

        public void Clear() {
            _lineDict.Clear();
            _offsetlineDict.Clear();
            ClearCache();
            _rootBone = null;
            TargetId = -1;
        }

        private void ClearCache() {
            foreach (var obj in _cache) {
                Object.Destroy(obj);
            }
            _cache.Clear();
        }

        private void SetColor(ref Color color1) {
            foreach (var boneName in _boneNames) {
                LineRenderer renderer;
                if (_lineDict.TryGetValue(boneName, out renderer)) {
                    renderer.material.color = color1;
                }
            }
        }

        public void SetRQ(int rq) {
            foreach (var obj in _cache) {
                var render = obj.GetComponent<LineRenderer>();
                render.material.renderQueue = rq;
            }
        }

        private LineRenderer CreateComponent(Material material, float startWidth, float endWidth) {
            var obj = new GameObject();
            _cache.Add(obj);

            var line = obj.AddComponent<LineRenderer>();
            line.materials = new[] { material, };
#if UNITY_5_6_OR_NEWER
            line.startWidth = startWidth;
            line.endWidth   = endWidth;
            line.positionCount = 2;
#else            
            line.SetWidth(startWidth, endWidth);
            line.SetVertexCount(2);
#endif
            return line;
        }

        private Material CreateMaterial() {
            var shader = Shader.Find("Hidden/Internal-Colored");
            var material = new Material(shader) {
                hideFlags = HideFlags.HideAndDontSave
            };
            material.SetInt("_ZTest",    (int)UnityEngine.Rendering.CompareFunction.Disabled);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_Cull",     (int)UnityEngine.Rendering.CullMode.Off);
            material.SetInt("_ZWrite", 0);
            material.renderQueue = 5000;
            return material;
        }
    }
}
