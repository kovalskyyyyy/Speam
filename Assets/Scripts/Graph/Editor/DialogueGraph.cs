using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace DialogueSystem.Editor
{
    [Graph(AssetExtension)]
    [Serializable]
    internal class DialogueGraph : Graph
    {
        public const string AssetExtension = "dialogue";

        [MenuItem("Assets/Create/Dialogue/Dialogue Graph", false)]
        static void CreateAssetFile()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<DialogueGraph>();
        }
    }
}