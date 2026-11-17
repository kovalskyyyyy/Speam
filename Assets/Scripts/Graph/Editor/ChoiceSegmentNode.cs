using System;
using DialogueSystem.Runtime;
using Unity.GraphToolkit.Editor;

namespace DialogueSystem.Editor
{
    /// <summary>
    /// Выбор из до 4 вариантов. У каждого варианта: текст, необязательное условие и свой выход.
    /// Вариант с пустым текстом в игре не показывается.
    /// </summary>
    [Serializable]
    internal class ChoiceSegmentNode : DialogueNodeBase
    {
        public const int OptionCount = 4;

        public static string TextPort(int i) => $"Option{i + 1}Text";
        public static string ConditionPort(int i) => $"Option{i + 1}Condition";
        public static string OutPort(int i) => $"Option{i + 1}Out";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddExecutionInput(context);

            for (int i = 0; i < OptionCount; i++)
            {
                context.AddInputPort<string>(TextPort(i))
                    .WithDisplayName($"Option {i + 1}")
                    .Build();
                context.AddInputPort<DialogueCondition>(ConditionPort(i))
                    .WithDisplayName($"Condition {i + 1}")
                    .WithTooltip("Необязательно. Если условие не выполнено, вариант скрыт.")
                    .Build();
                context.AddOutputPort(OutPort(i))
                    .WithDisplayName($"Go {i + 1}")
                    .Build();
            }
        }
    }
}
