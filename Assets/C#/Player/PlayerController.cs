using UnityEngine;
using System; // 添加对UIManager的引用

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f; // 移动速度
    public float runSpeedMultiplier = 1.5f; // 奔跑速度倍数
    public float acceleration = 10f; // 加速度
    public float deceleration = 15f; // 减速度（停止时的减速更快）
    public float airAcceleration = 5f; // 空中加速度（如果角色离地）
    
    [Header("输入平滑")]
    public float inputSmoothTime = 0.1f; // 输入平滑时间

    [Header("旋转设置")]
    public float mouseSensitivity = 100f; // 鼠标灵敏度
    public float minCameraAngle = -60f; // 最小相机角度（向下看）
    public float maxCameraAngle = 60f; // 最大相机角度（向上看）
    public float mouseSmoothing = 0.05f; // 鼠标平滑时间
    
    [Header("冲刺设置")]
    public bool toggleSprint = false; // 切换冲刺模式（true=切换，false=按住）

    [Header("菜单设置")]
    public bool isMenuOpen = false; // 是否显示菜单UI（可在Inspector或外部脚本设置）
    public KeyCode toggleMenuKey = KeyCode.Escape; // 切换菜单的快捷键（默认ESC）

    private float xRotation = 0f; // 相机的X轴旋转角度
    private bool isInputEnabled = true; // 控制输入是否启用的标志
    private bool isSprinting = false; // 冲刺状态

    private CharacterController controller; // 角色控制器组件
    private Camera playerCamera; // 玩家相机
    
    // 移动相关变量
    private Vector3 currentVelocity = Vector3.zero; // 当前速度
    private Vector2 currentInput = Vector2.zero; // 当前输入
    private Vector2 inputVelocity = Vector2.zero; // 输入平滑速度
    
    // 鼠标平滑相关变量
    private Vector2 currentMouseDelta = Vector2.zero;
    private Vector2 currentMouseVelocity = Vector2.zero;

    void Start()
    {
        // 获取组件引用
        controller = GetComponent<CharacterController>();
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        // 初始化光标状态
        UpdateCursorState();
    }

    void Update()
    {
        // 处理菜单切换（按快捷键ESC切换菜单显示/隐藏）
        HandleMenuToggle();
        
        // 根据菜单状态更新光标状态
        UpdateCursorState();

        if (isInputEnabled && !isMenuOpen) // 菜单打开时禁用游戏输入
        {
            // 处理移动输入
            HandleMovement();
            
            // 处理鼠标旋转
            HandleMouseLook();
        }
    }

    void HandleMovement()
    {
        // 获取原始WASD输入
        float horizontal = Input.GetAxis("Horizontal"); // A/D 左右移动
        float vertical = Input.GetAxis("Vertical"); // W/S 前进后退
        Vector2 targetInput = new Vector2(horizontal, vertical);
        
        // 限制输入向量大小，防止对角线移动过快
        if (targetInput.magnitude > 1f)
        {
            targetInput.Normalize();
        }

        // 使用SmoothDamp进行输入平滑
        currentInput = Vector2.SmoothDamp(currentInput, targetInput, ref inputVelocity, inputSmoothTime);

        // 处理冲刺输入
        HandleSprintInput();

        // 计算移动方向（基于平滑后的输入）
        Vector3 moveDirection = transform.right * currentInput.x + transform.forward * currentInput.y;

        // 计算目标速度
        float targetSpeed = isSprinting ? moveSpeed * runSpeedMultiplier : moveSpeed;
        Vector3 targetVelocity = moveDirection * targetSpeed;

        // 选择加速度（如果有输入则加速，没有输入则减速）
        float currentAcceleration = currentInput.magnitude > 0.1f ? acceleration : deceleration;

        // 使用加速度平滑过渡到目标速度
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, currentAcceleration * Time.deltaTime);

        // 应用移动
        controller.Move(currentVelocity * Time.deltaTime);
    }

    void HandleSprintInput()
    {
        if (toggleSprint)
        {
            // 切换模式：按一次Shift开始冲刺，再按一次停止
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                isSprinting = !isSprinting;
            }
        }
        else
        {
            // 按住模式：按住Shift冲刺，松开停止
            isSprinting = Input.GetKey(KeyCode.LeftShift);
        }
    }

    void HandleMouseLook()
    {
        // 获取原始鼠标输入
        Vector2 targetMouseDelta = new Vector2(
            Input.GetAxis("Mouse X") * mouseSensitivity,
            Input.GetAxis("Mouse Y") * mouseSensitivity
        );

        // 使用SmoothDamp平滑鼠标输入
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseVelocity, mouseSmoothing);

        // 处理上下旋转（相机）
        xRotation -= currentMouseDelta.y * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, minCameraAngle, maxCameraAngle); // 限制相机上下角度

        // 应用相机旋转
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        // 处理左右旋转（玩家）
        transform.Rotate(Vector3.up * currentMouseDelta.x * Time.deltaTime);
    }

    /// <summary>
    /// 处理菜单切换（按快捷键）
    /// </summary>
    void HandleMenuToggle()
    {
        if (Input.GetKeyDown(toggleMenuKey))
        {
            // 检查当前退出游戏UI是否已经打开
            bool isExitUIOpen = false;
            if (UIManager.Instance != null)
            {
                isExitUIOpen = UIManager.Instance.IsUIVisible(UIManager.UIType.退出游戏UI);
            }
            
            // 如果退出游戏UI已经打开，则关闭它
            if (isExitUIOpen)
            {
                isMenuOpen = false;
                isInputEnabled = true;
                
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.HideUI(UIManager.UIType.退出游戏UI);
                    Debug.Log("隐藏退出游戏UI");
                }
                
                Debug.Log("菜单状态切换：关闭");
            }
            // 如果退出游戏UI未打开，检查是否有其他UI打开
            else
            {
                bool canOpenExitUI = true;
                if (UIManager.Instance != null)
                {
                    // 检查是否有其他UI打开（排除退出游戏UI）
                    canOpenExitUI = !UIManager.Instance.IsAnyOtherUIVisible(UIManager.UIType.退出游戏UI);
                }
                
                if (canOpenExitUI)
                {
                    isMenuOpen = true;
                    isInputEnabled = false;
                    
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowUI(UIManager.UIType.退出游戏UI);
                        Debug.Log("显示退出游戏UI");
                    }
                    
                    // 菜单打开时重置移动状态
                    currentVelocity = Vector3.zero;
                    currentInput = Vector2.zero;
                    inputVelocity = Vector2.zero;
                    currentMouseDelta = Vector2.zero;
                    currentMouseVelocity = Vector2.zero;
                    isSprinting = false;
                    
                    Debug.Log("菜单状态切换：打开");
                }
                else
                {
                    Debug.Log("有其他UI处于打开状态，无法显示退出游戏UI");
                }
            }
        }
    }

    /// <summary>
    /// 根据菜单状态更新光标状态
    /// </summary>
    void UpdateCursorState()
    {
        if (isMenuOpen)
        {
            // 菜单显示时：光标可见，不锁定（可以自由移动操作UI）
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // 菜单隐藏时：光标锁定到屏幕中心，隐藏（游戏游玩状态）
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// 控制玩家输入的启用/禁用（外部调用时会同步菜单状态）
    /// </summary>
    /// <param name="enable">是否启用输入</param>
    public void SetPlayerInputEnabled(bool enable)
    {
        isInputEnabled = enable;
        isMenuOpen = !enable; // 禁用输入 = 打开菜单，启用输入 = 关闭菜单
        
        // 更新光标状态
        UpdateCursorState();
        
        // 禁用输入时重置移动状态
        if (!enable)
        {
            currentVelocity = Vector3.zero;
            currentInput = Vector2.zero;
            inputVelocity = Vector2.zero;
            currentMouseDelta = Vector2.zero;
            currentMouseVelocity = Vector2.zero;
            isSprinting = false;
        }
    }

    /// <summary>
    /// 外部设置菜单显示状态（例如UI脚本调用）
    /// </summary>
    /// <param name="isOpen">是否打开菜单</param>
    public void SetMenuOpen(bool isOpen)
    {
        isMenuOpen = isOpen;
        isInputEnabled = !isOpen;
        
        // 更新光标状态
        UpdateCursorState();
        
        // 菜单打开时重置移动状态
        if (isOpen)
        {
            currentVelocity = Vector3.zero;
            currentInput = Vector2.zero;
            inputVelocity = Vector2.zero;
            currentMouseDelta = Vector2.zero;
            currentMouseVelocity = Vector2.zero;
            isSprinting = false;
        }
    }

    /// <summary>
    /// 获取当前菜单状态（供外部UI脚本判断）
    /// </summary>
    public bool IsMenuOpen()
    {
        return isMenuOpen;
    }

    /// <summary>
    /// 获取当前移动速度（供外部调用）
    /// </summary>
    public float GetCurrentSpeed()
    {
        return currentVelocity.magnitude;
    }

    /// <summary>
    /// 获取是否正在冲刺
    /// </summary>
    public bool IsSprinting()
    {
        return isSprinting;
    }

    /// <summary>
    /// 强制设置冲刺状态（供外部调用）
    /// </summary>
    public void SetSprinting(bool sprint)
    {
        isSprinting = sprint;
    }
}