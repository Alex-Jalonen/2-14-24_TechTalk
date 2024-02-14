using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

public class UniTaskWorker : Worker
{
    private List<UniTask> tasks;

    protected override void RunTasks(CancellationToken ct)
    {
        tasks = new List<UniTask>();

        _taskCount = Mathf.Min(_taskCount, 100000);

        // Initialize the list with 1000 tasks
        for (int i = 0; i < _taskCount; i++)
        {
            tasks.Add(UniTask.CompletedTask);

            RefreshIndex(i, ct);
        }
    }

    private void RefreshIndex(int i, CancellationToken ct)
    {
        tasks[i] = WaitUniTask(i, ct);
    }

    private async UniTask WaitUniTask(int i, CancellationToken ct)
    {
        var delay = Random.Range(.1f, 1f);

        await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: ct);

        RefreshIndex(i, ct);
    }
}