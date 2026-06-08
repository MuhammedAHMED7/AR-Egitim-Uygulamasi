using UnityEngine;

namespace ARApp.Core.ContextualLabel
{
    [CreateAssetMenu(fileName = "NewContextualLabelData", menuName = "AR Education/Contextual Label Data")]
    public class ContextualLabelData : ScriptableObject
    {
        [Header("UI Metin Ýçerikleri")]
        [SerializeField] private string title;
        [SerializeField] private string concept;
        [TextArea(3, 5)][SerializeField] private string description;

        public string Title => title;
        public string Concept => concept;
        public string Description => description;
    }
}