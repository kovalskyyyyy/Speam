using Entities;
using Player;
using UnityEngine;
using Animations;

namespace Player
{
    public class PlayerContext
    {
        //public WalkableScript Walker;
        //public InputManager Input;
        //public PlayerAnimationView Anim;

        public PlayerContext(
                Rigidbody p_body,
                Camera p_camera,
                Walkable p_walkControl,
                InputReader p_inputReader,
                CharacterAnimationView p_animationView) 
        {
            body = p_body;
            camera = p_camera;
            walkControl = p_walkControl;
            inputReader = p_inputReader;
            animationView = p_animationView;
        }

        public Rigidbody              body          { get; }
        public Camera                 camera        { get; set; }
        public Walkable               walkControl   { get; }
        public InputReader            inputReader   { get; }
        public CharacterAnimationView animationView { get; }
    }
}
