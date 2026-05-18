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

    void Update()
    {
        DetectInteractable();
    }

    void DetectInteractable()
    {
        currentInteractable = null;

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactMask))
        {
            Interactables interactable = hit.collider.GetComponent<Interactables>();

            if (interactable != null)
            {
                currentInteractable = interactable;
            }
        }
    }
}
