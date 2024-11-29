using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/OnChangedState")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "OnChangedState", message: "On Changed State", category: "Events", id: "0dc2d5ea38bed9d4c5740446c308ece7")]
public partial class OnChangedState : EventChannelBase
{
    public delegate void OnChangedStateEventHandler();
    public event OnChangedStateEventHandler Event; 

    public void SendEventMessage()
    {
        Event?.Invoke();
    }

    public override void SendEventMessage(BlackboardVariable[] messageData)
    {
        Event?.Invoke();
    }

    public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)
    {
        OnChangedStateEventHandler del = () =>
        {
            callback();
        };
        return del;
    }

    public override void RegisterListener(Delegate del)
    {
        Event += del as OnChangedStateEventHandler;
    }

    public override void UnregisterListener(Delegate del)
    {
        Event -= del as OnChangedStateEventHandler;
    }
}

