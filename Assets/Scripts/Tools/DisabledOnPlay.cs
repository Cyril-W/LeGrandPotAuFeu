using UnityEngine;

public class DisabledOnPlay : MonoBehaviour {
    public void Disable() {
        gameObject.SetActive(false);
    }
}
