using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncCoroutine
{
    public partial class AwaiterCoroutine<TInstruction> : INotifyCompletion
    {
        private Action _continuation;

        private bool _isCompleted;

        public AwaiterCoroutine()
        {
        }

        public AwaiterCoroutine(TInstruction instruction)
        {
            ProcessCoroutine(instruction);
        }

        public TInstruction Instruction { get; protected set; }
        public Enumerator Coroutine { get; private set; }

        public bool IsCompleted
        {
            get => _isCompleted;
            protected set
            {
                _isCompleted = value;

                if (value && _continuation != null)
                {
                    _continuation();
                    _continuation = null;
                }
            }
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            OnCompleted(continuation);
        }

        private void ProcessCoroutine(TInstruction instruction)
        {
            Instruction = instruction;
            Coroutine = new Enumerator(this);

            AwaiterCoroutineer.Instance.StartAwaiterCoroutine(this);
        }

        public TInstruction GetResult()
        {
            return Instruction;
        }

        protected virtual void OnCompleted(Action continuation)
        {
            _continuation = continuation;
        }
    }

    public class AwaiterCoroutineWaitForMainThread : AwaiterCoroutine<WaitForMainThread>
    {
        public AwaiterCoroutineWaitForMainThread()
        {
            Instruction = default;
        }

        protected override void OnCompleted(Action continuation)
        {
            base.OnCompleted(continuation);

            if (SynchronizationContext.Current != null)
                IsCompleted = true;
            else
                AwaiterCoroutineer.Instance.SynchronizationContext.Post(state => { IsCompleted = true; }, null);
        }
    }
}