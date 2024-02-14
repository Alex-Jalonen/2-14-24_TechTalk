using AsyncCoroutine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class AsyncCoroutineTaskWorker : Worker
{
    protected override async void RunTasks(CancellationToken ct)
    {
        List<Task> tasks = new List<Task>();

        _taskCount = Mathf.Min(_taskCount, 1000);

        // Initialize the list with 1000 tasks
        for (int i = 0; i < _taskCount; i++)
        {
            tasks.Add(WaitTask(ct));
        }

        // Replace completed tasks with new ones
        while (!ct.IsCancellationRequested)
        {
            await Task.WhenAny(tasks);

            for (int i = 0; i < tasks.Count; i++)
            {
                if (tasks[i].IsCompleted)
                {
                    tasks[i] = WaitTask(ct);
                }
            }
        }
    }

    private async Task WaitTask(CancellationToken ct)
    {
        var delay = Random.Range(.1f, 1f);

        await TaskEx.DelaySafe(TimeSpan.FromSeconds(delay), ct);
    }
}
