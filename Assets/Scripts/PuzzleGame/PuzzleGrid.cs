using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame
{
    /// <summary>Renders a grid of colored tiles inside a GridLayoutGroup.</summary>
    public class PuzzleGrid : MonoBehaviour
    {
        [SerializeField] GridLayoutGroup layoutGroup;
        [SerializeField] Image tilePrefab;

        const float CellSize = 50f;
        const float Spacing = 4f;

        Image[] _tiles;

        public void BuildGrid(int rows, int cols, Color[] colors)
        {
            foreach (Transform child in layoutGroup.transform)
                Destroy(child.gameObject);

            layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layoutGroup.constraintCount = cols;
            layoutGroup.cellSize = new Vector2(CellSize, CellSize);
            layoutGroup.spacing = new Vector2(Spacing, Spacing);
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);

            var rt = layoutGroup.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(
                cols * CellSize + (cols - 1) * Spacing,
                rows * CellSize + (rows - 1) * Spacing);

            _tiles = new Image[rows * cols];
            for (int i = 0; i < rows * cols; i++)
            {
                var tile = Instantiate(tilePrefab, layoutGroup.transform);
                tile.color = colors != null && i < colors.Length ? colors[i] : Color.gray;
                _tiles[i] = tile;
            }
        }

        public void UpdateColors(Color[] colors)
        {
            if (_tiles == null) return;
            for (int i = 0; i < _tiles.Length && i < colors.Length; i++)
                _tiles[i].color = colors[i];
        }
    }
}
