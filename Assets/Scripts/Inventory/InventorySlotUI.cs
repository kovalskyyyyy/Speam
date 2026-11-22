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

        public ItemData CurrentItem { get; private set; }
        public bool IsOccupied => CurrentItem != null;
        public Button Button => m_button;

        public void SetItem(ItemData item)
        {
            CurrentItem = item;

            if (item != null && item.Icon != null)
            {
                if (m_iconImage != null)
                {
                    m_iconImage.sprite = item.Icon;
                    m_iconImage.gameObject.SetActive(true);
                }
                if (m_quantityText != null)
                {
                    m_quantityText.gameObject.SetActive(false);
                }
            }
            else if (item != null)
            {
                // Fallback for testing before sprites are assigned: show item name
                if (m_iconImage != null)
                {
                    m_iconImage.gameObject.SetActive(false);
                }
                if (m_quantityText != null)
                {
                    m_quantityText.text = item.ItemName;
                    m_quantityText.gameObject.SetActive(true);
                }
            }
            else
            {
                Clear();
            }
        }

        public void Clear()
        {
            CurrentItem = null;

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
