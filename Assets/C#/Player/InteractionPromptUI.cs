using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    public PlayerInteractionDetector detector;
    public TextMeshProUGUI promptText;

    void Update()
    {
        UpdatePrompt();
    }

    void UpdatePrompt()
    {
        if (detector == null || promptText == null)
            return;

        if (detector.currentInteractable != null)
        {
            promptText.gameObject.SetActive(true);
            promptText.text = "[E] " + detector.currentInteractable.interactText;
        }
        else
        {
            promptText.gameObject.SetActive(false);
        }
    }
}
