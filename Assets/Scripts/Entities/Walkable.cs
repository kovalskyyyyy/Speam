using UnityEngine;
using UnityEngine.AI;
using Animations;

namespace Entities
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Rigidbody))]
    public class Walkable : MonoBehaviour
    {
        public CharacterAnimationView animationView { get; set; }

        public Vector3 velocity => m_rb.linearVelocity;

        [Header("Constraits")]
        [SerializeField] private float moveForce = 20f; 
        [SerializeField] private float maxSpeed = 4f; 
        [SerializeField] private float arriveDistance = 0.3f; 
        [SerializeField] private float maxDistanceSuggest = 2f; 
        [SerializeField] private float turnSpeed = 360f;
        [SerializeField] private float maxMoveAngle = 15f;
        [SerializeField] private float brakeForce = 20;
        [SerializeField] private float stopSpeed = 0.1f;
        [SerializeField] private float turnSlowAngle = 90f;

        public bool GoToPoint(Vector3 point)
        {
            if (!NavMesh.SamplePosition(point, out NavMeshHit hit, maxDistanceSuggest , NavMesh.AllAreas))
                return false;

            // m_path is mutating here
            if (!m_agent.CalculatePath(hit.position, m_path) || 
                m_path.status != NavMeshPathStatus.PathComplete)
            {
                Debug.Log("[Walkable] Path is unreachable");
                return false;
            }
            
            m_corners = m_path.corners;
            m_index = 1;
            m_braking = false;
            m_moving = true;
            return true;
        }

        public void GoToEntity()
        {
            // TODO: Implement
        }

        private NavMeshAgent m_agent;
        private NavMeshPath m_path;
        private Rigidbody m_rb;

        private bool m_braking = false;
        private bool m_moving = false;
        private int m_index = 0;
        private bool m_pivoting = false;

        private Vector3[] m_corners;

        private bool isLastCorner => m_index == m_corners.Length - 1;

        private void Awake()
        {
            m_agent = GetComponent<NavMeshAgent>();
            m_rb    = GetComponent<Rigidbody>();
            m_path  = new NavMeshPath();

            m_agent.updatePosition = false;
            m_agent.updateRotation = false;
        }


        private void LateUpdate() {
            animationView.SetSpeed(velocity.magnitude / maxSpeed);
        }

        private Vector3 FlatVelocity
        {
            get {
                Vector3 v = m_rb.linearVelocity;
                v.y = 0;
                return v;
            }
        }


        private void FixedUpdate() {
            m_agent.nextPosition = m_rb.position;
            if (!m_moving) {
                return;
            }

            if (m_index >= m_corners.Length) {
                m_moving = false;
                return;
            }

            Vector3 to = m_corners[m_index] - m_rb.position;
            to.y = 0;

            Vector3 flatVel = FlatVelocity;

            if (isLastCorner && TryBrake(to, flatVel)) {
                return;
            }

            if (to.magnitude < arriveDistance) {
                m_index++;
                return;
            }

            RotateTowards(to);
            Steer(to, flatVel);
        }

        private void Steer(Vector3 to, Vector3 flatVel) {
            Vector3 desiredVel = to.normalized * (maxSpeed * GetFacingFactor(to));
            Vector3 dv = desiredVel - flatVel;

            float accel = Vector3.Dot(dv, flatVel) < 0f ? brakeForce : moveForce;

            Vector3 acceleration = Vector3.ClampMagnitude(dv / Time.fixedDeltaTime, accel);
            m_rb.AddForce(acceleration, ForceMode.Acceleration);
        }

        private float GetFacingFactor(Vector3 to) {
            Vector3 forward = m_rb.rotation * Vector3.forward;
            forward.y = 0;

            float angle = Vector3.Angle(forward, to);
            return Mathf.InverseLerp(turnSlowAngle, maxMoveAngle, angle);
        }

        /// <summary> 
        /// Returns true if the character is braking (or has just stopped) and should not accelerate
        /// </summary>
        private bool TryBrake(Vector3 to, Vector3 flatVel) {
            float brakeDistance = flatVel.sqrMagnitude / (2f * brakeForce);

            if (!m_braking && to.magnitude > brakeDistance + arriveDistance)
            {
                return false;
            }

            if (flatVel.magnitude < stopSpeed)
            {
                m_rb.linearVelocity = new Vector3(0f, m_rb.linearVelocity.y, 0f);
                m_braking = false;
                m_moving = false;
                return true;
            }

            if (!m_braking)
            {
                m_braking = true;
                // animationView.StopFromRunning(); // TODO: Add braking animation
            }

            m_rb.AddForce(-flatVel.normalized * brakeForce, ForceMode.Acceleration);
            return true;
        }

        private void RotateTowards(Vector3 to) {
            Quaternion targetRotation = Quaternion.LookRotation(to.normalized, Vector3.up);
            m_rb.MoveRotation(Quaternion.RotateTowards(
                m_rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime)
            );
        }

        private bool IsFacing(Vector3 to) {
            Vector3 forward = m_rb.rotation * Vector3.forward;
            forward.y = 0;

            return Vector3.Angle(forward, to) <= maxMoveAngle;
        }
    }
}
