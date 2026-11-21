using Player.States;
using DialogueSystem.Runtime;
using StateMachine;
using Animations;
using CustomDebug;

using UnityEngine;
using Entities;

namespace Player
{
    [RequireComponent(typeof(Walkable))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CharacterAnimationView))]
    public class PlayerController : MonoBehaviour
    {
        public StateMachine<PlayerState> stateMachine => m_machine;

        [SerializeField]
        private Camera m_camera;

        [Tooltip("Ассет Input Reader. Если не задан, создаётся временный экземпляр в рантайме.")]
        [SerializeField]
        private InputReader m_inputReader;

        private StateMachine<PlayerState> m_machine;
        private DialogueRunner m_runner;
        private bool m_ownsInputReader;

        [Button]
        private void TestStopAnimation() {
            CharacterAnimationView animationView = GetComponent<CharacterAnimationView>();
            animationView.StopFromRunning();
        }

        private void Awake()
        {
            if (m_camera == null) m_camera = Camera.main;

            if (m_inputReader == null)
            {
                // ScriptableObject нельзя создавать через new.
                m_inputReader = ScriptableObject.CreateInstance<InputReader>();
                m_ownsInputReader = true;
            }

            PlayerContext data = new PlayerContext(
                    p_body          : GetComponent<Rigidbody>(),
                    p_camera        : m_camera,
                    p_walkControl   : GetComponent<Walkable>(),
                    p_inputReader   : m_inputReader,
                    p_animationView : GetComponent<CharacterAnimationView>()
                    );

            m_machine = new StateMachine<PlayerState>();
            m_machine.AddState(new PlayableState(m_machine, data));
            m_machine.AddState(new DialogueState(m_machine, data));

            m_machine.ChangeState<PlayableState>();
        }

        // Именно Start, а не Awake: к Start все Awake уже отработали, и Instance точно выставлен
        private void Start()
        {
            m_runner = DialogueRunner.Instance;
            if (m_runner == null) return;

            m_runner.Started += OnStartedHandler;
            m_runner.Ended += OnEndedHandler;
        }

        private void OnDestroy()
        {
            if (m_runner != null)
            {
                m_runner.Started -= OnStartedHandler;
                m_runner.Ended -= OnEndedHandler;
            }

            if (m_ownsInputReader && m_inputReader != null)
                Destroy(m_inputReader);
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
