using UnityEngine;


namespace Animations {
  public class CharacterAnimationView : MonoBehaviour
  {
      [SerializeField] private float speedDamp = 0.1f;
      private Animator m_anim;

      private void Awake() => m_anim = GetComponentInChildren<Animator>();

      public void SetSpeed(float v)      => m_anim.SetFloat(AnimHash.Speed, v, speedDamp, Time.deltaTime);
  }
}
