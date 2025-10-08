using System;
using UnityEngine;

public abstract class TutorialObjective : MonoBehaviour
{
    public event Action<TutorialObjective> Completed;

    public bool IsComplete { get; private set; }
    protected bool IsActive { get; private set; }
    protected TutorialManager Manager { get; private set; }

    internal void Configure(TutorialManager manager)
    {
        Manager = manager;
        OnConfigure(manager);
    }

    internal void ResetObjective()
    {
        IsComplete = false;
        IsActive = false;
        OnReset();
    }

    internal void BeginObjective()
    {
        if (IsComplete)
        {
            Completed?.Invoke(this);
            return;
        }

        IsActive = true;
        OnBegin();
    }

    internal void EndObjective()
    {
        if (!IsActive)
            return;

        IsActive = false;
        OnEnd();
    }

    protected void CompleteObjective()
    {
        if (IsComplete)
            return;

        IsComplete = true;
        IsActive = false;
        OnComplete();
        Completed?.Invoke(this);
    }

    protected virtual void OnConfigure(TutorialManager manager) { }
    protected virtual void OnReset() { }
    protected virtual void OnBegin() { }
    protected virtual void OnEnd() { }
    protected virtual void OnComplete() { }
}