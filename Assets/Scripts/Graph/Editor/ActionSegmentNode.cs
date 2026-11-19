using System;
using DialogueSystem.Runtime;
using Unity.GraphToolkit.Editor;

namespace DialogueSystem.Editor
{
    [Serializable]
    internal class ActionSegmentNode : DialogueNodeBase
    {
        public const string IN_ACTION = "Action";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddExecutionInput(context);
            AddExecutionOutput(context);

            context.AddInputPort<ActionForNodes>(IN_ACTION)
                .WithDisplayName("Action")
                .Build();
        }
    }
}
