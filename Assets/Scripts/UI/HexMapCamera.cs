using LeGrandPotAuFeu.Grid;
using LeGrandPotAuFeu.Utility;
using UnityEngine;

namespace LeGrandPotAuFeu.UI {
	public class HexMapCamera : MonoBehaviour {
		[Header("Stick Zoom Range")]
		[SerializeField] float stickMinZoom = -250;
		[SerializeField] float stickMaxZoom = -45;
		[Header("Swivel Zoom Range")]
		[SerializeField] float swivelMinZoom = 90;
		[SerializeField] float swivelMaxZoom = 45;
		[Header("Movement Speed Range")]
		[SerializeField] float moveSpeedMinZoom = 400;
		[SerializeField] float moveSpeedMaxZoom = 100;
		[SerializeField] float distanceDamp = 5;
		[Header("Rotation Speed")]
		[SerializeField] float rotationSpeed = 180;
		[SerializeField] float rotationDamp = 2;
		[Header("Other")]
		[SerializeField] HexGrid grid;

		public static bool Locked {
			set {
				instance.enabled = !value;
			}
		}
		public static bool isFocused { get; set; }

		static HexMapCamera instance;

		Transform swivel, stick, player;
		float zoom = 1f;
		float rotationAngle;

		void Awake() {
			instance = this;
			swivel = transform.GetChild(0);
			stick = swivel.GetChild(0);
		}

		void Update() {
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

				if (!player) {
					player = grid.Player.transform;
				}

				if (player) {
					// position
					Vector3 targetPos = player.position;
					transform.position = Vector3.Lerp(transform.position, targetPos, distanceDamp * Time.deltaTime);

					// rotation
					Quaternion targetRot;
					if (Input.GetMouseButton(0)) {
						float rotationDelta = -Input.GetAxis("Mouse X");
						if (rotationDelta != 0f) {
							targetRot = Quaternion.Euler(Vector3.up * rotationDelta * rotationSpeed * Time.deltaTime);
							player.localRotation = player.localRotation * targetRot;
						}
					} 	
					targetRot = Quaternion.Euler(new Vector3(0, player.rotation.eulerAngles.y, 0f));
					transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationDamp * Time.deltaTime);
				}
			}
		}

		public static void ValidatePosition() {
			instance.AdjustPosition(0f, 0f);
		}

		void AdjustZoom(float delta) {
			zoom = Mathf.Clamp01(zoom + delta);

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
			float xMax = grid.cellCountX * 2f * HexMetrics.innerRadius; // (grid.cellCountX * HexMetrics.chunkSizeX - 0.5f) * (2f * HexMetrics.innerRadius);
			position.x = Mathf.Clamp(position.x, 0f, xMax);

			float zMax = grid.cellCountZ * 1.75f * HexMetrics.innerRadius; // (grid.cellCountZ * HexMetrics.chunkSizeZ - 1) * (1.5f * HexMetrics.outerRadius);
			position.z = Mathf.Clamp(position.z, 0f, zMax);

			return position;
		}
	}
}