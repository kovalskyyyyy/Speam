using System;
using System.Collections.Generic;
using System.IO;
using DialogueSystem.Runtime;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Saves
{
    /// <summary>
    /// Хранит и применяет сохранения. Загрузкой сцен занимается GameManager,
    /// поэтому здесь только чтение/запись/применение данных.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [SerializeField] DialogueDatabase dialogueDb;

        [Tooltip("Префабы НПС, которые спавнятся кодом (у каждого свой prefabId).")]
        [SerializeField] List<NpcSaveable> npcPrefabs = new();

        string PathFor(int slot) => Path.Combine(Application.persistentDataPath, $"save_{slot}.json");

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        static Transform FindPlayer()
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            return go != null ? go.transform : null;
        }

        // ---------------- SAVE ----------------

        public bool Save(int slot = 0)
        {
            // Сохраняемся только между диалогами: позицию внутри диалога мы не храним.
            if (DialogueRunner.Instance != null && DialogueRunner.Instance.IsRunning)
            {
                Debug.LogWarning("Нельзя сохраниться во время диалога.");
                return false;
            }

            var player = FindPlayer();
            var data = new SaveData
            {
                sceneName = SceneManager.GetActiveScene().name,
                savedAt = DateTime.Now.ToString("s"),
                playerPosition = player != null ? player.position : Vector3.zero
            };

            foreach (var npc in FindObjectsByType<NpcSaveable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                data.npcs.Add(npc.Capture());

            data.completedDialogues.AddRange(GameState.CompletedDialogues);
            data.doneActions.AddRange(GameState.DoneActions);
            data.flags.AddRange(GameState.Flags);
            data.quests.AddRange(GameState.Quests.Values);

            // Пишем во временный файл, потом подменяем: вылет посреди записи не убьёт старый сейв.
            string path = PathFor(slot);
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, JsonUtility.ToJson(data, true));
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);

            Debug.Log($"Saved: {path}");
            return true;
        }

        // ---------------- LOAD ----------------

        public bool HasSave(int slot = 0) => File.Exists(PathFor(slot));

        /// <summary>Читает сейв. Возвращает null, если файла нет или он повреждён.</summary>
        public SaveData Read(int slot = 0)
        {
            if (!HasSave(slot)) return null;

            try
            {
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(PathFor(slot)));
                if (data == null)
                {
                    Debug.LogError("Сейв пустой или повреждён.");
                    return null;
                }
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Сейв повреждён: {e}");
                return null;
            }
        }

        /// <summary>Применяет сейв к уже загруженной сцене.</summary>
        public void Apply(SaveData data)
        {
            if (data == null) return;

            // 1. Глобальное состояние
            GameState.Clear();

            if (data.completedDialogues != null)
                foreach (var d in data.completedDialogues) GameState.CompletedDialogues.Add(d);

            if (data.doneActions != null)
                foreach (var a in data.doneActions) GameState.DoneActions.Add(a);

            if (data.flags != null)
                foreach (var f in data.flags) GameState.Flags.Add(f);

            if (data.quests != null)
                foreach (var q in data.quests)
                    if (q != null && !string.IsNullOrEmpty(q.questId))
                        GameState.Quests[q.questId] = q;

            // 2. Игрок
            var player = FindPlayer();
            if (player != null)
            {
                player.position = data.playerPosition;

                var agent = player.GetComponentInChildren<NavMeshAgent>();
                if (agent != null) agent.Warp(data.playerPosition);
            }

            // 3. НПС, которые уже стоят на сцене
            var existing = new Dictionary<string, NpcSaveable>();
            foreach (var npc in FindObjectsByType<NpcSaveable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (!string.IsNullOrEmpty(npc.id))
                    existing[npc.id] = npc;

            var restored = new HashSet<string>();

            if (data.npcs != null)
            {
                foreach (var state in data.npcs)
                {
                    if (state == null) continue;

                    if (!existing.TryGetValue(state.id, out var npc))
                    {
                        // На сцене такого нет — спавним из префаба
                        var prefab = npcPrefabs.Find(p => p != null && p.prefabId == state.prefabId);
                        if (prefab == null)
                        {
                            Debug.LogWarning($"Нет префаба для НПС '{state.id}' (prefabId '{state.prefabId}')");
                            continue;
                        }

                        npc = Instantiate(prefab, state.position, state.rotation);
                        npc.id = state.id;
                    }

                    npc.Restore(state, dialogueDb);
                    restored.Add(state.id);
                }
            }

            // 4. Есть на сцене, но нет в сейве => этого НПС убрали/убили до сохранения
            foreach (var kv in existing)
                if (!restored.Contains(kv.Key))
                    Destroy(kv.Value.gameObject);
        }
    }
}
