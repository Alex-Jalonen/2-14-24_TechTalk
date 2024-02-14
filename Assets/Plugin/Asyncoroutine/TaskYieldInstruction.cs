using System;
using System.Threading.Tasks;
using UnityEngine;

// It refers to this link : https://github.com/Microsoft/xbox-live-unity-plugin/blob/master/Assets/Xbox%20Live/Scripts/UnityTaskExtensions.cs

namespace AsyncCoroutine
{
    public class TaskYieldInstruction : CustomYieldInstruction
    {
        public TaskYieldInstruction(Task task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            Task = task;
        }

        public Task Task { get; }

        public override bool keepWaiting
        {
            get
            {
                if (Task.Exception != null)
                    throw Task.Exception;

                return !Task.IsCompleted;
            }
        }
    }

    public class TaskYieldInstruction<T> : TaskYieldInstruction
    {
        public TaskYieldInstruction(Task<T> task)
            : base(task)
        {
            Task = task;
        }

        public new Task<T> Task { get; }

        public T Result => Task.Result;
    }
}