using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [SerializeField] Camera cameraToFace;

    private void OnValidate() {
        if (cameraToFace == null) cameraToFace = Camera.main;
    }

    void Start() {
        if (cameraToFace == null) cameraToFace = Camera.main;
    }

    void LateUpdate() {
        transform.LookAt(transform.position + cameraToFace.transform.rotation * Vector3.forward, cameraToFace.transform.rotation * Vector3.up);
    }
}
