using System;
using Unity.GraphToolkit.Editor;

namespace DialogueSystem.Editor
{
    /// <summary>Конец диалога. Только вход.</summary>
    [Serializable]
    internal class EndNode : DialogueNodeBase
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddExecutionInput(context);
        }
    }
}
