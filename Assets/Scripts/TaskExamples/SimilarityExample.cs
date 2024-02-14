using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
// disabling this warning because I want to author some async functions that don't actually do awaiting
#pragma warning disable CS1998
// disabling this warning because this is an example file where we want to
// be able to turn functions off by early returning
#pragma warning disable CS0162

public class SimilarityExample : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        //to inspect specific chapters jump to the function and comment out the first line return

        SamePattern();
        AsyncFunctionsThatReturnVoid();

        CancellationTokens();
        DavesDelayTip();
        TestingExceptionThrowing();
        CancellationTokenParameterOrderTip();

        SamePatternCont();
        DrawBack();
        Compatibility();
        Misc();
    }

    #region Same pattern & Async functions that return void
    private void SamePattern()
    {
        return;

        DoAsyncWork_WithoutTask();
        _ = DoAsyncWork_WithUniTaskVoid();

        //NOTE: check out this syntax
        // _ = DoAsyncWork_WithUniTask();
        //
        // check out what happens when you remove `_ =`
        //
        // check out when you put `_ =` infront of DoAsyncWork_WithTask
    }

    private async void AsyncFunctionsThatReturnVoid()

    {
        //when your async function returns void it means theres no Task
        //to keep track of the function. UniTask mirrors this with 
        //UniTaskVoid, which is an object that is returned but it not awaitable

        // check out the errors when you uncomment these lines
        //await DoAsyncWork_WithoutTask();
        //await DoAsyncWork_WithUniTask();
    }

    private async void DoAsyncWork_WithoutTask()
    {
        Debug.Log("[w/oT] Starting some work...");

        await Task.Delay(TimeSpan.FromSeconds(1f));

        Debug.Log("[w/oT] Done with some work!");
    }

    private async UniTaskVoid DoAsyncWork_WithUniTaskVoid()
    {
        Debug.Log("[UTV] Starting some work...");

        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        Debug.Log("[UTV] Done with some work!");
    }
    #endregion

    #region Cancellation Tokens, Dave's Delay tip, and Exception in async functions
    private async void CancellationTokens()
    {
        return;

        var cts = new CancellationTokenSource();
        CancelSoon(cts);

        CancellableWork_WithoutTask(cts.Token);
        _ = CancellableWork_WithUniTaskVoid(cts.Token);
    }

    private async void DavesDelayTip()
    {
        return;

        // Dave pointed out a particular issue that comes up with using delay
        // you may have spotted it in the CancellableWork functions

        await CallingTroublesomeDelayCancellation_WithoutTask();
        await Task.Delay(TimeSpan.FromSeconds(.1f));

        // without a try-catch block, cancelling a delay sends an exception that causes
        // the flow of execution to leave the scope of the function and return early
        // meaning we never call out "done with work" function

        // in a real example this could leave something hanging in a weird state
        // or even worse, cause a resource leak!

        // even more alarm, however, is what happens when an exception is throw inside a task...
        await CallingTroublesomeDelayCancellation_WithTask();
        await Task.Delay(TimeSpan.FromSeconds(.1f));

        // this is the same code, same exception is throw, but because the function returns 
        // a task, the exception gets eaten. (why? idk, kinda scares me tbh)

        // with a cursory glance it may seem that unitask behaves the same way...

        await CallingTroublesomeDelayCancellation_WithUniTask();
        await Task.Delay(TimeSpan.FromSeconds(.1f));

        // however! We can tell UniTask to make some noise about this exception

        UniTaskScheduler.PropagateOperationCanceledException = true;
        await CallingTroublesomeDelayCancellation_WithUniTask();

        // Additionally, The OperationCanceledException is the only exception that may be
        // eaten this way. All other exception make it to the logs when using UniTask

        // What we'll probably want to do is make a UniTaskExtensions static class with a SafeDelay
        // that catches and discards the cancellation exception because often we have work we want
        // to finish up before releasing control back up the control flow
    }

    private async void TestingExceptionThrowing()
    {
        return;

        // heres the different types of async function and how they respond to exceptions being throw

        Debug.Log("exception test begin");

        TestExceptionThrower_WithoutTask();          // does get logged
        _ = TestExceptionThrower_WithTask();         // does NOT get logged
        _ = TestCancelExceptionThrower_WithTask();   // does NOT get logged
        _ = TestException_WithUniTaskVoid();         // does get logged
        _ = TestException_WithUniTask();             // does get logged

        UniTaskScheduler.PropagateOperationCanceledException = false;
        _ = TestCancelException_WithUniTaskVoid();   // does NOT get logged (by default)
        _ = TestCancelException_WithUniTask();       // does NOT get logged (by default)

        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        UniTaskScheduler.PropagateOperationCanceledException = true;
        _ = TestCancelException_WithUniTaskVoid();   // does get logged (when specified)
        _ = TestCancelException_WithUniTask();       // does NOT get logged (this one baffles me a bit, would love others thoughts)
    }

    private void CancellationTokenParameterOrderTip()
    {
        return;

        //NOTE: By putting the cancellation token parameter at the end it makes it easy to
        // refactor the function to be overloaded to accept a cancellation token or not.
        // The Named Argument/Parameter feature of C# makes this not super necessary but still useful

        OverloadedAsyncFunctionWithOptionalCt(42, "hello world");
        OverloadedAsyncFunctionWithOptionalCt(42, "hello world", ct: CancellationToken.None);
    }

    private async void CancellableWork_WithoutTask(CancellationToken ct)
    {
        Debug.Log("[w/oT] Starting some work...");

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(3f), ct);
        }
        catch (TaskCanceledException e)
        {
            Debug.LogWarning(e);
        }

        Debug.Log("[w/oT] Done with some work!");
    }

    private async UniTaskVoid CancellableWork_WithUniTaskVoid(CancellationToken ct)
    {
        Debug.Log("[UTV] Starting some work...");

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(3f), cancellationToken: ct);
        }
        catch (OperationCanceledException e)
        {
            Debug.LogWarning(e);
        }

        Debug.Log("[UTV] Done with some work!");
    }

    private async void OverloadedAsyncFunctionWithOptionalCt(int param, string message)
    {
        Debug.Log($"[w/oT] Not using parameter {param} or message {message}");
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
    }

    private async void OverloadedAsyncFunctionWithOptionalCt(int param, string message, CancellationToken ct)
    {
        Debug.Log($"[w/oT] Not using parameter {param} or message {message}");
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: ct);
    }

    private async Task CallingTroublesomeDelayCancellation_WithoutTask()
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        //Debug.Log("[w/oT] Before call");

        //set up the work to run
        TroublesomeDelayCancellation_WithoutTask(ct);

        //wait a little bit
        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        //cancel before the work is done
        cts.Cancel();

        // and if you're curious, the exception we caused doesn't break this function
        //Debug.Log("[w/oT] After call");
    }

    private async Task CallingTroublesomeDelayCancellation_WithTask()
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        //Debug.Log("[T] Before call");

        //set up the work to run
        _ = TroublesomeDelayCancellation_WithTask(ct);

        //wait a little bit
        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        //cancel before the work is done
        cts.Cancel();

        // and if you're curious, the exception we caused doesn't break this function
        //Debug.Log("[T] After call");
    }

    private async Task CallingTroublesomeDelayCancellation_WithUniTask()
    {
        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        //set up the work to run
        _ = TroublesomeDelayCancellation_WithUniTaskVoid(ct);

        //wait a little bit
        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        //cancel before the work is done
        cts.Cancel();
    }

    private async void TroublesomeDelayCancellation_WithoutTask(CancellationToken ct)
    {
        Debug.Log("[w/oT] Starting some work...");

        // no try-catch!
        await Task.Delay(TimeSpan.FromSeconds(3f), ct);

        Debug.Log("[w/oT] Done with some work!");
    }

    private async Task TroublesomeDelayCancellation_WithTask(CancellationToken ct)
    {
        Debug.Log("[T] Starting some work...");

        //no try-catch!
        await Task.Delay(TimeSpan.FromSeconds(3f), ct);

        Debug.Log("[T] Done with some work!");
    }

    private async UniTaskVoid TroublesomeDelayCancellation_WithUniTaskVoid(CancellationToken ct)
    {
        Debug.Log("[UTV] Starting some work...");

        //no try-catch!
        await UniTask.Delay(TimeSpan.FromSeconds(3f), cancellationToken: ct);

        Debug.Log("[UTV] Done with some work!");
    }

    private async void TestExceptionThrower_WithoutTask()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(.1f));
        throw new Exception("[w/oT] test exception");
    }

    private async Task TestExceptionThrower_WithTask()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(.1f));
        throw new Exception("[T] test exception");
    }

    private async Task TestCancelExceptionThrower_WithTask()
    {
        var cts = new CancellationTokenSource();
        _ = Task.Delay(TimeSpan.FromSeconds(.1f), cts.Token);
        cts.Cancel();
    }

    private async UniTask TestException_WithUniTask()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(.1f));
        throw new Exception("[UT] test exception");
    }

    private async UniTaskVoid TestException_WithUniTaskVoid()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(.1f));
        throw new Exception("[UTV] test exception");
    }

    private async UniTask TestCancelException_WithUniTask()
    {
        var cts = new CancellationTokenSource();
        CancelSoon(cts);
        await UniTask.Delay(TimeSpan.FromSeconds(3f), cancellationToken: cts.Token);
    }

    private async UniTaskVoid TestCancelException_WithUniTaskVoid()
    {
        var cts = new CancellationTokenSource();
        CancelSoon(cts);
        await UniTask.Delay(TimeSpan.FromSeconds(3f), cancellationToken: cts.Token);
    }

    private async void CancelSoon(CancellationTokenSource cts)
    {
        await Task.Delay(TimeSpan.FromSeconds(.1f));
        cts.Cancel();
    }

    #endregion

    #region Same Pattern Cont, Compatibility, and Misc
    private async void SamePatternCont()
    {
        return;

        // so I've showed off a lot of `async void` and `async UniTaskVoid`
        // functions but lets also peak at `async Task` and `async UniTask`
        // you know, for sanity's sake

        await DoAsyncWork_WithTask();
        await DoAsyncWork_WithUniTask();

        // here, by returning a task object we're able to await these async functions
        // this is useful when you want to wait for some action to happen in game
        // before continuing the flow of execution in this function

        await WaitUntilThing_WithTask();
        await WaitUntilThing_WithUniTask();

        // here's where the paradigm begins to shift though,
        // check out these examples of waiting for input ^
    }

    private async void DrawBack()
    {
        return;

        // while testing the amount of overhead that Task and UniTask produce
        // I discovered one critical pattern that we use with Task that UniTask
        // doesn't support

        await ConvenientWaitAny_WithTask();
        // vs
        try
        {
            await MalfunctioningWaitAny_WithUniTask();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        await Task.Delay(TimeSpan.FromSeconds(1.5f));

        // but it's ok, we can still work around this
        await WorkAroundWaitAny_WithUniTask();
    }

    private async void Compatibility()
    {
        return;

        // look, they're so polite!

        _ = TaskWaitsForUniTask_WithTask();
        _ = UniTaskWaitsForTask_WithUniTask();

        // Note: you also aren't obligate to do anything with a returned task
    }

    private void Misc()
    {
        return;

        var uniTask = DoAsyncWork_WithUniTask();

        Debug.Log(uniTask.Status);

        var task = DoAsyncWork_WithTask();

        Debug.Log(task.Status);

        /// check out <see cref="UniTaskStatus"/> and <see cref="TaskStatus"/>
        // UniTask is a bit simpler, which I personally favor for the work we're doing with tasks
    }

    private async Task DoAsyncWork_WithTask()
    {
        Debug.Log("[T] Starting some work...");

        await Task.Delay(TimeSpan.FromSeconds(1f));

        Debug.Log("[T] Done with some work!");
    }

    private async Task DoAsyncWork_WithTask(float seconds)
    {
        Debug.Log("[T] Starting some work...");

        await Task.Delay(TimeSpan.FromSeconds(seconds));

        Debug.Log("[T] Done with some work!");
    }

    private async UniTask DoAsyncWork_WithUniTask()
    {
        Debug.Log("[UT] Starting some work...");

        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        Debug.Log("[UT] Done with some work!");
    }

    private async UniTask DoAsyncWork_WithUniTask(float seconds)
    {
        Debug.Log("[UT] Starting some work...");

        await UniTask.Delay(TimeSpan.FromSeconds(seconds));

        Debug.Log("[UT] Done with some work!");
    }

    private async Task WaitUntilThing_WithTask(CancellationToken ct = default)
    {
        Debug.Log("[T] Starting waiting until thing...");

        while (!ct.IsCancellationRequested)
        {
            var conditionTrue = Input.GetKeyDown(KeyCode.Space);

            if (conditionTrue)
            {
                break;
            }

            await Task.Yield();
            // here we actually would use await new WaitUntilNextFrame in our project
            // from the asyncCoroutine package dave provided. this has overhead to
            // it and slows the project down due to GC. 

            // after a quick investigation this does seem to similar to checking
            // every frame, but im not sure about the down sides to Task.Yield :shrug:
        }

        Debug.Log($"[T] Thing happened!");
    }

    private async UniTask WaitUntilThing_WithUniTask(CancellationToken ct = default)
    {
        Debug.Log("[UT] Starting waiting until thing...");

        // it's so beautiful it'll bring a tear to a grown man's eye :')
        await UniTask.WaitUntil(
            predicate: () => Input.GetKeyDown(KeyCode.Space),
            timing: PlayerLoopTiming.Update,
            cancellationToken: ct);

        Debug.Log($"[UT] Thing happened!");
    }

    private async Task TaskWaitsForUniTask_WithTask()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(.1f));
    }

    private async UniTask UniTaskWaitsForTask_WithUniTask()
    {
        await Task.Delay(TimeSpan.FromSeconds(.1f));
    }

    private async Task ConvenientWaitAny_WithTask()
    {
        Debug.Log("[T] ConvenientWaitAny_WithTask start");

        var tasks = new List<Task>()
        {
            DoAsyncWork_WithTask(.5f),
            DoAsyncWork_WithTask(1f),
            DoAsyncWork_WithTask(1.5f)
        };

        while (tasks.Count > 0)
        {
            var completedTask = await Task.WhenAny(tasks);

            tasks.Remove(completedTask);

            Debug.Log("[T] Task finished, continuing to wait on remaining task");
        }
    }

    private async UniTask MalfunctioningWaitAny_WithUniTask()
    {
        Debug.Log("[UT] MalfunctioningWaitAny_WithUniTask start");

        var tasks = new List<UniTask>()
        {
            DoAsyncWork_WithUniTask(.5f),
            DoAsyncWork_WithUniTask(1f),
            DoAsyncWork_WithUniTask(1.5f)
        };

        while (tasks.Count > 0)
        {
            var index = await UniTask.WhenAny(tasks);

            tasks.RemoveAt(index);

            Debug.Log("[UT] Task finished, continuing to wait on remaining task");
        }
    }

    private async UniTask WorkAroundWaitAny_WithUniTask()
    {
        Debug.Log("[UT] WorkAroundWaitAny_WithUniTask start");

        var tasks = new List<UniTask>()
        {
            DoAsyncWork_WithUniTask(.5f),
            DoAsyncWork_WithUniTask(1f),
            DoAsyncWork_WithUniTask(1.5f)
        };

        Action<UniTask> RemoveFunc = (UniTask task) =>
        {
            tasks.Remove(task);
        };

        foreach (var task in tasks)
        {
            RunThenRemove(task, RemoveFunc);
        }

        await UniTask.WaitUntil(() => tasks.Count <= 0);
    }

    private async void RunThenRemove(UniTask task, Action<UniTask> removeFunc)
    {
        await task;
        removeFunc(task);
        Debug.Log("[UT] Task finished, continuing to wait on remaining task");
    }
    #endregion
}
