using UnityEngine;
using UnityEngine.UI;

public class LapManager : MonoBehaviour
{
    public static LapManager Instance { get; private set; }

    public int totalLaps = 3;
    public int totalCheckpoints = 4;

    public Text lapText;
    public Text messageText;

    int _currentLap = 1;
    int _nextCheckpoint = 0;
    bool _raceFinished = false;

    public int CurrentLap => _currentLap;
    public int NextCheckpoint => _nextCheckpoint;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateLapUI();
        if (messageText) messageText.text = "";
    }

    public void RegisterCheckpoint(int index)
    {
        if (_raceFinished) return;
        if (index != _nextCheckpoint) return;

        _nextCheckpoint++;

        if (_nextCheckpoint >= totalCheckpoints)
        {
            _nextCheckpoint = 0;
            _currentLap++;

            if (_currentLap > totalLaps)
            {
                _raceFinished = true;
                if (messageText) messageText.text = "FINISH!";
                Debug.Log("Race Complete!");
                return;
            }
        }

        UpdateLapUI();
    }

    void UpdateLapUI()
    {
        if (lapText)
            lapText.text = $"Lap {Mathf.Min(_currentLap, totalLaps)}/{totalLaps}";
    }
}
