using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame
{
    /// <summary>A circular color button that applies an operation when clicked.</summary>
    [RequireComponent(typeof(Button))]
    public class ColorButtonUI : MonoBehaviour
    {
        [SerializeField] Image colorImage;
        int _buttonIndex;

        public void Setup(int index, Color color)
        {
            _buttonIndex = index;
            if (colorImage != null)
                colorImage.color = color;
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        void OnClick()
        {
            PuzzleGameManager.Instance.ApplyButton(_buttonIndex);
        }
    }
}
