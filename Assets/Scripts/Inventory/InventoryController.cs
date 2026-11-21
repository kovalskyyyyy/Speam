using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using CustomDebug;

namespace Inventory
{
    public enum ItemType
    {
        None = 0,
        Key,
        OldNote,
        Flashlight,
        Crowbar,
        Photo,
        Matches,
        Lighter,
        Diary,
        Screwdriver,
        RustyKey
    }

    [Serializable]
    public struct ItemVisual
    {
        public ItemType type;
        public Sprite icon;
    }

    public class InventoryController : MonoBehaviour
    {
        public static InventoryController Instance { get; private set; }

        public event Action<ItemType> OnItemAdded;
        public event Action<ItemType> OnItemRemoved;

        [Header("UI Panels & Controls")]
        [SerializeField] private GameObject m_inventoryPanel;
        [SerializeField] private RectTransform m_slotsParent;
        [SerializeField] private Button m_closeButton;
        [SerializeField] private Button m_toggleButton;
        [SerializeField] private Text m_toggleTabText;
        [SerializeField] private Text m_titleText;

        [Header("Settings")]
        [SerializeField] private int m_slotCapacity = 10;
        [SerializeField] private bool m_startOpen = false;
        [SerializeField] private bool m_toggleWithKey = true;
        [SerializeField] private float m_slideDuration = 0.25f;

        [Header("Items")]
        [SerializeField] private List<ItemType> m_startingItems = new();
        [SerializeField] private List<ItemVisual> m_itemVisuals = new();

        public bool IsOpen => m_isOpen;
        public IReadOnlyList<ItemType> Items => m_items;
        public int Count => m_items.Count;
        public int Capacity => m_slotCapacity;

        public bool HasItem(ItemType item) => item != ItemType.None && m_items.Contains(item);

        public bool AddItem(ItemType item)
        {
            if (item == ItemType.None) return false;
            if (HasItem(item)) return false;
            if (m_items.Count >= m_slotCapacity) return false;

            m_items.Add(item);
            UpdateUI();
            OnItemAdded?.Invoke(item);
            return true;
        }

        public bool RemoveItem(ItemType item)
        {
            if (!m_items.Remove(item)) return false;

            UpdateUI();
            OnItemRemoved?.Invoke(item);
            return true;
        }

        public void Toggle()
        {
            if (m_isOpen) Close();
            else Open();
        }

        public void Open()
        {
            m_isOpen = true;
            UpdateTabVisuals();
            SlideTo(m_openPos);
        }

        public void Close()
        {
            m_isOpen = false;
            UpdateTabVisuals();
            SlideTo(m_closedPos);
        }

        private readonly List<ItemType> m_items = new();
        private InventorySlotUI[] m_slots;
        private RectTransform m_panelRect;
        private Coroutine m_slideCoroutine;
        private Vector2 m_openPos;
        private Vector2 m_closedPos;
        private bool m_isOpen;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureEventSystem();
            InitUI();
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
        }

        private void Start()
        {
            for (int i = 0; i < m_startingItems.Count; i++)
            {
                AddItem(m_startingItems[i]);
            }
        }

        private void Update()
        {
            if (!m_toggleWithKey) return;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.iKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame)
                {
                    Toggle();
                }
            }
        }

        private void InitUI()
        {
            if (m_inventoryPanel != null)
            {
                m_panelRect = m_inventoryPanel.GetComponent<RectTransform>();
            }

            if (m_slotsParent != null)
            {
                m_slots = m_slotsParent.GetComponentsInChildren<InventorySlotUI>(false);
            }
            else
            {
                m_slots = GetComponentsInChildren<InventorySlotUI>(false);
            }

            if (m_panelRect != null)
            {
                float width = m_panelRect.rect.width > 0 ? m_panelRect.rect.width : 230f;
                m_openPos = Vector2.zero;
                m_closedPos = new Vector2(width, 0f);

                m_isOpen = m_startOpen;
                m_panelRect.anchoredPosition = m_isOpen ? m_openPos : m_closedPos;
            }

            UpdateTabVisuals();

            if (m_closeButton != null)
                m_closeButton.onClick.AddListener(Close);

            if (m_toggleButton != null)
                m_toggleButton.onClick.AddListener(Toggle);

            UpdateUI();
        }


        private void UpdateTabVisuals()
        {
            if (m_toggleTabText != null)
            {
                m_toggleTabText.text = m_isOpen ? "▶" : "◀";
            }
        }

        private void SlideTo(Vector2 targetPos)
        {
            if (m_panelRect == null) return;

            if (m_slideCoroutine != null)
                StopCoroutine(m_slideCoroutine);

            m_slideCoroutine = StartCoroutine(SlideRoutine(targetPos));
        }

        private IEnumerator SlideRoutine(Vector2 targetPos)
        {
            Vector2 startPos = m_panelRect.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < m_slideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / m_slideDuration);
                t = Mathf.SmoothStep(0f, 1f, t);
                m_panelRect.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, t);
                yield return null;
            }

            m_panelRect.anchoredPosition = targetPos;
            m_slideCoroutine = null;
        }

        private void UpdateUI()
        {
            if (m_slots == null) return;

            for (int i = 0; i < m_slots.Length; i++)
            {
                if (i < m_items.Count)
                {
                    ItemType type = m_items[i];
                    m_slots[i].SetItem(type, GetItemIcon(type));
                }
                else
                {
                    m_slots[i].Clear();
                }
            }
        }

        private Sprite GetItemIcon(ItemType type)
        {
            for (int i = 0; i < m_itemVisuals.Count; i++)
            {
                if (m_itemVisuals[i].type == type)
                    return m_itemVisuals[i].icon;
            }
            return null;
        }

        #if UNITY_EDITOR
        [Button]
        private void TestToggle() => Toggle();

        [Button]
        private void TestAddKey() => AddItem(ItemType.Key);

        [Button]
        private void TestAddNote() => AddItem(ItemType.OldNote);

        [Button]
        private void TestRemoveKey() => RemoveItem(ItemType.Key);
        #endif
    }
}
