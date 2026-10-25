using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Larje.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PDWPanel : MaskableGraphic
    {
        private const string SHADER_NAME = "Hidden/PDW/Panel";

        private static Material s_sharedMaterial;

        [Header("Colors")]
        [SerializeField] private Color fillColor = Color.white;
        [SerializeField] private Color borderColor = Color.black;

        [Header("Shape")]
        [SerializeField, Min(0f)] private float borderPx = 0f;
        [SerializeField, Min(0f)] private float softnessPx = 0f;

        [SerializeField] private List<CornerOptions> cornerOptions = new List<CornerOptions>() {
            new CornerOptions() {
                corners = new List<CornerType>() { CornerType.BottomLeft, CornerType.TopLeft, CornerType.TopRight, CornerType.BottomRight },
                radiusPx = 10f
            }
        };

        public override Material materialForRendering
        {
            get
            {
                EnsureMaterial();

                Material mat = s_sharedMaterial;
                return GetModifiedMaterial(mat);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            EnsureMaterial();

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
                canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord2;
                canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord3;
                canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Tangent;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EnsureMaterial();
            SetAllDirty();
        }

        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            EnsureMaterial();
            SetAllDirty();
        }
        #endif

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Rect r = GetPixelAdjustedRect();

            Vector2 size = new Vector2(Mathf.Max(1f, r.width), Mathf.Max(1f, r.height));
            size.x *= transform.lossyScale.x;
            size.y *= transform.lossyScale.y;

            UIVertex v = UIVertex.simpleVert;
            v.color = color;
            v.uv1 = fillColor;
            v.uv2 = borderColor;
            v.uv3 = new Vector4(borderPx, softnessPx, size.x, size.y);
            v.tangent = new Vector4(GetCornerRadius(0), GetCornerRadius(1), GetCornerRadius(2), GetCornerRadius(3));

            v.position = new Vector3(r.xMin, r.yMin);
            v.uv0 = new Vector4(0f, 0f, 0f, 0f);
            vh.AddVert(v);

            v.position = new Vector3(r.xMin, r.yMax);
            v.uv0 = new Vector4(0f, 1f, 0f, 0f);
            vh.AddVert(v);

            v.position = new Vector3(r.xMax, r.yMax);
            v.uv0 = new Vector4(1f, 1f, 0f, 0f);
            vh.AddVert(v);

            v.position = new Vector3(r.xMax, r.yMin);
            v.uv0 = new Vector4(1f, 0f, 0f, 0f);
            vh.AddVert(v);

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        private void EnsureMaterial()
        {
            if (s_sharedMaterial == null)
            {
                Shader shader = Shader.Find(SHADER_NAME);
                if (shader == null)
                {
                    return;
                }

                s_sharedMaterial = new Material(shader)
                {
                    name = $"PDW Panel Shared",
                    hideFlags = HideFlags.HideAndDontSave
                };
            }
        }

        private float GetCornerRadius(int cornerIndex)
        {
            float radiusPx = 0f;

            CornerOptions corner = cornerOptions?.Find(c => c.corners.Contains((CornerType)cornerIndex));
            if (corner != null)
            {
                radiusPx = corner.radiusPx;
            }
            
            return radiusPx;
        }

        public enum CornerType
        {
            BottomLeft = 0,
            TopLeft = 1,
            TopRight = 2,
            BottomRight = 3
        }
        
        [Serializable]
        public sealed class CornerOptions
        {
            public List<CornerType> corners;
            public float radiusPx;
        }
    }
}
