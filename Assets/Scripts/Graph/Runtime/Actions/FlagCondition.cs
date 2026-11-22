using UnityEngine;
using Saves;
namespace DialogueSystem.Runtime
{
    /// <summary>Условие для варианта выбора: флаг установлен (или НЕ установлен, если invert).</summary>
    [CreateAssetMenu(menuName = "Dialogue/Conditions/Flag", fileName = "FlagCondition")]
    public class FlagCondition : DialogueCondition
    {
        [SerializeField] string flag;
        [SerializeField] bool invert;
        public override bool IsMet() => GameState.HasFlag(flag) != invert;
    }
}
