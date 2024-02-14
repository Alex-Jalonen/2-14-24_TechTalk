using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

public class OldFashionFlockCoordinator : MonoBehaviour
{
    [SerializeField]
    private GameObject _boidPrefab;

    [SerializeField]
    private int _boidCount = 10;

    [SerializeField]
    private float _spawnRadius = 3f;

    private List<Transform> _boids = new();

    List<Vector2> _velocities;

    // Start is called before the first frame update
    void Start()
    {
        _velocities = new List<Vector2>(_boidCount);

        for (int i = 0; i < _boidCount; i++)
        {
            var spawnPosition = Random.insideUnitCircle * _spawnRadius / 2;
            var spawnDirection = new Vector2(spawnPosition.y, -spawnPosition.x).normalized;
            var spawnRotation = Quaternion.FromToRotation(Vector2.up, spawnDirection);

            var boidGO = Instantiate(_boidPrefab, spawnPosition, spawnRotation);

            _boids.Add(boidGO.transform);
            _velocities.Add(spawnDirection);
        }
    }

    void Update()
    {
        Profiler.BeginSample("Old Fashion Flock Coordinator");
        CalculateVelocityJob();
        ApplyVelocityJob();
        Profiler.EndSample();
    }

    private void CalculateVelocityJob()
    {
        for (var index = 0; index < _boids.Count; index++)
        {
            var position = _boids[index].position;
            var tangent = new float2(position.y, -position.x);

            _velocities[index] = math.normalize(tangent);
        }
    }

    private void ApplyVelocityJob()
    {
        for (var index = 0; index < _boids.Count; index++)
        {
            var boid = _boids[index];

            boid.position += _velocities[index].xy0() * Time.deltaTime;

            var newRotation = Quaternion.FromToRotation(Vector2.up, _velocities[index]);
            boid.rotation = newRotation;
        }
    }
}
