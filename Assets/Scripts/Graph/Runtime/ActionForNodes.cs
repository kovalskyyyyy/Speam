using UnityEngine;
using System.Collections;

namespace DialogueSystem.Runtime
{
    public abstract class ActionForNodes : ScriptableObject
    {
        [SerializeField, HideInInspector] string id;
        public string Id => id;

#if UNITY_EDITOR
        void OnValidate()
        {
            var guid = UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(this));
            if (id != guid) { id = guid; UnityEditor.EditorUtility.SetDirty(this); }
        }
#endif
        public abstract void Act();

        public virtual IEnumerator Execute()
        {
            Act();
            yield break;
        }
    }
}