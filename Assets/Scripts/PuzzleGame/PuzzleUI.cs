using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PuzzleGame
{
    public class PuzzleUI : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] TMP_Text levelText;
        [SerializeField] TMP_Text movesText;
        [SerializeField] Button backButton;
        [SerializeField] Button restartButton;

        [Header("Buttons Panel")]
        [SerializeField] Transform buttonsContainer;
        [SerializeField] ColorButtonUI colorButtonPrefab;

        [Header("Win Panel")]
        [SerializeField] GameObject winPanel;
        [SerializeField] Button nextLevelButton;
        [SerializeField] Button retryButton;

        void Awake()
        {
            // Wire buttons here — AddListener in editor scripts creates runtime-only
            // listeners that are not serialized, so we hook them up at runtime instead.
            backButton?.onClick.AddListener(OnBackClicked);
            restartButton?.onClick.AddListener(OnRestartClicked);
            nextLevelButton?.onClick.AddListener(OnNextClicked);
            retryButton?.onClick.AddListener(OnRestartClicked);
        }

        public void OnLevelLoaded(int levelNumber, int maxMoves, ButtonDef[] buttons)
        {
            levelText.text = $"LEVEL {levelNumber}";
            UpdateMoves(0, maxMoves);
            winPanel.SetActive(false);

            BuildColorButtons(buttons);
        }

        void BuildColorButtons(ButtonDef[] buttons)
        {
            foreach (Transform child in buttonsContainer)
                Destroy(child.gameObject);

            for (int i = 0; i < buttons.Length; i++)
            {
                var btn = Instantiate(colorButtonPrefab, buttonsContainer);
                btn.Setup(i, buttons[i].color);
            }
        }

        public void UpdateMoves(int count, int max)
        {
            if (movesText == null) return;
            movesText.text = max > 0 ? $"Moves: {count} / {max}" : $"Moves: {count}";
        }

        public void ShowWin(bool hasNext)
        {
            winPanel.SetActive(true);
            nextLevelButton.interactable = hasNext;
        }

        public void OnBackClicked()    => PuzzleGameManager.Instance.PreviousLevel();
        public void OnRestartClicked() => PuzzleGameManager.Instance.RestartLevel();
        public void OnNextClicked()    => PuzzleGameManager.Instance.NextLevel();
    }
}
