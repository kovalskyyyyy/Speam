using System;
using Unity.GraphToolkit.Editor;

namespace DialogueSystem.Editor
{
    /// <summary>Начало диалога. Только выход.</summary>
    [Serializable]
    internal class StartNode : DialogueNodeBase
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddExecutionOutput(context);
        }
    }
}
