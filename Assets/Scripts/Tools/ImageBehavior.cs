using UnityEngine;
using UnityEngine.UI;

public class ImageBehavior : MonoBehaviour {
    [SerializeField] Image image;
    [SerializeField] Color startColor;
    [SerializeField] Color endColor;
    [SerializeField] bool applyColorOnEnable = false;

    void OnValidate() {
        TryFillNull();
    }

    void Start() {
        TryFillNull();
    }

    void OnEnable() {
        TryFillNull();
        if (applyColorOnEnable && image != null) { image.color = startColor; }
    }

    void TryFillNull() {
        if (image == null) { image = GetComponent<Image>(); }
    }

    public void LerpColor(float lerp) {
        if (image == null) { return; }
        image.color = Color.Lerp(startColor, endColor, Mathf.Clamp01(lerp));
    }
}
