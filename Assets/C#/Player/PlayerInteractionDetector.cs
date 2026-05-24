using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionDetector : MonoBehaviour
{
    [Header("Detection")]
    public float interactDistance = 3f;
    public LayerMask interactMask = ~0;

    [Header("Debug")]
    public Interactables currentInteractable;

    // 调试模式：开启后会输出检测信息
    [Header("Debug Settings")]
    public bool debugMode = true;

    void Update()
    {
        DetectInteractable();
    }

    void DetectInteractable()
    {
        Interactables previous = currentInteractable;
        currentInteractable = null;

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactMask))
        {
            if (debugMode)
            {
                Debug.DrawRay(transform.position, transform.forward * interactDistance, Color.red);
                Debug.Log($"[检测] 射线命中: {hit.collider.name}");
            }

            Interactables interactable = hit.collider.GetComponent<Interactables>();

            if (interactable != null)
            {
                currentInteractable = interactable;
                if (previous != interactable && debugMode)
                {
                    Debug.Log($"[检测] 发现可交互物: {interactable.npcName} - {interactable.actionType}");
                }
            }
            else if (debugMode)
            {
                Debug.Log($"[检测] 射线命中 {hit.collider.name}，但没有 Interactables 组件");
            }
        }
    }
}
