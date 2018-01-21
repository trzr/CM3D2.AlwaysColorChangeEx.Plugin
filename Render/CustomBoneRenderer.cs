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

        private Dictionary<string, LineRenderer> lineDict = new Dictionary<string, LineRenderer>();
        private List<GameObject> cache = new List<GameObject>();

        private Material lineMaterial;
        private float lineWidth = 0.006f;
        private Color color = Color.white;

        private Transform rootBone;
        private bool isVisible = false;
        private bool skipVisble = false;
        #endregion

        /// 指定されたメイドのインスタンスID
        private int _targetId;
        public int TargetId {
            get { return _targetId; }
            set { _targetId = value;  }
        }

        public bool IsEnabled() {
            return rootBone != null;
        }

        public void SetVisible(bool visible) {
            if (isVisible != visible) {
                SetVisibleAll(visible);
            }
            isVisible = visible;
        }

        private void SetVisibleAll(bool visible) {
            foreach (var obj in cache) {
                obj.SetActive(visible);
            }
        }

        public void Setup(Transform bone) {
            Clear();
            rootBone = bone;

            foreach (Transform child in bone) {
                if (child.childCount == 0) continue;
                SetupBone(child);
            }

            foreach (var obj in cache) {
                obj.SetActive(isVisible);
            }
        }

        private void SetupBone(Transform bone) {
            if (lineDict.ContainsKey(bone.name)) return;

            var lineRenderer = CreateComponent();
            lineRenderer.gameObject.name = NAME_PREFIX + bone.name;
            lineDict.Add(bone.name, lineRenderer);

            foreach (Transform child in bone) {
                SetupBone(child);
            }
        }
        private void UpdateVisible(bool visible) {
            if (skipVisble == visible) {
                skipVisble = !visible;
                SetVisibleAll(visible);
            }
        }
        public void Update() {
            if (rootBone == null) {
                if (isVisible) SetVisible(false);
                return;
            }
            if (!rootBone.gameObject.activeSelf) {
                // 一時非表示
                UpdateVisible(false);
                return;
            } else {
                // 一時非表示からの復帰
                UpdateVisible(true);
            }

            foreach (Transform child in rootBone) {
                if (child.childCount == 0) continue;

                if (child.gameObject.activeSelf) {
                    UpdatePosition(child, true);
                }
            }
        }

        public void UpdatePosition(Transform bone, bool isRoot = false) {
            if (bone.name.StartsWith(NAME_PREFIX)) return;
            LineRenderer boneLine;
            if (!lineDict.TryGetValue(bone.name, out boneLine)) return;

            if (bone.childCount == 0) {
                boneLine.SetVertexCount(0);
                boneLine.enabled = false;
                return;
            }
            boneLine.SetVertexCount(2);
            boneLine.SetPosition(0, bone.position);

            Vector3? pos = null;
            if (bone.childCount == 1) {
                var child0 = bone.GetChild(0);
                pos = child0.position;
                if (pos == bone.position) {
                    Vector3 vec = new Vector3(-0.1f, 0f, 0f);
                    var loc = bone.rotation * vec;
                    pos = bone.position + loc;
                }
            } else {
                if (bone.childCount == 2) {
                    Transform child = null;
                    if (bone.GetChild(0).name.EndsWith(NAME_SCL)) {
                        child = bone.GetChild(1);
                    } else if (bone.GetChild(1).name.EndsWith(NAME_SCL)) {
                        child = bone.GetChild(0);
                    }
                    if (child != null) {
                        pos = child.position;
                    }
                }
                if (!pos.HasValue) {
                    float maxLength = 0;
                    if (!isRoot) {
                        for (var i = 0; i < bone.childCount; i++) {
                            var child = bone.GetChild(i);
                            var delta = child.position - bone.position;
                            var length = delta.magnitude;

                            if (length > maxLength) maxLength = length;
                        }
                    } else {
                        maxLength = 0.1f;
                    }

                    Vector3 vec = new Vector3(-maxLength, 0f, 0f);
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
            lineDict.Clear();
            ClearCache();
            rootBone = null;
            _targetId = -1;
        }

        private void ClearCache() {
            foreach (var obj in cache) {
                UnityEngine.Object.Destroy(obj);
            }
            cache.Clear();
        }

        private LineRenderer CreateComponent() {
            if (lineMaterial == null) {
                var shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader) {
                    hideFlags = HideFlags.HideAndDontSave
                };
                lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Disabled);
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                lineMaterial.SetInt("_ZWrite", 0);
                lineMaterial.renderQueue = 5000;
            }
            lineMaterial.color = color;
            var obj = new GameObject();
            cache.Add(obj);

            var line = obj.AddComponent<LineRenderer>();
            line.materials = new Material[] { lineMaterial, };
            line.SetWidth(lineWidth, lineWidth * 0.15f);
            return line;
        }
    }
}