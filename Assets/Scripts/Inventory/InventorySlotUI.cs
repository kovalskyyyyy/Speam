using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image m_backgroundImage;
        [SerializeField] private Image m_iconImage;
        [SerializeField] private Text m_quantityText;
        [SerializeField] private Button m_button;

        public ItemType CurrentItem { get; private set; } = ItemType.None;
        public bool IsOccupied => CurrentItem != ItemType.None;
        public Button Button => m_button;

        public void SetItem(ItemType type, Sprite icon)
        {
            CurrentItem = type;

            if (icon != null)
            {
                if (m_iconImage != null)
                {
                    m_iconImage.sprite = icon;
                    m_iconImage.gameObject.SetActive(true);
                }
                if (m_quantityText != null)
                {
                    m_quantityText.gameObject.SetActive(false);
                }
            }
            else
            {
                // Fallback for testing before sprites are assigned: show item name
                if (m_iconImage != null)
                {
                    m_iconImage.gameObject.SetActive(false);
                }
                if (m_quantityText != null)
                {
                    m_quantityText.text = type.ToString();
                    m_quantityText.gameObject.SetActive(true);
                }
            }
        }

        public void Clear()
        {
            CurrentItem = ItemType.None;

            if (m_iconImage != null)
            {
                m_iconImage.sprite = null;
                m_iconImage.gameObject.SetActive(false);
            }

            if (m_quantityText != null)
            {
                m_quantityText.gameObject.SetActive(false);
            }
        }
    }
}
