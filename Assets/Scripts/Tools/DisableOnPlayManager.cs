using UnityEngine;

public class DisableOnPlayManager : MonoBehaviour {
    [SerializeField] DisabledOnPlay[] disableOnPlays;
    void OnValidate() {
        disableOnPlays = FindObjectsOfType<DisabledOnPlay>(true);
    }

    void Start() {
        for (int i = disableOnPlays.Length - 1; i >= 0; i--) {
            var disableOnPlay = disableOnPlays[i];
            disableOnPlay.Disable();
            Destroy(disableOnPlay);
        }
        disableOnPlays = null;
    }
}
