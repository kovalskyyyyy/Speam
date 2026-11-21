using UnityEngine;
using System.Collections;

namespace DialogueSystem.Runtime
{
    public abstract class ActionForNodes : ScriptableObject
    {
        public abstract void Act();

        public virtual IEnumerator Execute()
        {
            Act();
            yield break;
        }
    }
}