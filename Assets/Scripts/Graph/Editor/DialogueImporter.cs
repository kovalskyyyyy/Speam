using System;
using System.Collections.Generic;
using System.Linq;
using DialogueSystem.Runtime;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace DialogueSystem.Editor
{
    /// <summary>Превращает граф диалога (.dialogue) в игровой DialogueAsset при каждом сохранении.</summary>
    [ScriptedImporter(2, DialogueGraph.AssetExtension)]
    internal class DialogueImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var graph = GraphDatabase.LoadGraphForImporter<DialogueGraph>(ctx.assetPath);
            if (graph == null)
            {
                Debug.LogError($"Не удалось загрузить граф диалога: {ctx.assetPath}");
                return;
            }

            var start = graph.GetNodes().OfType<StartNode>().FirstOrDefault();
            if (start == null)
            {
                Debug.LogWarning($"В графе {ctx.assetPath} нет Start-ноды.");
                return;
            }

            var asset = ScriptableObject.CreateInstance<DialogueAsset>();
            asset.id = AssetDatabase.AssetPathToGUID(ctx.assetPath);
            Build(start, asset);

            ctx.AddObjectToAsset("RuntimeAsset", asset);
            ctx.SetMainObject(asset);
        }

        static void Build(INode start, DialogueAsset asset)
        {
            var indexOf = new Dictionary<INode, int>();
            var order = new List<INode>();
            var queue = new Queue<INode>();

            // Проход 1: найти все достижимые ноды и создать для каждой рантайм-данные.
            void Visit(INode node)
            {
                if (node == null || indexOf.ContainsKey(node)) return;
                indexOf[node] = asset.nodes.Count;
                asset.nodes.Add(Translate(node));
                order.Add(node);
                queue.Enqueue(node);
            }

            var first = Next(start, DialogueNodeBase.EXEC_OUT);
            Visit(first);
            asset.startIndex = first != null ? indexOf[first] : -1;

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node is ChoiceSegmentNode)
                {
                    for (int i = 0; i < ChoiceSegmentNode.OptionCount; i++)
                        Visit(Next(node, ChoiceSegmentNode.OutPort(i)));
                }
                else
                {
                    Visit(Next(node, DialogueNodeBase.EXEC_OUT));
                }
            }

            // Проход 2: расставить ссылки (теперь индексы всех нод известны).
            int IndexOf(INode n) => n != null && indexOf.TryGetValue(n, out var i) ? i : -1;

            foreach (var node in order)
            {
                var runtime = asset.nodes[indexOf[node]];
                if (runtime is ChoiceNodeData choice)
                {
                    for (int i = 0; i < ChoiceSegmentNode.OptionCount; i++)
                        choice.options[i].nextIndex = IndexOf(Next(node, ChoiceSegmentNode.OutPort(i)));
                }
                else
                {
                    runtime.next = IndexOf(Next(node, DialogueNodeBase.EXEC_OUT));
                }
            }
        }

        static DialogueNodeData Translate(INode node)
        {
            switch (node)
            {
                case DialogueSegmentNode segment:
                    return new LineNodeData
                    {
                        speaker = GetInputPortValue<SpeakerData>(segment.GetInputPortByName(DialogueSegmentNode.IN_SPEAKER)),
                        text = GetInputPortValue<string>(segment.GetInputPortByName(DialogueSegmentNode.IN_TEXT))
                    };

                case ChoiceSegmentNode choiceNode:
                    var data = new ChoiceNodeData();
                    for (int i = 0; i < ChoiceSegmentNode.OptionCount; i++)
                    {
                        data.options.Add(new ChoiceOption
                        {
                            text = GetInputPortValue<string>(choiceNode.GetInputPortByName(ChoiceSegmentNode.TextPort(i))),
                            condition = GetInputPortValue<DialogueCondition>(choiceNode.GetInputPortByName(ChoiceSegmentNode.ConditionPort(i)))
                        });
                    }
                    return data;
                
                case ActionSegmentNode actionNode:
                    return new ActionNodeData
                    {
                    action = GetInputPortValue<ActionForNodes>(actionNode.GetInputPortByName(ActionSegmentNode.IN_ACTION)),
                    };

                case EndNode _:
                    return new EndNodeData();

                default:
                    throw new ArgumentException($"Неподдерживаемая нода: {node.GetType()}");
            }
        }

        // У нод без выходного порта (EndNode) вернётся null: поэтому "?." везде.
        static INode Next(INode node, string portName)
        {
            return node.GetOutputPortByName(portName)?.FirstConnectedPort?.GetNode();
        }

        /// <summary>Значение порта: из подключённой ноды-константы/переменной, иначе встроенное в порт. Взято из сэмпла.</summary>
        static T GetInputPortValue<T>(IPort port)
        {
            T value = default;

            if (port.IsConnected)
            {
                switch (port.FirstConnectedPort.GetNode())
                {
                    case IVariableNode variableNode:
                        variableNode.Variable.TryGetDefaultValue<T>(out value);
                        return value;
                    case IConstantNode constantNode:
                        constantNode.TryGetValue<T>(out value);
                        return value;
                }
            }
            else
            {
                port.TryGetValue(out value);
            }

            return value;
        }
    }
}
