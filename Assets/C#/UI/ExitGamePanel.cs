using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 退出到开始界面 — 按热键弹出确认面板，点「确定」返回开始界面。
/// 挂在一个常驻 GameObject 上（推荐挂 UIManager 或新建空物体）。
/// 退出面板通过 Inspector 拖拽引用。
/// </summary>
public class ExitGamePanel : MonoBehaviour
{
    [Header("退出确认面板（拖拽 Panel GameObject）")]
    [SerializeField] private GameObject exitPanel;

    [Header("面板上的按钮（面板子物体）")]
    [SerializeField] private Button confirmBtn;
    [SerializeField] private Button cancelBtn;

    [Header("触发热键")]
    [SerializeField] private KeyCode triggerKey = KeyCode.Escape;

    [Header("场景名")]
    [SerializeField] private string mainMenuSceneName = "开始界面";

    private SimpleFirstPersonController _playerController;
    private bool _isOpen = false;

    private void Start()
    {
        // 强制 ESC，覆盖 Inspector 旧值
        triggerKey = KeyCode.Escape;

        // 初始隐藏面板
        if (exitPanel != null) exitPanel.SetActive(false);

        if (confirmBtn != null)
            confirmBtn.onClick.AddListener(OnConfirm);
        if (cancelBtn != null)
            cancelBtn.onClick.AddListener(OnCancel);
    }

    private void Update()
    {
        if (Input.GetKeyDown(triggerKey))
        {
            if (_isOpen)
                Close();
            else
                Open();
        }
    }

    private void OnDestroy()
    {
        if (confirmBtn != null) confirmBtn.onClick.RemoveListener(OnConfirm);
        if (cancelBtn != null) cancelBtn.onClick.RemoveListener(OnCancel);
    }

    private void Open()
    {
        _isOpen = true;

        CachePlayerController();
        _playerController?.SetPlayerInputEnabled(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        var crosshair = FindObjectOfType<CrosshairUI>();
        if (crosshair != null && crosshair.crosshairImage != null)
            crosshair.crosshairImage.enabled = false;

        if (exitPanel != null) exitPanel.SetActive(true);
    }

    private void Close()
    {
        _isOpen = false;

        _playerController?.SetPlayerInputEnabled(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        var crosshair = FindObjectOfType<CrosshairUI>();
        if (crosshair != null && crosshair.crosshairImage != null)
            crosshair.crosshairImage.enabled = true;

        if (exitPanel != null) exitPanel.SetActive(false);
    }

    private void OnConfirm()
    {
        // 切场景前确保光标可见（Cursor 状态是全局的，不随场景重置）
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnCancel()
    {
        Close();
    }

    private void CachePlayerController()
    {
        if (_playerController != null) return;
        var player = ObjectManager.Instance?.GetObject(ObjectManager.ObjectType.Player);
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _playerController = player.GetComponent<SimpleFirstPersonController>();
    }
}
