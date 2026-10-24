using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Larje.Core.UI
{
    public sealed class VirtualScreen : GraphicRaycaster
    {
        private Func<PointerEventData, Hit> _resolve;

        public bool IsPointerOnScreen { get; private set; }
        public Vector2 PointerPosition { get; private set; }

        public void Initialize(Func<PointerEventData, Hit> resolve)
        {
            _resolve = resolve;
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (_resolve == null)
            {
                base.Raycast(eventData, resultAppendList);
                return;
            }

            Hit hit = _resolve(eventData);
            IsPointerOnScreen = hit.IsHit;
            if (hit.IsHit)
            {
                PointerPosition = hit.TextureCoord;
                eventData.position = hit.TextureCoord;
                base.Raycast(eventData, resultAppendList);
            }
        }

        public readonly struct Hit
        {
            public readonly bool IsHit;
            public readonly Vector2 TextureCoord;

            public Hit(bool isHit, Vector2 textureCoord)
            {
                IsHit = isHit;
                TextureCoord = textureCoord;
            }
        }
    }
}
