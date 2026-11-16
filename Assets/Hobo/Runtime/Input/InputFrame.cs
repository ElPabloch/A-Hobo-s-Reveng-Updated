using UnityEngine;

namespace Hobo
{
    /// <summary>Commands captured during one frame. Contains no game state or device objects.</summary>
    public struct InputFrame
    {
        public Vector2 Movement;
        public Vector2 Aim;
        public bool Block;
        public bool Attack;
        public bool Throw;
        public bool Interact;
        public bool TogglePause;
        public bool ToggleFullscreen;
    }
}
