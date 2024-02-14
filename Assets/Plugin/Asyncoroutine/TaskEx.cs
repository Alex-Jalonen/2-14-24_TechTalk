using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace AsyncCoroutine
{
    [Preserve]
    public static class TaskEx
    {
        /// <summary>
        ///     Blocks while condition is true or <see cref="timeoutInMS" /> occurs.
        /// </summary>
        /// <param name="condition">The condition that will perpetuate the block.</param>
        /// <param name="timeoutInMS">Timeout in milliseconds. Must be > 0.</param>
        /// <param name="frequencyInMS">The frequency at which the condition will be check, in milliseconds. -1 means "every frame"</param>
        /// <exception cref="TimeoutException"></exception>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task WaitWhile(Func<bool> condition, uint timeoutInMS = 1000, int frequencyInMS = -1)
        {
            await WaitWhile(condition, CancellationToken.None, (int)timeoutInMS, frequencyInMS);
        }

        /// <summary>
        ///     Blocks while condition is true, the <see cref="CancellationToken" /> is triggered, or <see cref="timeoutInMS" />
        ///     occurs.
        /// </summary>
        /// <param name="condition">The condition that will perpetuate the block.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <param name="timeoutInMS">Timeout in milliseconds. -1 means "no timeout" but MUST have a valid CancellationToken.</param>
        /// <param name="frequencyInMS">The frequency at which the condition will be check, in milliseconds.</param>
        /// <exception cref="TimeoutException"></exception>
        /// <returns></returns>
        public static async Task WaitWhile(Func<bool> condition, CancellationToken ct, int timeoutInMS = -1, int frequencyInMS = -1)
        {
            Debug.Assert(!(timeoutInMS <= 0 && ct.Equals(CancellationToken.None)), "If you're going to wait without a timeout, you must supply a CancellationToken.");

            async Task WaitWhileInternal(Func<bool> c, CancellationToken token, int frequency)
            {
                switch (frequency <= 0)
                {
                case true: // Wait for the next frame by default
                    while (c()) await DelaySafe(frequency, token);
                    break;
                case false: // Otherwise, wait the specified frequency
                    try
                    {
                        while (c())
                            await Task.Delay(frequency, token);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }

                    break;
                }
            }

            var waitTask = WaitWhileInternal(condition, ct, frequencyInMS);

            if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeoutInMS, ct)))
                throw new TimeoutException();
        }

        /// <summary>
        ///     Blocks until condition is true or <see cref="timeoutInMS" /> occurs.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="timeoutInMS"></param>
        /// <param name="frequencyInMS"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task WaitUntil(Func<bool> condition, uint timeoutInMS = 1000, int frequencyInMS = -1)
        {
            await WaitUntil(condition, CancellationToken.None, (int)timeoutInMS, frequencyInMS);
        }

        /// <summary>
        ///     Blocks until condition is true, the <see cref="CancellationToken" /> is signaled, or <see cref="timeoutInMS" />
        ///     occurs.
        /// </summary>
        /// <param name="condition">The break condition.</param>
        /// <param name="ct">The cancellation token</param>
        /// <param name="timeoutInMS">The timeout in milliseconds.</param>
        /// <param name="frequencyInMS">
        ///     The frequency at which the condition will be checked. A value 0 or less means "wait for
        ///     next frame"
        /// </param>
        /// <returns></returns>
        public static async Task WaitUntil(Func<bool> condition, CancellationToken ct, int timeoutInMS = -1, int frequencyInMS = -1)
        {
            Debug.Assert(!(timeoutInMS <= 0 && ct.Equals(CancellationToken.None)), "If you're going to wait without a timeout, you must supply a CancellationToken.");

            async Task WaitUntilInternal(Func<bool> c, CancellationToken token, int frequency)
            {
                switch (frequency <= 0)
                {
                case true: // Wait for the next frame by default
                    while (!c()) await DelaySafe(frequency, token);
                    break;
                case false: // Otherwise, wait the specified frequency
                    try
                    {
                        while (!c())
                            await Task.Delay(frequency, token);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }

                    break;
                }
            }

            var waitTask = WaitUntilInternal(condition, ct, frequencyInMS);
            var timeoutTask = Task.Delay(timeoutInMS, ct);

            var completedTask = await Task.WhenAny(waitTask, timeoutTask);

            var timedOut = completedTask == timeoutTask;

            if (!completedTask.IsCanceled && timedOut)
                throw new TimeoutException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task DelaySafe(int milliseconds)
        {
            await DelaySafe(milliseconds, CancellationToken.None);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task DelaySafe(int milliseconds, CancellationToken ct)
        {
            await DelaySafe(TimeSpan.FromMilliseconds(milliseconds), ct);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task DelaySafe(TimeSpan timespan)
        {
            await DelaySafe(timespan, CancellationToken.None);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task DelaySafe(TimeSpan timespan, CancellationToken ct)
        {
            if (timespan.TotalMilliseconds < 0)
            {
                await new WaitForNextFrame();
                return;
            }

            var startTime = Time.timeAsDouble;
            while (!ct.IsCancellationRequested && Time.timeAsDouble - startTime < timespan.TotalSeconds)
                await new WaitForNextFrame();
        }

        public static async Task WaitForCancellationAsync(CancellationToken cancellationToken)
        {
            // Create a TaskCompletionSource to represent the cancellation task.
            var tcs = new TaskCompletionSource<bool>();

            // Register a callback to complete the TaskCompletionSource when the token is canceled.
            cancellationToken.Register(() => tcs.SetResult(true));

            // Wait for the cancellation task to complete.
            await tcs.Task;
        }
    }
}