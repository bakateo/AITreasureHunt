using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SelectSpoon : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    public Animator spoonAnimator;


    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }
    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnSelect);
    }

    private void OnSelect(SelectEnterEventArgs args)
    {
        spoonAnimator.SetBool("isSelected", true);
    }
}
