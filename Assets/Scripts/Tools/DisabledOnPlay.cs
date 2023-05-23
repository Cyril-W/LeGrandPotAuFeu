using UnityEngine;

public class DisabledOnPlay : MonoBehaviour {
    void Awake() {
        gameObject.SetActive(false);
    }
}
