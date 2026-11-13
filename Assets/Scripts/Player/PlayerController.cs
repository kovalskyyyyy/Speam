using Player.States;
using StateMachine;
using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        private StateMachine<PlayerState> m_machine;

        private void Awake()
        {
            PlayerContext data = new PlayerContext();
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
