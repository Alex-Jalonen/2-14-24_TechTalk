using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

public class FlockCoordinator : MonoBehaviour
{
    [SerializeField]
    private GameObject _boidPrefab;

    [SerializeField]
    private int _boidCount = 10;

    [SerializeField]
    private float _spawnRadius = 3f;

    private List<Transform> _boids = new();

    NativeArray<float2> _positions;
    NativeArray<float2> _velocities;

    // Start is called before the first frame update
    void Start()
    {
        _positions = new NativeArray<float2>(_boidCount, Allocator.Persistent);
        _velocities = new NativeArray<float2>(_boidCount, Allocator.Persistent);

        for (int i = 0; i < _boidCount; i++)
        {
            var spawnPosition = Random.insideUnitCircle * _spawnRadius / 2;
            var spawnDirection = new Vector2(spawnPosition.y, -spawnPosition.x).normalized;
            var spawnRotation = Quaternion.FromToRotation(Vector2.up, spawnDirection);

            var boidGO = Instantiate(_boidPrefab, spawnPosition, spawnRotation);

            _boids.Add(boidGO.transform);

            _positions[i] = spawnPosition;
            _velocities[i] = spawnDirection;
        }
    }

    void Update()
    {
        Profiler.BeginSample("Flock Coordinator");
        CalculateVelocityJob();
        ApplyVelocityJob();
        Profiler.EndSample();
    }

    void OnDestroy()
    {
        // Dispose native arrays when the script is destroyed
        _positions.Dispose();
        _velocities.Dispose();
    }

    private void CalculateVelocityJob()
    {
        BoidVelocityCalculationJob job = new BoidVelocityCalculationJob()
        {
            Positions = _positions,
            Velocities = _velocities,
        };
        JobHandle jobHandle = job.Schedule(_boidCount, 256);

        // Wait for the job to complete
        jobHandle.Complete();
    }

    private void ApplyVelocityJob()
    {
        var transforms = new TransformAccessArray(_boidCount);

        for (var index = 0; index < _boids.Count; index++)
        {
            transforms.Add(_boids[index]);
        }

        ApplyVelocity job = new ApplyVelocity()
        {
            Positions = _positions,
            Velocities = _velocities,
            Dt = Time.deltaTime
        };
        JobHandle jobHandle = job.Schedule(transforms);

        // Wait for the job to complete
        jobHandle.Complete();

        transforms.Dispose();
    }

    // Define the job struct with [BurstCompile] attribute for performance
    [BurstCompile]
    struct BoidVelocityCalculationJob : IJobParallelFor
    {
        // Native arrays to store boid positions, velocities, and adjusted velocities
        [ReadOnly] public NativeArray<float2> Positions;
        public NativeArray<float2> Velocities;

        public void Execute(int index)
        {
            var position = Positions[index];
            var tangent = new float2(position.y, -position.x);

            Velocities[index] = math.normalize(tangent);
        }
    }

    [BurstCompile]
    struct ApplyVelocity : IJobParallelForTransform
    {
        public NativeArray<float2> Positions;
        public NativeArray<float2> Velocities;
        public float Dt;

        public void Execute(int index, TransformAccess transform)
        {
            Positions[index] += Velocities[index] * Dt;
            transform.position = new Vector3(Positions[index].x, Positions[index].y, 0);

            var newRotation = Quaternion.FromToRotation(Vector2.up, (Vector2)(Velocities[index]));
            transform.rotation = newRotation;
        }
    }
}
