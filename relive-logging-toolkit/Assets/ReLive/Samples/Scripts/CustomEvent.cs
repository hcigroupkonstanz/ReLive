using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReLive.Events;

public class CustomEvent : MonoBehaviour
{
    private int clickCount = 0;
    public void TriggerCustomEvent()
    {
        string attribute1 = "Hi";
        clickCount++;
        ReliveEvent.Log(ReliveEventType.Custom, new Dictionary<string, object>
        {
            { "attribute1", attribute1 },
            { "clickCount", clickCount }
        });
    }
}
