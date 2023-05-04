using TMPro;
using UnityEngine;

public class VersionNumber : MonoBehaviour {
    [SerializeField] TextMeshProUGUI versionText;

    void OnValidate() {
        if (versionText == null) { versionText = GetComponent<TextMeshProUGUI>(); }
    }

    void Start() {
        if (versionText == null) { versionText = GetComponent<TextMeshProUGUI>(); }
        if (versionText != null) { versionText.text = Application.version; }
    }
}
