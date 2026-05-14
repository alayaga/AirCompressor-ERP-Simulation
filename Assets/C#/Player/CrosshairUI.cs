using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    public Image crosshairImage;
    public PlayerInteractionDetector detector;

    public Color normalColor = Color.black;
    public Color interactColor = Color.green;

    void Update()
    {
        if (crosshairImage == null || detector == null)
            return;

        if (detector.currentInteractable != null)
        {
            crosshairImage.color = interactColor;
        }
        else
        {
            crosshairImage.color = normalColor;
        }
    }
}
