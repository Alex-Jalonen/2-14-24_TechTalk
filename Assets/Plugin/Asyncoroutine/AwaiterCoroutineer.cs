using System.Threading;
using UnityEngine;

namespace AsyncCoroutine
{
    public class AwaiterCoroutineer : MonoBehaviour
    {
        private static AwaiterCoroutineer _instance;

        public static AwaiterCoroutineer Instance
        {
            get
            {
                Install();
                return _instance;
            }
        }

        public SynchronizationContext SynchronizationContext { get; private set; }

        private void Awake()
        {
            if (_instance == null)
                _instance = this;

            DontDestroyOnLoad(_instance);
            SynchronizationContext = SynchronizationContext.Current;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Install()
        {
            if (_instance == null)
                _instance = new GameObject("AwaiterCoroutineer").AddComponent<AwaiterCoroutineer>();
        }

        public void StartAwaiterCoroutine<T>(AwaiterCoroutine<T> awaiterCoroutine)
        {
            StartCoroutine(awaiterCoroutine.Coroutine);
        }

        public void StopAwaiterCoroutine<T>(AwaiterCoroutine<T> awaiterCoroutine)
        {
            StopCoroutine(awaiterCoroutine.Coroutine);
        }
    }
}