using Entities;
using Player;
using UnityEngine;

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
        public InputReader inputReader { get; set; }
    }
}
