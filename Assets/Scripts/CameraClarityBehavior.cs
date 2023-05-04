using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class CameraClarityBehavior : MonoBehaviour {
    [System.Serializable]
    class TransparentObject {
        [HideInInspector] public string ObjectName;
        public MeshRenderer ObjectToHide;
        public Material MaterialToHide;
        public Material MaterialTransparent;
        public bool isMainMaterial = false;
    }

    [SerializeField] CinemachineVirtualCamera virtualCamera;
    [SerializeField, ReadOnly] float distance = 0f;
    [SerializeField] Vector2 minMaxDistanceToHide = new Vector2(100f, 150f);
    [SerializeField] TransparentObject[] transparentObjects;
    [SerializeField] Vector2 minMaxMaterialTransparency = new Vector2(0f, 0.5f);
    [SerializeField] float transparenceDuration = 1f;
    [SerializeField] bool debugVerbose = false;

    bool currentHide = false;
    float currentTransparence = 0f;

    static readonly string _transparency = "_Transparency";

    void OnValidate() {
        TryFillNull();
        foreach (var transparentObject in transparentObjects) {
            if (transparentObject.ObjectToHide != null) {
                transparentObject.ObjectName = transparentObject.ObjectToHide.name;
            }
        }
    }

    void TryFillNull() {
        if (virtualCamera == null) virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
    }

    void OnDestroy() {
        if (currentHide || (!currentHide && currentTransparence > 0f)) {
            foreach (var transparentObject in transparentObjects) {
                if (debugVerbose) {
                    Debug.LogWarning("Destroying " + transparentObject.ObjectToHide.transform.parent.name + " > " + transparentObject.ObjectToHide.transform.name /*+ " > " + transparentObject.ObjectToHide.material.name*/);
                } else {
                    Debug.LogWarning("Destroying instanciated transparent material");
                }
                Destroy(transparentObject.ObjectToHide.material);
            }
        }
    }

    void Start() {
        TryFillNull();
        var materialNames = new List<string>();
        foreach (var transparentObject in transparentObjects) {
            if (/*!(*/!materialNames.Contains(transparentObject.MaterialToHide.name) /*|| materialNames.Contains(transparentObject.MaterialTransparent.name))*/) {
                materialNames.Add(transparentObject.MaterialToHide.name);
                //materialNames.Add(transparentObject.MaterialTransparent.name);
                transparentObject.isMainMaterial = true;
            }
        }
    }

    void FixedUpdate() {
        distance = Vector3.Distance(virtualCamera.transform.position, transform.position);
        bool hide;
        if (!currentHide) {
            hide = distance <= minMaxDistanceToHide.x;
        } else {
            hide = distance <= minMaxDistanceToHide.y;
        }
        if (currentHide != hide) {
            currentHide = hide;
            currentTransparence = transparenceDuration;
            foreach (var transparentObject in transparentObjects) {
                transparentObject.ObjectToHide.shadowCastingMode = currentHide ? UnityEngine.Rendering.ShadowCastingMode.Off : UnityEngine.Rendering.ShadowCastingMode.On; 
                if (currentHide) {
                    transparentObject.ObjectToHide.material = transparentObject.MaterialTransparent;
                }
            }
        }
        if (currentTransparence > 0f) {
            currentTransparence -= Time.deltaTime;
            HideObject();
            if (!currentHide && currentTransparence <= 0f) {
                foreach (var transparentObject in transparentObjects) {
                    Destroy(transparentObject.ObjectToHide.material, 0.5f);
                    transparentObject.ObjectToHide.material = transparentObject.MaterialToHide;
                }
            }
        }
    }

    void HideObject() {
        var initialValue = currentHide ? minMaxMaterialTransparency.x : minMaxMaterialTransparency.y;
        var targetValue = !currentHide ? minMaxMaterialTransparency.x : minMaxMaterialTransparency.y;
        Material currentMainMaterial = null; 
        foreach (var transparentObject in transparentObjects) {
            if (transparentObject.isMainMaterial) {
                var t = Mathf.Lerp(initialValue, targetValue, 1f - Mathf.Clamp01(currentTransparence / transparenceDuration));
                transparentObject.ObjectToHide.material.SetFloat(_transparency, t);
                currentMainMaterial = transparentObject.ObjectToHide.material;
            } else {
                transparentObject.ObjectToHide.material = currentMainMaterial;
            }
        }
    }
}
