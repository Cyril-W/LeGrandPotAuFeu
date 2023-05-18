using TMPro;
using UnityEngine;

public class VersionNumber : MonoBehaviour {
    [SerializeField] TextMeshProUGUI versionText;

    void OnValidate() {
        TryFillNull();
    }

    void Start() {
        TryFillNull();
        if (versionText != null) { versionText.text = "V " + Application.version; }
    }

    void TryFillNull () {
        if (versionText == null) { versionText = GetComponent<TextMeshProUGUI>(); }
    }
}
