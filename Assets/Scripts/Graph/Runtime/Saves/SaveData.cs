using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saves
{
    // JsonUtility не умеет Dictionary и HashSet -> в сейве только List.

    [Serializable]
    public class NpcState
    {
        public string id;           // уникальный ID НПС (для стоящих на сцене)
        public string prefabId;     // ID префаба (для НПС, которых спавнит код)
        public Vector3 position;
        public Quaternion rotation;
        public string dialogueId;   // DialogueAsset.id текущего диалога
    }

    [Serializable]
    public class QuestState
    {
        public string questId;
        public int stage;
        public bool completed;
    }

    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public string sceneName;
        public string savedAt;
        public Vector3 playerPosition;

        public List<NpcState> npcs = new();
        public List<string> completedDialogues = new();  // id пройденных диалогов
        public List<string> doneActions = new();         // id выполненных action-нод
        public List<string> flags = new();               // произвольные флаги: "gate_opened"
        public List<QuestState> quests = new();
    }

    /// <summary>Рантайм-состояние игры. Всё, что должно пережить сохранение, живёт здесь.</summary>
    public static class GameState
    {
        public static readonly HashSet<string> CompletedDialogues = new();
        public static readonly HashSet<string> DoneActions = new();
        public static readonly HashSet<string> Flags = new();
        public static readonly Dictionary<string, QuestState> Quests = new();

        public static event Action Changed;

        // Диалоги
        public static void MarkDialogueDone(string id)
        { if (!string.IsNullOrEmpty(id) && CompletedDialogues.Add(id)) Changed?.Invoke(); }
        public static bool IsDialogueDone(string id) => CompletedDialogues.Contains(id);

        // Выполненные action-ноды
        public static void MarkActionDone(string id)
        { if (!string.IsNullOrEmpty(id) && DoneActions.Add(id)) Changed?.Invoke(); }
        public static bool IsActionDone(string id) => DoneActions.Contains(id);

        // Флаги
        public static void SetFlag(string flag)
        { if (!string.IsNullOrEmpty(flag) && Flags.Add(flag)) Changed?.Invoke(); }
        public static bool HasFlag(string flag) => Flags.Contains(flag);

        // Квесты
        public static QuestState GetOrCreateQuest(string questId)
        {
            if (!Quests.TryGetValue(questId, out var q))
                Quests[questId] = q = new QuestState { questId = questId };
            return q;
        }
        public static void SetQuestStage(string questId, int stage)
        { GetOrCreateQuest(questId).stage = stage; Changed?.Invoke(); }
        public static void CompleteQuest(string questId)
        { GetOrCreateQuest(questId).completed = true; Changed?.Invoke(); }
        public static bool IsQuestCompleted(string id) => Quests.TryGetValue(id, out var q) && q.completed;

        public static void Clear()
        {
            CompletedDialogues.Clear(); DoneActions.Clear(); Flags.Clear(); Quests.Clear();
        }
    }
}
