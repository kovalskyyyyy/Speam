using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item")]
    public class ItemData : ScriptableObject
    {
        [SerializeField] private string m_id;
        [SerializeField] private string m_itemName;
        [TextArea(2, 4)]
        [SerializeField] private string m_description;
        [SerializeField] private Sprite m_icon;

        public string Id => string.IsNullOrEmpty(m_id) ? name : m_id;
        public string ItemName => string.IsNullOrEmpty(m_itemName) ? name : m_itemName;
        public string Description => m_description;
        public Sprite Icon => m_icon;
    }
}
