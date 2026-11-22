using UnityEngine;
using Saves;
namespace DialogueSystem.Runtime
{
    /// <summary>Action-нода: ставит флаг. Флаги сохраняются и читаются условиями/миром.</summary>
    [CreateAssetMenu(menuName = "Dialogue/Actions/Set Flag", fileName = "SetFlag")]
    public class SetFlagAction : ActionForNodes
    {
        [SerializeField] string flag;
        public override void Act() => GameState.SetFlag(flag);
    }
}
