using UnityEngine;
using Saves;
namespace DialogueSystem.Runtime
{
    /// <summary>Action-нода: переключает НПС на другой диалог (например, после разговора).</summary>
    [CreateAssetMenu(menuName = "Dialogue/Actions/Set NPC Dialogue", fileName = "SetNpcDialogue")]
    public class SetNpcDialogueAction : ActionForNodes
    {
        [SerializeField] string npcId;
        [SerializeField] DialogueAsset newDialogue;

        public override void Act()
        {
            var npc = NpcSaveable.Find(npcId);
            if (npc == null) { Debug.LogWarning($"SetNpcDialogue: НПС '{npcId}' не найден."); return; }
            npc.SetDialogue(newDialogue);
        }
    }
}
