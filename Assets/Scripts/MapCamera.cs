using UnityEngine;

namespace LeGrandPotAuFeu {
    public class MapCamera : MonoBehaviour {
        [Header("Required Data")]
        [SerializeField] Camera mapCamera;
        [SerializeField] Transform target;
        [Header("Field of view Range")]
        [SerializeField] float fOVMinZoom = 5;
        [SerializeField] float fOVMaxZoom = 2;
        [Header("Stick Zoom Range")]
        [SerializeField] float stickMinZoom = -20;
        [SerializeField] float stickMaxZoom = -20;
        [Header("Swivel Zoom Range")]
        [SerializeField] float swivelMinZoom = 90;
        [SerializeField] float swivelMaxZoom = 30;
        [Header("Movement Speed Range")]
        [SerializeField] float moveSpeedMinZoom = 100;
        [SerializeField] float moveSpeedMaxZoom = 200;
        [SerializeField] float distanceDamp = 5;
        [Header("Rotation Speed")]
        [SerializeField] float rotationSpeed = 180;
        //[SerializeField] float rotationDamp = 2;
        [Header("Clamped position")]
        [SerializeField] Vector2 xMaxRange = new Vector2(-10f, 10f);
        [SerializeField] Vector2 zMaxRange = new Vector2(-10f, 10f);

        public bool isFocused { get; set; }

        Transform swivel, stick;
        float zoom = 1f;
        float rotationAngle;

        void Awake() {
            swivel = transform.GetChild(0);
            stick = swivel.GetChild(0);
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                isFocused = !isFocused;
            }

            if (!isFocused) {
                float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
                if (zoomDelta != 0f) {
                    AdjustZoom(zoomDelta);
                }

                float rotationDelta = Input.GetAxis("Rotation");
                if (rotationDelta != 0f) {
                    AdjustRotation(rotationDelta);
                }

                float xDelta = Input.GetAxis("Horizontal");
                float zDelta = Input.GetAxis("Vertical");
                if (xDelta != 0f || zDelta != 0f) {
                    AdjustPosition(xDelta, zDelta);
                }
            }
        }

        private void FixedUpdate() {
            if (isFocused) {
                // dezoom ... maybe in relation to the height
                AdjustZoom(Time.deltaTime);

                if (target) {
                    // position
                    Vector3 targetPos = target.position;
                    transform.position = Vector3.Lerp(transform.position, targetPos, distanceDamp * Time.deltaTime);

                    // rotation
                    /*Quaternion targetRot;
                    if (Input.GetMouseButton(0)) {
                        float rotationDelta = -Input.GetAxis("Mouse X");
                        if (rotationDelta != 0f) {
                            targetRot = Quaternion.Euler(Vector3.up * rotationDelta * rotationSpeed * Time.deltaTime);
                            target.localRotation = target.localRotation * targetRot;
                        }
                    }
                    targetRot = Quaternion.Euler(new Vector3(0, target.rotation.eulerAngles.y, 0f));
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationDamp * Time.deltaTime);*/
                    float rotationDelta = Input.GetAxis("Rotation");
                    if (rotationDelta != 0f) {
                        AdjustRotation(rotationDelta);
                    }
                }
            }
        }

        void AdjustZoom(float delta) {
            zoom = Mathf.Clamp01(zoom + delta);

            float fOW = Mathf.Lerp(fOVMinZoom, fOVMaxZoom, zoom);
            if (mapCamera.orthographic) {
                mapCamera.orthographicSize = fOW;
            } else {
                mapCamera.fieldOfView = fOW;
            }

            float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
            stick.localPosition = new Vector3(0f, 0f, distance);

            float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
            swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }

        void AdjustRotation(float delta) {
            rotationAngle += delta * rotationSpeed * Time.deltaTime;
            if (rotationAngle < 0f) {
                rotationAngle += 360f;
            } else if (rotationAngle >= 360f) {
                rotationAngle -= 360f;
            }
            transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
        }

        void AdjustPosition(float xDelta, float zDelta) {
            Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
            float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
            float distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * damping * Time.deltaTime;

            Vector3 position = transform.localPosition;
            position += direction * distance;
            transform.localPosition = ClampPosition(position);
        }

        Vector3 ClampPosition(Vector3 position) {
            position.x = Mathf.Clamp(position.x, xMaxRange.x, xMaxRange.y);
            position.z = Mathf.Clamp(position.z, zMaxRange.x, zMaxRange.y);
            return position;
        }
    }
}