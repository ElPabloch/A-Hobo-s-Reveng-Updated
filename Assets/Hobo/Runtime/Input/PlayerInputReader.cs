using UnityEngine;
using UnityEngine.InputSystem;

namespace Hobo
{
    /// <summary>
    /// Converts keyboard, mouse and gamepad values into commands.
    /// Reading input never changes campaign state, menus or window settings.
    /// </summary>
    public sealed class PlayerInputReader
    {
        const float MovementDeadzone = 0.15f;
        const float AimDeadzone = 0.20f;
        bool useGamepadAim;

        public InputFrame Read(Camera camera, Vector2 playerPosition, Vector2 previousAim)
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            var gamepad = Gamepad.current;
            var movement = ReadMovement(keyboard, gamepad);
            var aim = ReadAim(camera, playerPosition, previousAim, mouse, gamepad);

            return new InputFrame
            {
                Movement = movement,
                Aim = aim,
                Block = keyboard?.spaceKey.isPressed == true || gamepad?.leftShoulder.isPressed == true,
                Attack = mouse?.leftButton.isPressed == true || gamepad?.rightShoulder.isPressed == true,
                Throw = mouse?.rightButton.wasPressedThisFrame == true || gamepad?.buttonWest.wasPressedThisFrame == true,
                Interact = keyboard?.eKey.wasPressedThisFrame == true || gamepad?.buttonSouth.wasPressedThisFrame == true,
                TogglePause = keyboard?.escapeKey.wasPressedThisFrame == true || gamepad?.startButton.wasPressedThisFrame == true,
                ToggleFullscreen = keyboard?.f11Key.wasPressedThisFrame == true
            };
        }

        static Vector2 ReadMovement(Keyboard keyboard, Gamepad gamepad)
        {
            var movement = Vector2.zero;
            if (keyboard != null)
            {
                float horizontal = (keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0);
                float vertical = (keyboard.sKey.isPressed ? 1 : 0) - (keyboard.wKey.isPressed ? 1 : 0);
                movement = new Vector2(horizontal, vertical);
            }

            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                if (stick.magnitude > MovementDeadzone)
                    movement = new Vector2(stick.x, -stick.y);
            }

            return movement;
        }

        Vector2 ReadAim(Camera camera, Vector2 playerPosition, Vector2 previousAim, Mouse mouse, Gamepad gamepad)
        {
            Vector2 aim = previousAim;
            if (gamepad != null)
            {
                Vector2 stick = gamepad.rightStick.ReadValue();
                if (stick.magnitude > AimDeadzone)
                {
                    aim = new Vector2(stick.x, -stick.y);
                    useGamepadAim = true;
                }
            }

            if (mouse == null)
                return aim;

            if (mouse.delta.ReadValue().sqrMagnitude > 1f)
                useGamepadAim = false;

            if (!useGamepadAim)
            {
                Vector3 worldPosition = camera.ScreenToWorldPoint(mouse.position.ReadValue());
                aim = GameCoordinates.ToSimulation(worldPosition) - playerPosition;
            }

            return aim;
        }
    }
}
