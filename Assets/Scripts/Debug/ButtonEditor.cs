using System.Reflection;

using UnityEngine;
using UnityEditor;


namespace CustomDebug {

    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class ButtonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var methods = target.GetType().GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<ButtonAttribute>() == null) continue;
                if (method.GetParameters().Length > 0) continue;

                if (GUILayout.Button(ObjectNames.NicifyVariableName(method.Name)))
                {
                    foreach (var t in targets)
                        method.Invoke(t, null);
                }
            }
        }
    }
    
}
