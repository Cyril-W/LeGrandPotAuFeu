using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DestinyManager : MonoBehaviour {
    public static DestinyManager Instance { get; private set; }

    [SerializeField] Image[] destinyCoins;
    [SerializeField] Color destinyCoinOnColor;
    [SerializeField] Color destinyCoinOffColor;
    [SerializeField] UnityEvent OnDestinyPointOver;

    int destinyPoints;

    void Start() {
        if (Instance == null || Instance != this) { Instance = this; }
        destinyPoints = destinyCoins.Length;
    }

    [ContextMenu("Gain Destiny Point")]
    public void DestinyPointGain() {
        if (destinyPoints == destinyCoins.Length) return;
        destinyPoints++;
        destinyCoins[destinyPoints - 1].color = destinyCoinOnColor;
    }

    [ContextMenu("Lose Destiny Point")]
    public void DestinyPointLose() {
        if (destinyPoints == 0) return;
        destinyCoins[destinyPoints - 1].color = destinyCoinOffColor;
        destinyPoints--;

        if (destinyPoints == 0) { OnDestinyPointOver?.Invoke(); }
    }
}
