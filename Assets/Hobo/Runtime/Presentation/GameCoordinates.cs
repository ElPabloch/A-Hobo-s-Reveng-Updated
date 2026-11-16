using UnityEngine;

namespace Hobo
{
    /// <summary>
    /// The imported simulation uses a 960x640 canvas with Y pointing down.
    /// Unity uses world units with Y pointing up. Keep this conversion in one place.
    /// </summary>
    public static class GameCoordinates
    {
        public const float SimulationPixelsPerWorldUnit = 32f;
        public const float SimulationCanvasHeight = 640f;

        public static Vector3 ToWorld(Vector2 position)
        {
            return new Vector3(
                position.x / SimulationPixelsPerWorldUnit,
                (SimulationCanvasHeight - position.y) / SimulationPixelsPerWorldUnit,
                0f);
        }

        public static Vector2 ToSimulation(Vector3 position)
        {
            return new Vector2(
                position.x * SimulationPixelsPerWorldUnit,
                SimulationCanvasHeight - position.y * SimulationPixelsPerWorldUnit);
        }
    }
}
