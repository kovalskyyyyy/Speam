using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using DialogueSystem.Runtime;

namespace Saves
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [SerializeField] DialogueDatabase dialogueDb;

        [Tooltip("Префабы НПС, которые спавнятся кодом (у каждого свой prefabId).")] [SerializeField]
        List<NpcSaveable> npcPrefabs = new();

        SaveData pendingLoad; // применим после загрузки сцены

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
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
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

        public void Load(int slot = 0)
        {
            if (!HasSave(slot))
            {
                Debug.LogWarning("Сейва нет.");
                return;
            }

            try
            {
                pendingLoad = JsonUtility.FromJson<SaveData>(File.ReadAllText(PathFor(slot)));
            }
            catch (Exception e)
            {
                Debug.LogError($"Сейв повреждён: {e}");
                return;
            }

            // Грузим сцену, а данные применяем в sceneLoaded (когда объекты новой сцены уже созданы).
            SceneManager.LoadScene(pendingLoad.sceneName);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (pendingLoad == null) return;
            Apply(pendingLoad);
            pendingLoad = null;
        }

        void Apply(SaveData data)
        {
            // 1. Глобальное состояние
            GameState.Clear();
            foreach (var d in data.completedDialogues) GameState.CompletedDialogues.Add(d);
            foreach (var a in data.doneActions) GameState.DoneActions.Add(a);
            foreach (var f in data.flags) GameState.Flags.Add(f);
            foreach (var q in data.quests) GameState.Quests[q.questId] = q;

            // 2. Игрок
            var player = FindPlayer();
            if (player != null) player.position = data.playerPosition;

            // 3. НПС, которые уже стоят на сцене
            var existing = new Dictionary<string, NpcSaveable>();
            foreach (var npc in FindObjectsByType<NpcSaveable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (!string.IsNullOrEmpty(npc.id))
                    existing[npc.id] = npc;

            var restored = new HashSet<string>();

            foreach (var state in data.npcs)
            {
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

            // 4. Есть на сцене, но нет в сейве => этого НПС убрали/убили до сохранения
            foreach (var kv in existing)
                if (!restored.Contains(kv.Key))
                    Destroy(kv.Value.gameObject);
        }
    }
}