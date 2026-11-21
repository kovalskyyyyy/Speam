using Player.States;
using DialogueSystem.Runtime;
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

        //TODO: Убрать SerializeField и находить камеру кодом GAME-XXX
        [SerializeField]
        private Camera m_camera;

        private StateMachine<PlayerState> m_machine;
        private DialogueRunner m_runner;

        // BEGIN DEBUG BUTTONS {
        [Button]
        private void TestStopAnimation() {
            CharacterAnimationView animationView = GetComponent<CharacterAnimationView>();
            animationView.StopFromRunning();
        }

        [Button]
        private void TestTurnLeftAnimation() {
            CharacterAnimationView animationView = GetComponent<CharacterAnimationView>();
            animationView.TurnLeft();
        }

        [Button]
        private void TestTurnRightAnimation() {
            CharacterAnimationView animationView = GetComponent<CharacterAnimationView>();
            animationView.TurnRight();
        }
        // END DEBUG BUTTONS }

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

        // Именно Start, а не Awake: к Start все Awake уже отработали, и Instance точно выставлен
        private void Start()
        {
            m_runner = DialogueRunner.Instance;
            if (m_runner == null)
            {
                Debug.LogWarning("DialogueRunner не найден на сцене.", this);
                return;
            }

            m_runner.Started += OnStartedHandler;
            m_runner.Ended += OnEndedHandler;
        }

        private void OnDestroy()
        {
            if (m_runner == null)
            {
                return;
            }

            m_runner.Started -= OnStartedHandler;
            m_runner.Ended -= OnEndedHandler;
        }

        private void Update()
        {
            m_machine.StateUpdate();
        }

        private void FixedUpdate()
        {
            m_machine.FixedStateUpdate();
        }

        private void OnStartedHandler() => m_machine.ChangeState<DialogueState>();

        private void OnEndedHandler() => m_machine.ChangeState<PlayableState>();
    }
}
