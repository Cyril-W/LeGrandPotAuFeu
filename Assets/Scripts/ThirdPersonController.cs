using UnityEngine;

public class ThirdPersonController : MonoBehaviour {
    [SerializeField] CharacterController characterController;
    [SerializeField] float speed = 6f;
    [SerializeField] float turnSmoothTime = 0.1f;
    [SerializeField] float gravityMultiplier = 1f;

    Vector3 direction;
    float turnSmoothVelocity, targetAngle, angle, horizontal, vertical, velocity, gravity = -9.81f;
    
    void FixedUpdate() {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (characterController.isGrounded && velocity < 0f) {
            velocity = -1f;
        } else {
            velocity += gravity * gravityMultiplier * Time.deltaTime;
        }
        direction.y = velocity;

        if (direction.magnitude >= 0.1f) {
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            characterController.Move(direction * speed * Time.deltaTime);
        }
    }
}
