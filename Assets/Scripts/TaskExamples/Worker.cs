using System;
using System.Threading;
using UnityEngine;

public abstract class Worker : MonoBehaviour
{
    public Action<Worker> OnEnableAction;

    [SerializeField]
    protected int _taskCount;

    private CancellationTokenSource _cts;

    void OnEnable()
    {
        OnEnableAction?.Invoke(this);

        _cts = new CancellationTokenSource();

        RunTasks(_cts.Token);
    }

    void OnDisable()
    {
        _cts?.Cancel();
    }

    protected abstract void RunTasks(CancellationToken ct);
}
