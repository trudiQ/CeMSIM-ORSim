using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleThrowable : Throwable
{
    // Allow all throwable functions to be toggleable so only the interactable is used
    public bool throwableEnabled;

    public void Toggle()
    {
        SetState(!throwableEnabled);
    }

    public void SetState(bool state)
    {
        throwableEnabled = state;
    }

    protected override void OnHandHoverBegin(Hand hand)
    {
        if (throwableEnabled)
            base.OnHandHoverBegin(hand);
    }

    protected override void HandHoverUpdate(Hand hand)
    {
        if (throwableEnabled)
            base.HandHoverUpdate(hand);
    }

    protected override void OnAttachedToHand(Hand hand)
    {
        if (throwableEnabled)
            base.OnAttachedToHand(hand);
    }

    protected override void OnDetachedFromHand(Hand hand)
    {
        if (throwableEnabled)
            base.OnDetachedFromHand(hand);
    }

    protected override void HandAttachedUpdate(Hand hand)
    {
        if (throwableEnabled)
            base.HandAttachedUpdate(hand);
    }

    protected override IEnumerator LateDetach(Hand hand)
    {
        if (throwableEnabled)
            yield return LateDetach(hand);
        else
            yield return null;
    }

    protected override void OnHandFocusAcquired(Hand hand)
    {
        if (throwableEnabled)
            base.OnHandFocusAcquired(hand);
    }
    protected override void OnHandFocusLost(Hand hand)
    {
        if (throwableEnabled)
            base.OnHandFocusLost(hand);
    }
}