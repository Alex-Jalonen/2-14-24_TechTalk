using UnityEngine;

/// <summary>
///  took every ounce of will not to just call this script OSHA
/// </summary>
public class WorkerSafety : MonoBehaviour
{
    private Worker[] _workers;

    void Awake()
    {
        _workers = GetComponents<Worker>();

        foreach (var worker in _workers)
        {
            worker.OnEnableAction += HandleEnable;
        }
    }

    private void HandleEnable(Worker enabledWorker)
    {
        foreach (var worker in _workers)
        {
            if (worker == enabledWorker)
                continue;

            worker.enabled = false;
        }
    }
}
