using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Segment")]
public class DialogueSegment : ScriptableObject
{
    public Line[] lines;
    public Choice[] choices;
    [Tooltip("Оставить пустым, если конец диалога или есть выборы")]
    public DialogueSegment next;

    public bool HasChoices => choices != null && choices.Length > 0;
}

[Serializable]
public class Line
{
    [TextArea(1,4)] public string text;
    public Sprite speaker;
}

[Serializable]
public class Choice
{
    public string text;
    public DialogueSegment target;
}