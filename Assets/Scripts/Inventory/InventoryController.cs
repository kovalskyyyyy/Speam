using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using CustomDebug;

namespace Inventory
{
    public class InventoryController : MonoBehaviour
    {
        public static InventoryController Instance { get; private set; }

        public event Action<ItemData> OnItemAdded;
        public event Action<ItemData> OnItemRemoved;

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
        [SerializeField] private bool m_dockLeft = true;

        [Header("Items")]
        [SerializeField] private List<ItemData> m_startingItems = new();
        [SerializeField] private ItemData m_testItem;

        private readonly List<ItemData> m_items = new();
        private InventorySlotUI[] m_slots;
        private RectTransform m_panelRect;
        private Coroutine m_slideCoroutine;
        private Vector2 m_openPos;
        private Vector2 m_closedPos;
        private bool m_isOpen;

        public bool IsOpen => m_isOpen;
        public IReadOnlyList<ItemData> Items => m_items;
        public int Count => m_items.Count;
        public int Capacity => m_slotCapacity;

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
                m_closedPos = new Vector2(m_dockLeft ? -width : width, 0f);

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

        public bool HasItem(ItemData item) => item != null && m_items.Contains(item);

        public bool HasItem(string itemId) =>
            m_items.Exists(i => i != null && (i.Id == itemId || i.name == itemId));

        public bool AddItem(ItemData item)
        {
            if (item == null) return false;
            if (HasItem(item)) return false;
            if (m_items.Count >= m_slotCapacity) return false;

            m_items.Add(item);
            UpdateUI();
            OnItemAdded?.Invoke(item);
            return true;
        }

        public bool RemoveItem(ItemData item)
        {
            if (item == null) return false;
            if (!m_items.Remove(item)) return false;

            UpdateUI();
            OnItemRemoved?.Invoke(item);
            return true;
        }

        public bool RemoveItem(string itemId)
        {
            ItemData item = m_items.Find(i => i != null && (i.Id == itemId || i.name == itemId));
            return item != null && RemoveItem(item);
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

        private void UpdateTabVisuals()
        {
            if (m_toggleTabText != null)
            {
                if (m_dockLeft)
                    m_toggleTabText.text = m_isOpen ? "◀" : "▶";
                else
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
                    m_slots[i].SetItem(m_items[i]);
                }
                else
                {
                    m_slots[i].Clear();
                }
            }
        }

        #if UNITY_EDITOR
        [Button]
        private void TestToggle() => Toggle();

        [Button]
        private void TestAddItem()
        {
            ItemData itemToUse = m_testItem != null ? m_testItem : (m_startingItems.Count > 0 ? m_startingItems[0] : null);
            if (itemToUse != null) AddItem(itemToUse);
        }

        [Button]
        private void TestRemoveItem()
        {
            ItemData itemToUse = m_testItem != null ? m_testItem : (m_items.Count > 0 ? m_items[0] : null);
            if (itemToUse != null) RemoveItem(itemToUse);
        }
        #endif
    }
}
