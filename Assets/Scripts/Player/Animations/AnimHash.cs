using UnityEngine;

namespace Animations
{
  
  ///<summary>
  ///Hash of animator parameters to make them discoverable for the C# type system.
  ///Use in animator to set different params
  ///<\summary>
  public static class AnimHash 
  {
    public static readonly int Speed = Animator.StringToHash("Speed");
    public static readonly int StopFromRunning = Animator.StringToHash("StopFromRunning");
    public static readonly int TurningRight = Animator.StringToHash("TurningRight");
    public static readonly int TurningLeft = Animator.StringToHash("TurningLeft");
  }

}
