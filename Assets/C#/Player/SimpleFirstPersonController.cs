using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFirstPersonController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraRoot;
    private CharacterController characterController;

    [Header("Move Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float crouchSpeed = 2.5f;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    [Header("Look Settings")]
    public float mouseSensitivity = 2f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Crouch Settings")]
    public KeyCode crouchKey = KeyCode.C;
    public float standHeight = 2f;
    public float crouchHeight = 1f;
    public float standCameraY = 1.6f;
    public float crouchCameraY = 1.0f;
    public float crouchTransitionSpeed = 10f;

    [Header("Run Settings")]
    public KeyCode runKey = KeyCode.LeftShift;

    [Header("Optional")]
    public bool lockCursorOnStart = true;
    
    [Header("Collision Check")]
    public LayerMask standCheckMask = ~0;


    private float pitch = 0f;
    private float verticalVelocity = 0f;
    private bool isCrouching = false;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Start()
    {
        if (lockCursorOnStart)
        {
            LockCursor();
        }

        if (cameraRoot == null)
        {
            Transform found = transform.Find("CameraRoot");
            if (found != null)
            {
                cameraRoot = found;
            }
        }

        if (cameraRoot == null)
        {
            Debug.LogError("未找到 cameraRoot，请在 Inspector 中拖入 CameraRoot 对象。");
            enabled = false;
            return;
        }

        // 初始化站立状态
        characterController.height = standHeight;
        characterController.center = new Vector3(0f, standHeight / 2f, 0f);

        Vector3 camPos = cameraRoot.localPosition;
        camPos.y = standCameraY;
        cameraRoot.localPosition = camPos;
    }

    void Update()
    {
        HandleCursor();
        Look();
        HandleCrouchToggle();
        UpdateCrouch();
        Move();
    }

    void HandleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }

        if (Input.GetMouseButtonDown(0))
        {
            LockCursor();
        }
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 左右看：旋转玩家本体
        transform.Rotate(Vector3.up * mouseX);

        // 上下看：旋转 CameraRoot
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void HandleCrouchToggle()
    {
        if (Input.GetKeyDown(crouchKey))
        {
            isCrouching = !isCrouching;
        }
    }


    void UpdateCrouch()
    {
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        float targetCameraY = isCrouching ? crouchCameraY : standCameraY;

        // 平滑改变角色高度
        characterController.height = Mathf.Lerp(
            characterController.height,
            targetHeight,
            crouchTransitionSpeed * Time.deltaTime
        );

        // 保持底部尽量贴地：center 要始终是 height / 2
        characterController.center = new Vector3(0f, characterController.height / 2f, 0f);

        // 平滑改变相机高度
        Vector3 camPos = cameraRoot.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, targetCameraY, crouchTransitionSpeed * Time.deltaTime);
        cameraRoot.localPosition = camPos;
    }

    bool CanStandUp()
    {
        float radius = characterController.radius * 0.95f;
        float targetHeight = standHeight;

        Vector3 bottom = transform.position + Vector3.up * radius;
        Vector3 top = transform.position + Vector3.up * (targetHeight - radius);

        return !Physics.CheckCapsule(
            bottom,
            top,
            radius,
            standCheckMask,
            QueryTriggerInteraction.Ignore
        );
    }

    void Move()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = (transform.right * inputX + transform.forward * inputZ).normalized;

        float currentSpeed = walkSpeed;

        bool isTryingToRun = Input.GetKey(runKey);
        bool isMovingForward = inputZ > 0f;

        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (isTryingToRun && isMovingForward)
        {
            currentSpeed = runSpeed;
        }

        Vector3 horizontalVelocity = moveDirection * currentSpeed;

        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            // 下蹲状态下通常不允许跳跃
            if (!isCrouching && Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalMove = horizontalVelocity;
        finalMove.y = verticalVelocity;

        characterController.Move(finalMove * Time.deltaTime);
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
