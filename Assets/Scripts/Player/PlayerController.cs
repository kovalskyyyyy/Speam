using Player.States;
using StateMachine;
using Animations;
using CustomDebug;

using UnityEngine;
using Entities;
using Unity.VisualScripting;

namespace Player
{
    [RequireComponent(typeof(Walkable))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CharacterAnimationView))]
    public class PlayerController : MonoBehaviour
    {
        public StateMachine<PlayerState> stateMachine => m_machine;

        [SerializeField]
        private Camera m_camera; // TODO: Get used of this SerializeField and 
                                 // make camera finding with code

        private StateMachine<PlayerState> m_machine;

        [Button]
        private void TestStopAnimation() {
            CharacterAnimationView animationView = GetComponent<CharacterAnimationView>();
            animationView.StopFromRunning();
        }

        private void Awake()
        {
            PlayerContext data = new PlayerContext(
                    p_body          : GetComponent<Rigidbody>(),
                    p_camera        : m_camera,
                    p_walkControl   : GetComponent<Walkable>(),
                    p_inputReader   : new InputReader(),
                    p_animationView : GetComponent<CharacterAnimationView>()
                    );
            
            
            m_machine = new StateMachine<PlayerState>();
            m_machine.AddState(new PlayableState(m_machine, data));

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
