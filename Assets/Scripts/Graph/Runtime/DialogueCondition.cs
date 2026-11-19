using UnityEngine;

namespace DialogueSystem.Runtime
{
    /// <summary>
    /// Базовый класс условия для варианта выбора. Делайте наследников под свою игру,
    /// например: "цветов в инвентаре >= 100" или "сейчас ночь".
    /// </summary>
    public abstract class DialogueCondition : ScriptableObject
    {
        public abstract bool IsMet();
    }
}
