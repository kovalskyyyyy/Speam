using System;
using System.Collections.Generic;

namespace DialogueSystem.Runtime
{
    /// <summary>Рантайм-нода: только данные, никакой логики.</summary>
    [Serializable]
    public abstract class DialogueNodeData
    {
        /// <summary>Индекс следующей ноды в DialogueAsset.nodes, -1 = конец диалога.</summary>
        public int next = -1;
    }

    [Serializable]
    public class LineNodeData : DialogueNodeData
    {
        public SpeakerData speaker;
        public string text;
    }

    [Serializable]
    public class ChoiceOption
    {
        public string text;
        public DialogueCondition condition;   // null = вариант доступен всегда
        public int nextIndex = -1;
    }

    [Serializable]
    public class ChoiceNodeData : DialogueNodeData
    {
        public List<ChoiceOption> options = new List<ChoiceOption>();
    }

    [Serializable]
    public class ActionNodeData : DialogueNodeData
    {
        public ActionForNodes action;
    }

    [Serializable]
    public class EndNodeData : DialogueNodeData { }
}
