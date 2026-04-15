using UnityEngine;

namespace PuzzleGame
{
    public class PuzzleGameManager : MonoBehaviour
    {
        public static PuzzleGameManager Instance { get; private set; }

        [Header("Level Assets")]
        public LevelData[] levels;

        [Header("Scene References")]
        public PuzzleGrid targetGrid;
        public PuzzleGrid playerGrid;
        public PuzzleUI ui;

        public int CurrentLevel  { get; private set; }
        public int MoveCount     { get; private set; }

        LevelData   _data;
        Color[]     _playerState;
        bool        _levelComplete;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            LoadLevel(0);
        }

        public void LoadLevel(int index)
        {
            CurrentLevel   = Mathf.Clamp(index, 0, levels.Length - 1);
            _data          = levels[CurrentLevel];
            MoveCount      = 0;
            _levelComplete = false;

            int size = _data.rows * _data.cols;
            _playerState = new Color[size];
            for (int i = 0; i < size; i++)
                _playerState[i] = new Color(0.35f, 0.35f, 0.35f);

            targetGrid.BuildGrid(_data.rows, _data.cols, _data.targetColors);
            playerGrid.BuildGrid(_data.rows, _data.cols, _playerState);
            ui.OnLevelLoaded(CurrentLevel + 1, _data.maxMoves, _data.buttons);
        }

        public void ApplyButton(int buttonIndex)
        {
            if (_data == null || _levelComplete) return;
            if (buttonIndex < 0 || buttonIndex >= _data.buttons.Length) return;

            var btn = _data.buttons[buttonIndex];
            foreach (int idx in btn.tileIndices)
                if (idx >= 0 && idx < _playerState.Length)
                    _playerState[idx] = btn.color;

            MoveCount++;
            playerGrid.UpdateColors(_playerState);
            ui.UpdateMoves(MoveCount, _data.maxMoves);

            if (CheckWin())
            {
                _levelComplete = true;
                ui.ShowWin(CurrentLevel + 1 < levels.Length);
            }
        }

        bool CheckWin()
        {
            var target = _data.targetColors;
            if (target == null || target.Length != _playerState.Length) return false;
            for (int i = 0; i < _playerState.Length; i++)
                if (!ColorClose(_playerState[i], target[i])) return false;
            return true;
        }

        static bool ColorClose(Color a, Color b) =>
            Mathf.Abs(a.r - b.r) < 0.02f &&
            Mathf.Abs(a.g - b.g) < 0.02f &&
            Mathf.Abs(a.b - b.b) < 0.02f;

        public void RestartLevel()  => LoadLevel(CurrentLevel);
        public void NextLevel()     => LoadLevel(CurrentLevel + 1);
        public void PreviousLevel() => LoadLevel(CurrentLevel - 1);
    }
}
