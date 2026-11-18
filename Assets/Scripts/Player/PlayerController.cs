using Player.States;
using StateMachine;
using UnityEngine;
using Entities;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(Walkable))]
    public class PlayerController : MonoBehaviour
    {
        public StateMachine<PlayerState> stateMachine => m_machine;
        [SerializeField]
        private Camera camera; // TODO: Get used of this SerializeField and 
                               // make camera finding with code

        private StateMachine<PlayerState> m_machine;

        private void Awake()
        {
            PlayerContext data = new PlayerContext();
            data.camera = camera;
            data.walkControl = GetComponent<Walkable>();
            data.inputReader = new InputReader();
            
            
            m_machine = new StateMachine<PlayerState>();
            m_machine.AddState(new PlayableState(m_machine, data));
        
            //m_machine.AddState(new BotRoamingState(m_machine, data));
            //m_machine.AddState(new BotChaseState(m_machine, data));
            //m_machine.AddState(new BotAttackState(m_machine, data, m_attacks));

            m_machine.ChangeState<PlayableState>();
        }

        private void Update()
        {
            m_machine.StateUpdate();
        }
        private void FixedUpdate()
        {
            m_machine.FixedStateUpdate();
        }
        
    }
}
