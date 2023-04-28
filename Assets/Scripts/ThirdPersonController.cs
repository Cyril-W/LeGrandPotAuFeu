using UnityEngine;

public class ThirdPersonController : MonoBehaviour {
    [SerializeField] CharacterController characterController;
    [SerializeField] float speed = 6f;
    [SerializeField] float turnSmoothTime = 0.1f;

    Vector3 direction;
    float turnSmoothVelocity, targetAngle, angle, horizontal, vertical;

    void FixedUpdate() {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f) {
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            characterController.Move(direction * speed * Time.deltaTime);
        }
    }
}
