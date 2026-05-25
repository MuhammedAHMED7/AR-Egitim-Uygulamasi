using UnityEngine;

namespace AREgitim.VR
{
    /// <summary>
    /// Bir objeye eklendiğinde, VRRayHoverHinter o objenin üzerine geldiğinde
    /// 'tooltipText' içeriğini ekrana yansıtır.
    /// </summary>
    public class VRHoverInfo : MonoBehaviour
    {
        [Tooltip("Hover anında gösterilecek metin")]
        [TextArea(1, 3)]
        public string tooltipText = "";
    }
}
