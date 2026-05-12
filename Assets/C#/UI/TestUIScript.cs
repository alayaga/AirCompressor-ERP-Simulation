using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 测试UI脚本
/// </summary>
public class TestUIScript : MonoBehaviour
{
    [Header("UI元素")]
    public Text displayText;
    public Text buttonText;
    public Button interactionButton;
    
    private bool _isButtonClicked = false;

    private void Awake()
    {
        if (interactionButton != null)
            interactionButton.onClick.AddListener(OnButtonClicked);
    }

    public void SetTexts(string displayContent, string buttonContent)
    {
        if (displayText != null) displayText.text = displayContent;
        if (buttonText != null) buttonText.text = buttonContent;
    }
    
    public void UpdateDisplayText(string content)
    {
        if (displayText != null) displayText.text = content;
    }
    
    public void UpdateButtonText(string content)
    {
        if (buttonText != null) buttonText.text = content;
    }
    
    private void OnButtonClicked()
    {
        _isButtonClicked = true;
    }
    
    public bool IsButtonClicked() => _isButtonClicked;
    
    public void ResetButtonClicked()
    {
        _isButtonClicked = false;
    }
    
    public IEnumerator WaitForButtonClick()
    {
        ResetButtonClicked();
        while (!_isButtonClicked) yield return null;
    }
    
    private void OnDestroy()
    {
        if (interactionButton != null)
            interactionButton.onClick.RemoveListener(OnButtonClicked);
    }
}
