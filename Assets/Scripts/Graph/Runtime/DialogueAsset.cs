using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.Runtime
{
    /// <summary>Игровая версия графа диалога. Создаётся импортёром автоматически.</summary>
    public class DialogueAsset : ScriptableObject
    {
        public string id;
        [SerializeReference] public List<DialogueNodeData> nodes = new List<DialogueNodeData>();
        public int startIndex = -1;
    }
}
