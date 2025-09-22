using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class control : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float airControl = 0.2f; // cuánto control en aire (0..1)

    [Header("Salto y gravedad")]
    [SerializeField] private float jumpHeight = 1.6f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float terminalVelocity = -50f;
    [SerializeField] private float groundedGraceTime = 0.15f; // pequeño leniency para saltos

    [Header("Opciones")]
    [SerializeField] private bool enableSprint = true;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

    private CharacterController cc;
    private Vector3 velocity; // y incluido
    private float currentSpeed;
    private float lastGroundedTime = -10f;
    private float lastJumpPressedTime = -10f;
    private const float jumpInputGraceTime = 0.15f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        HandleInputs();
        HandleMovement();
        ApplyGravityAndJump();
        MoveCharacter();
    }

    void HandleInputs()
    {
        // detectar último frame que el jugador estuvo en el suelo
        if (cc.isGrounded)
            lastGroundedTime = Time.time;

        // detectar salto (grace window)
        if (Input.GetButtonDown("Jump"))
            lastJumpPressedTime = Time.time;
    }

    void HandleMovement()
    {
        // entrada
        float inputX = Input.GetAxisRaw("Horizontal"); // -1..1
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(inputX, 0f, inputZ);
        input = Vector3.ClampMagnitude(input, 1f);

        // movimiento relativo a la cámara (si hay cam)
        Vector3 moveDirection;
        if (cameraTransform != null)
        {
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            moveDirection = (forward * input.z + right * input.x).normalized;
        }
        else
        {
            // si no hay cámara, mover relativo al world
            moveDirection = (transform.forward * input.z + transform.right * input.x).normalized;
        }

        // sprint
        bool sprinting = enableSprint && Input.GetKey(sprintKey) && input.magnitude > 0.1f;
        float targetSpeed = sprinting ? sprintSpeed : walkSpeed;

        // suavizar velocidad con aceleración
        float control = cc.isGrounded ? 1f : airControl;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * control * Time.deltaTime);

        // setear la componente horizontal de velocity hacia la dirección deseada
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 desired = moveDirection * currentSpeed;

        // aplico lerp/MoveTowards para evitar cambios instantáneos
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, desired, acceleration * control * Time.deltaTime);

        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;
    }

    void ApplyGravityAndJump()
    {
        // gravedad
        if (cc.isGrounded && velocity.y < 0f)
            velocity.y = -2f; // pequeño empuje hacia abajo para mantener contacto

        // salto: permitimos un pequeño "grace" si se presionó poco antes de aterrizar
        bool canJump = (Time.time - lastGroundedTime) <= groundedGraceTime;
        bool pressedJumpRecently = (Time.time - lastJumpPressedTime) <= jumpInputGraceTime;

        if (pressedJumpRecently && (canJump || cc.isGrounded))
        {
            // v = sqrt(2 * g * h) -> con g positivo
            float g = Mathf.Abs(gravity);
            velocity.y = Mathf.Sqrt(2f * g * jumpHeight);
            lastJumpPressedTime = -10f; // consumir la entrada
        }

        // aplicar gravedad cada frame (nota: gravedad es negativa)
        velocity.y += gravity * Time.deltaTime;
        if (velocity.y < terminalVelocity) velocity.y = terminalVelocity;
    }

    void MoveCharacter()
    {
        Vector3 displacement = velocity * Time.deltaTime;
        cc.Move(displacement);
    }
}
