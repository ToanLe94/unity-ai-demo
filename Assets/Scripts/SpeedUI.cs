using UnityEngine;
using UnityEngine.UI;

public class SpeedUI : MonoBehaviour
{
    public KartController kart;
    public Text speedText;

    void Update()
    {
        if (kart == null || speedText == null) return;
        float kmh = Mathf.Abs(kart.CurrentSpeedKmh);
        speedText.text = $"{kmh:F0} km/h";
    }
}
