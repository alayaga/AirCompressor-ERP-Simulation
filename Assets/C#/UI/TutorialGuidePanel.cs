using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 操作指引面板 — 首次进入场景时显示，点击「确定」后关闭且不再显示。
/// 挂在指引面板的根节点上。需拖拽 confirmBtn 引用。
/// </summary>
public class TutorialGuidePanel : MonoBehaviour
{
    [Header("按钮引用")]
    [SerializeField] private Button confirmBtn;

    /// <summary>是否已看过指引（运行时静态标记，当前会话不再显示）</summary>
    private static bool _hasBeenShown = false;

    private SimpleFirstPersonController _playerController;

    private void Start()
    {
        if (_hasBeenShown)
        {
            gameObject.SetActive(false);
            return;
        }

        _hasBeenShown = true;

        if (confirmBtn != null)
            confirmBtn.onClick.AddListener(OnConfirmClicked);

        // 延迟一帧确保所有 Manager 初始化完毕
        StartCoroutine(ShowDelayed());
    }

    private IEnumerator ShowDelayed()
    {
        yield return null; // 等一帧

        // 找到玩家控制器
        if (_playerController == null)
        {
            var player = ObjectManager.Instance?.GetObject(ObjectManager.ObjectType.Player);
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _playerController = player.GetComponent<SimpleFirstPersonController>();
        }

        // 锁定玩家并显示鼠标
        _playerController?.SetPlayerInputEnabled(false);

        // 兜底：直接设光标（有些版本 UnlockCursor 不生效）
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 隐藏准心
        var crosshair = FindObjectOfType<CrosshairUI>();
        if (crosshair != null && crosshair.crosshairImage != null)
            crosshair.crosshairImage.enabled = false;

        gameObject.SetActive(true);
        Debug.Log($"[TutorialGuidePanel] 指引面板已显示, hasController={_playerController != null}");
    }

    private void OnDestroy()
    {
        if (confirmBtn != null)
            confirmBtn.onClick.RemoveListener(OnConfirmClicked);
    }

    private void OnConfirmClicked()
    {
        // 恢复准心
        var crosshair = FindObjectOfType<CrosshairUI>();
        if (crosshair != null && crosshair.crosshairImage != null)
            crosshair.crosshairImage.enabled = true;

        // 解锁玩家并隐藏鼠标
        _playerController?.SetPlayerInputEnabled(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        gameObject.SetActive(false);
    }
}
