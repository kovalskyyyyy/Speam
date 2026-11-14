using System;
using DialogueSystem.Runtime;
using Unity.GraphToolkit.Editor;

namespace DialogueSystem.Editor
{
    /// <summary>Реплика: говорящий (SpeakerData) + текст, затем переход к следующей ноде.</summary>
    [Serializable]
    internal class DialogueSegmentNode : DialogueNodeBase
    {
        public const string IN_SPEAKER = "Speaker";
        public const string IN_TEXT = "Text";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddExecutionInput(context);
            AddExecutionOutput(context);

            context.AddInputPort<SpeakerData>(IN_SPEAKER)
                .WithDisplayName("Speaker")
                .Build();
            context.AddInputPort<string>(IN_TEXT)
                .AsTextArea(5, 8)
                .Build();
        }
    }
}
