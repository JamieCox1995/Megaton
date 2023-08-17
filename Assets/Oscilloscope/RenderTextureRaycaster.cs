using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    [AddComponentMenu("Event/RenderTexture Raycaster")]
    [RequireComponent(typeof(Canvas))]
    public class RenderTextureRaycaster : GraphicRaycaster
    {
        [SerializeField]
        private Camera m_eventCameraOverride;
        [SerializeField]
        private string m_texturePropertyName = "_MainTex";

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (m_eventCameraOverride != null)
            {
                Ray ray = m_eventCameraOverride.ScreenPointToRay(eventData.position);

                bool foundRenderTexture = false;
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    Renderer[] renderers = hit.transform.gameObject.GetComponents<Renderer>();
                    
                    foreach (var renderer in renderers)
                    {
                        Material[] materials = renderer.sharedMaterials;

                        foreach (var material in materials)
                        {
                            var texture = material.GetTexture(m_texturePropertyName);

                            if (texture == eventCamera.targetTexture)
                            {
                                eventData.position = eventCamera.ViewportToScreenPoint(hit.textureCoord);
                                base.Raycast(eventData, resultAppendList);
                                foundRenderTexture = true;
                                break;
                            }
                        }
                        if (foundRenderTexture) break;
                    }
                }
            }
            else
            {
                base.Raycast(eventData, resultAppendList);
            }
        }
    }
}