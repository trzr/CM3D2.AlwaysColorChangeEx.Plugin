using CM3D2.AlwaysColorChangeEx.Plugin.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Render
{
    ///
    /// ボーン描画クラス.
    ///  ボーンにアタッチさせた方が恐らく容易だが、極力オリジナルのオブジェクトにアタッチを避けて実装.
    ///
    public class CustomBoneRenderer //: MonoBehaviour
    {
        #region Fields
        private const string NAME_PREFIX = "___LINE_";
        private const string NAME_SCL = "_SCL_";

        private readonly Dictionary<string, LineRenderer> _lineDict = new Dictionary<string, LineRenderer>();
        private readonly List<GameObject> _cache = new List<GameObject>();

        private Material _lineMaterial;
        private readonly float _lineWidth = 0.006f;
        private readonly Color _color = Color.white;

        private Transform _rootBone;
        private bool _isVisible;
        private bool _skipVisble;
        #endregion

        /// 指定されたメイドのインスタンスID
        private int _targetId;
        public int TargetId {
            get { return _targetId; }
            set { _targetId = value;  }
        }

        public bool IsEnabled() {
            return _rootBone != null;
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

        public void Setup(Transform bone) {
            Clear();
            _rootBone = bone;

            foreach (Transform child in bone) {
                if (child.childCount == 0) continue;
                SetupBone(child);
            }

            foreach (var obj in _cache) {
                obj.SetActive(_isVisible);
            }
        }

        private void SetupBone(Transform bone) {
            if (_lineDict.ContainsKey(bone.name)) return;

            var lineRenderer = CreateComponent();
            lineRenderer.gameObject.name = NAME_PREFIX + bone.name;
            _lineDict.Add(bone.name, lineRenderer);

            foreach (Transform child in bone) {
                SetupBone(child);
            }
        }

        private void UpdateVisible(bool visible) {
            if (_skipVisble != visible) return;

            _skipVisble = !visible;
            SetVisibleAll(visible);
        }

        public void Update() {
            if (_rootBone == null) {
                if (_isVisible) SetVisible(false);
                return;
            }
            if (!_rootBone.gameObject.activeSelf) {
                // 一時非表示
                UpdateVisible(false);
                return;
            } 
            // 一時非表示からの復帰
            UpdateVisible(true);

            foreach (Transform child in _rootBone) {
                if (child.childCount == 0) continue;

                if (child.gameObject.activeSelf) {
                    UpdatePosition(child, true);
                }
            }
        }

        public void UpdatePosition(Transform bone, bool isRoot=false) {
//            if (bone.name.StartsWith(NAME_PREFIX)) return;
            LineRenderer boneLine;
            if (!_lineDict.TryGetValue(bone.name, out boneLine)) return;

            if (bone.childCount == 0) {
#if UNITY_5_6_OR_NEWER
                boneLine.positionCount = 0;
#else
                boneLine.SetVertexCount(0);
#endif
                boneLine.enabled = false;
                return;
            }
#if UNITY_5_6_OR_NEWER
            boneLine.positionCount = 2;
#else
            boneLine.SetVertexCount(2);
#endif
            boneLine.SetPosition(0, bone.position);

            Vector3? pos = null;
            if (bone.childCount == 1) {
                var child0 = bone.GetChild(0);
                pos = child0.position;
                if (pos == bone.position) {
                    var vec = new Vector3(-0.1f, 0f, 0f);
                    var loc = bone.rotation * vec;
                    pos = bone.position + loc;
                }
            } else {
                if (bone.childCount == 2) {
                    if (bone.GetChild(0).name.EndsWith(NAME_SCL)) {
                        pos = bone.GetChild(1).position;
                    } else if (bone.GetChild(1).name.EndsWith(NAME_SCL)) {
                        pos = bone.GetChild(0).position;
                    }
                }
                if (!pos.HasValue) {
                    var maxLength = 0f;
                    if (!isRoot) {
                        foreach (Transform child in bone) {
                            var delta = child.position - bone.position;
                            var length = delta.magnitude;

                            if (length > maxLength) maxLength = length;
                        }
                    } else {
                        maxLength = 0.1f;
                    }

                    var vec = new Vector3(-maxLength, 0f, 0f);
                    var loc = bone.rotation * vec;
                    pos = bone.position + loc;
                }
            }
            boneLine.SetPosition(1, pos.Value);

            foreach (Transform childBone in bone) {
                UpdatePosition(childBone);
            }
        }

        public void Clear() {
            _lineDict.Clear();
            ClearCache();
            _rootBone = null;
            _targetId = -1;
        }

        private void ClearCache() {
            foreach (var obj in _cache) {
                UnityEngine.Object.Destroy(obj);
            }
            _cache.Clear();
        }

        private LineRenderer CreateComponent() {

            if (_lineMaterial == null) {
                var shader = Shader.Find("Hidden/Internal-Colored");
                _lineMaterial = new Material(shader) {
                    hideFlags = HideFlags.HideAndDontSave
                };
                _lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Disabled);
                _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                _lineMaterial.SetInt("_ZWrite", 0);
                _lineMaterial.renderQueue = 5000;
            }
            _lineMaterial.color = _color;
            var obj = new GameObject();
            _cache.Add(obj);

            var line = obj.AddComponent<LineRenderer>();
            line.materials = new[] { _lineMaterial, };
#if UNITY_5_6_OR_NEWER
            line.startWidth = _lineWidth;
            line.endWidth   = _lineWidth * 0.2f;
#else            
            line.SetWidth(_lineWidth, _lineWidth*0.20f);
#endif
            return line;
        }
    }
}