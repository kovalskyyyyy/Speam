using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Inventory
{
    /// <summary>
    /// Справочник "id -> ItemData". Позволяет по строковому идентификатору из файла сохранения
    /// быстро находить соответствующий ассет предмета.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<ItemData> m_items = new();

        private Dictionary<string, ItemData> m_map;
        private static ItemDatabase s_instance;

        public IReadOnlyList<ItemData> AllItems => m_items;

        public ItemData Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (m_map == null)
            {
                BuildMap();
            }

            if (m_map.TryGetValue(id, out var result))
            {
                return result;
            }

#if UNITY_EDITOR
            // Фоллбэк в редакторе: если предмет создан, но база ещё не была обновлена через Collect
            foreach (var guid in AssetDatabase.FindAssets("t:ItemData"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (item != null && (item.Id == id || item.name == id))
                {
                    if (!m_items.Contains(item))
                    {
                        m_items.Add(item);
                    }
                    m_map[id] = item;
                    return item;
                }
            }
#endif

            return null;
        }

        private void BuildMap()
        {
            m_map = new Dictionary<string, ItemData>();
            for (int i = 0; i < m_items.Count; i++)
            {
                var item = m_items[i];
                if (item == null) continue;

                if (!string.IsNullOrEmpty(item.Id))
                {
                    if (!m_map.TryAdd(item.Id, item))
                        Debug.LogWarning($"[ItemDatabase] Дубликат ID предмета: '{item.Id}' ({item.name})", item);
                }

                if (!string.IsNullOrEmpty(item.name) && item.name != item.Id)
                {
                    m_map.TryAdd(item.name, item);
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Collect all items")]
        public void Collect()
        {
            m_items.Clear();
            foreach (var guid in AssetDatabase.FindAssets("t:ItemData"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (asset != null && !m_items.Contains(asset))
                {
                    m_items.Add(asset);
                }
            }
            m_map = null;
            EditorUtility.SetDirty(this);
            Debug.Log($"[ItemDatabase] Собрано предметов: {m_items.Count}", this);
        }

        [MenuItem("Tools/Inventory/Update Item Database")]
        public static void UpdateItemDatabaseAsset()
        {
            var db = Resources.Load<ItemDatabase>("ItemDatabase");
            if (db == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                db = CreateInstance<ItemDatabase>();
                AssetDatabase.CreateAsset(db, "Assets/Resources/ItemDatabase.asset");
            }
            db.Collect();
            AssetDatabase.SaveAssets();
            Debug.Log("[ItemDatabase] Ассет обновлен по пути Assets/Resources/ItemDatabase.asset");
        }

        private void OnValidate() => m_map = null;
#endif

        /// <summary>
        /// Возвращает синглтон-экземпляр базы предметов из Resources или создаёт рантайм-экземпляр.
        /// </summary>
        public static ItemDatabase GetOrCreate()
        {
            if (s_instance != null) return s_instance;

            s_instance = Resources.Load<ItemDatabase>("ItemDatabase");
            if (s_instance == null)
            {
                s_instance = CreateInstance<ItemDatabase>();
#if UNITY_EDITOR
                s_instance.Collect();
#endif
            }
            return s_instance;
        }
    }
}
