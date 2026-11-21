using System.Collections.Generic;
using UnityEngine;
using DialogueSystem.Runtime;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Saves
{
    /// <summary>Справочник "id -> DialogueAsset". Нужен, чтобы по строке из сейва найти ассет.</summary>
    [CreateAssetMenu(menuName = "Dialogue/Dialogue Database", fileName = "DialogueDatabase")]
    public class DialogueDatabase : ScriptableObject
    {
        [SerializeField] List<DialogueAsset> dialogues = new();
        Dictionary<string, DialogueAsset> map;

        public DialogueAsset Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (map == null)
            {
                map = new Dictionary<string, DialogueAsset>();
                foreach (var d in dialogues)
                {
                    if (d == null || string.IsNullOrEmpty(d.id)) continue;
                    if (!map.TryAdd(d.id, d))
                        Debug.LogWarning($"Дубликат id диалога: {d.id} ({d.name})", d);
                }
            }
            return map.TryGetValue(id, out var result) ? result : null;
        }

#if UNITY_EDITOR
        // ПКМ по заголовку компонента/ассета -> "Collect all dialogues"
        [ContextMenu("Collect all dialogues")]
        void Collect()
        {
            dialogues.Clear();
            foreach (var guid in AssetDatabase.FindAssets("t:DialogueAsset"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<DialogueAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null) dialogues.Add(asset);
            }
            map = null;
            EditorUtility.SetDirty(this);
            Debug.Log($"DialogueDatabase: собрано диалогов — {dialogues.Count}", this);
        }

        void OnValidate() => map = null;
#endif
    }
}
