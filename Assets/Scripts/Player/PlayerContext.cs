using Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerContext
    {
        //public WalkableScript Walker;
        //public InputManager Input;
        //public PlayerAnimationView Anim;
        public Rigidbody2D body { get; }
        public Camera camera { get; set; }
        public Walkable walkControl { get; set; }
        public InputActionReference pointClickAction { get; set; }
    }
}