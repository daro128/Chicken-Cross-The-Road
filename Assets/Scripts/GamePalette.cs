using UnityEngine;

namespace ChickenRush
{
    /// <summary>
    /// Holds every material used by procedurally generated objects. Because the scene references
    /// these materials, their shader is always included in a standalone build (Shader.Find would not be).
    /// The Editor scene builder creates and assigns all of these automatically.
    /// </summary>
    public class GamePalette : MonoBehaviour
    {
        [Header("Environment")]
        public Material grassLight;
        public Material grassDark;
        public Material road;
        public Material marking;
        public Material trunk;
        public Material leaves;
        public Material leavesDark;
        public Material flowerPink;
        public Material flowerYellow;
        public Material finishTile;
        public Material finishPole;

        [Header("Chicken")]
        public Material chickenWhite;
        public Material chickenWing;
        public Material beak;
        public Material comb;
        public Material eye;

        [Header("Cars")]
        public Material[] carBodies;
        public Material carWindow;
        public Material carWheel;
        public Material carLight;
    }
}
