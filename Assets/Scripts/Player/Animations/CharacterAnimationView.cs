using UnityEngine;


namespace Animations {
  public class CharacterAnimationView : MonoBehaviour
  {
      [SerializeField] private float speedDamp = 0.1f;

      public void SetSpeed(float v) => m_anim.SetFloat(AnimHash.Speed, v, speedDamp, Time.deltaTime);
      public void StopFromRunning() => m_anim.SetTrigger(AnimHash.StopFromRunning);
      public void TurnRight()       => m_anim.SetTrigger(AnimHash.TurningRight);
      public void TurnLeft()        => m_anim.SetTrigger(AnimHash.TurningLeft);
      
      private Animator m_anim;

      private void Awake() {
        m_anim = GetComponentInChildren<Animator>();
      } 

  }
}
