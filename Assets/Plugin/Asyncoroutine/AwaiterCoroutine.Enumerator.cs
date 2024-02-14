using System.Collections;

namespace AsyncCoroutine
{
    public partial class AwaiterCoroutine<TInstruction>
    {
        public class Enumerator : IEnumerator
        {
            private readonly IEnumerator _nestedCoroutine;
            private readonly AwaiterCoroutine<TInstruction> _parent;

            public Enumerator(AwaiterCoroutine<TInstruction> parent)
            {
                _parent = parent;
                _nestedCoroutine = parent.Instruction as IEnumerator;
            }

            public object Current { get; private set; }

            bool IEnumerator.MoveNext()
            {
                if (_nestedCoroutine != null)
                {
                    var result = _nestedCoroutine.MoveNext();
                    Current = _nestedCoroutine.Current;
                    _parent.IsCompleted = !result;

                    return result;
                }

                if (Current == null)
                {
                    Current = _parent.Instruction;
                    return true;
                }

                _parent.IsCompleted = true;
                return false;
            }

            void IEnumerator.Reset()
            {
                Current = null;
                _parent.IsCompleted = false;
            }
        }
    }
}