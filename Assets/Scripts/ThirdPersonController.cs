using UnityEngine;
using UnityEngine.Events;

public class ThirdPersonController : MonoBehaviour {
    [SerializeField] CharacterController characterController;
    [SerializeField] float speed = 6f;
    [SerializeField, Range(0f, 1f)] float crouchSpeedRatio = 0.5f;
    [SerializeField] float turnSmoothTime = 0.1f;
    [SerializeField] float gravityMultiplier = 1f;
    [SerializeField] float timeBetweenSteps = 0.4f;
    [SerializeField] UnityEvent onFootStep;

    Vector3 direction;
    float turnSmoothVelocity, targetAngle, angle, horizontal, vertical, velocity, gravity = -9.81f, currentTimeBetweenSteps = 0f;
    bool isCrouched = false;

    void FixedUpdate() {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f) {
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            if (currentTimeBetweenSteps > 0) {
                currentTimeBetweenSteps -= Time.deltaTime;
            }
            if (currentTimeBetweenSteps <= 0) {
                currentTimeBetweenSteps = timeBetweenSteps / (isCrouched ? crouchSpeedRatio : 1f);
                onFootStep?.Invoke();
                if (GroupManager.Instance != null) {
                    for (int i = 0; i < GroupManager.Instance.GetNumberHeroSaved(); i++) {
                        onFootStep?.Invoke();
                    }
                }
            }
        }

        if (characterController.isGrounded && velocity < 0f) {
            velocity = 0f;//-1f;
        } else {
            velocity += gravity * gravityMultiplier * Time.deltaTime;
        }
        direction.y = velocity;

        characterController.Move(direction * speed * (isCrouched ? crouchSpeedRatio : 1f) * Time.deltaTime);;
    }

    public void SetIsCrouched(bool newIsCrouched) {
        isCrouched = newIsCrouched;
    }
}
