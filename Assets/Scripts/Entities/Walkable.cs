using UnityEngine;
using UnityEngine.AI;

namespace Entities
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Walkable : MonoBehaviour
    {
        private NavMeshAgent m_agent;


        public void GoToPoint(Vector3 point)
        {
            m_agent.SetDestination(point);
        }

        public void GoToEntity()
        {
            
        }

    private void Awake()
        {
            m_agent = GetComponent<NavMeshAgent>();
        }
    }
}