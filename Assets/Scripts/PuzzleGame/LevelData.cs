using System;
using UnityEngine;

namespace PuzzleGame
{
    [Serializable]
    public class ButtonDef
    {
        public Color color = Color.white;
        [Tooltip("Flat tile indices (row * cols + col) that this button affects")]
        public int[] tileIndices;
    }

    [CreateAssetMenu(fileName = "PuzzleLevel", menuName = "PuzzleGame/Level Data")]
    public class LevelData : ScriptableObject
    {
        public int rows = 3;
        public int cols = 5;
        public Color[] targetColors;   // length = rows * cols
        public ButtonDef[] buttons;
        [Tooltip("0 = unlimited moves")]
        public int maxMoves = 0;
    }
}
