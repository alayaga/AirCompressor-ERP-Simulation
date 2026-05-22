using UnityEngine;

public enum UIFormMode
{
    Fill,
    Approve,
    PushDown
}

/// <summary>
/// UI表单基类
/// </summary>
public class UIFormBase : MonoBehaviour
{
    [Header("模式设置")]
    [SerializeField] protected UIFormMode currentMode = UIFormMode.Fill;

    [Header("UI引用")]
    [SerializeField] protected GameObject fillModeUI;
    [SerializeField] protected GameObject approveModeUI;
    [SerializeField] protected GameObject pushDownModeUI;

    protected bool isVisible = false;

    protected virtual void Awake()
    {
        SetAllUIVisibility(false);
    }

    public virtual void ShowUIMode(UIFormMode mode)
    {
        currentMode = mode;
        SetAllUIVisibility(false);

        switch (mode)
        {
            case UIFormMode.Fill:
                if (fillModeUI != null) fillModeUI.SetActive(true);
                break;
            case UIFormMode.Approve:
                if (approveModeUI != null) approveModeUI.SetActive(true);
                break;
            case UIFormMode.PushDown:
                if (pushDownModeUI != null) pushDownModeUI.SetActive(true);
                break;
        }

        gameObject.SetActive(true);
        isVisible = true;

        // ✅ 修改：将 PlayerController 改为 SimpleFirstPersonController
        var player = ObjectManager.Instance?.GetObject(ObjectManager.ObjectType.Player);
        player?.GetComponent<SimpleFirstPersonController>()?.SetPlayerInputEnabled(false);
    }

    public virtual void HideUI()
    {
        SetAllUIVisibility(false);
        gameObject.SetActive(false);
        isVisible = false;

        // ✅ 修改：将 PlayerController 改为 SimpleFirstPersonController
        var player = ObjectManager.Instance?.GetObject(ObjectManager.ObjectType.Player);
        player?.GetComponent<SimpleFirstPersonController>()?.SetPlayerInputEnabled(true);
    }

    public void SetAllUIVisibility(bool visible)
    {
        if (fillModeUI != null) fillModeUI.SetActive(visible);
        if (approveModeUI != null) approveModeUI.SetActive(visible);
        if (pushDownModeUI != null) pushDownModeUI.SetActive(visible);
    }

    public virtual void SwitchMode(UIFormMode mode)
    {
        if (isVisible)
            ShowUIMode(mode);
        else
            currentMode = mode;
    }
}