using System;
using Unity.GraphToolkit.Editor;

namespace DialogueSystem.Editor
{
    /// <summary>
    /// Общий предок всех нод диалога: добавляет порты "выполнения" (входной и выходной).
    /// ВНИМАНИЕ: способ создания портов без типа (AddInputPort(name)) я предположил по аналогии
    /// с сэмплом. Сверьте с VisualNovelNode.AddInputOutputExecutionPorts и поправьте, если нужно.
    /// </summary>
    [Serializable]
    internal abstract class DialogueNodeBase : Node
    {
        public const string EXEC_IN = "In";
        public const string EXEC_OUT = "Next";

        protected static void AddExecutionInput(IPortDefinitionContext context)
        {
            context.AddInputPort(EXEC_IN).Build();
        }

        protected static void AddExecutionOutput(IPortDefinitionContext context)
        {
            context.AddOutputPort(EXEC_OUT).Build();
        }
    }
}