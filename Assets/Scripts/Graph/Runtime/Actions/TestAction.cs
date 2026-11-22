using UnityEngine;
using System.Collections;

namespace DialogueSystem.Runtime
{
    [CreateAssetMenu(menuName = "Dialogue/Actions/Wait", fileName = "NewWaitAction")]
    public class TestAction : ActionForNodes
    {
        [SerializeField] float seconds = 2f;
        public override void Act() { }
        public override IEnumerator Execute()
        {
            Debug.Log($"WaitAction: жду {seconds} с");
            yield return new WaitForSecondsRealtime(seconds);
            Debug.Log("WaitAction: готово");
        }
    }
}

