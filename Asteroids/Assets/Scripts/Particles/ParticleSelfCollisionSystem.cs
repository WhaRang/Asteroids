using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSelfCollisionSystem : MonoBehaviour
{
    private const int DEFAULT_RANDOM_SEED = 1337;
    private const int SCORE_PER_MISSLE = 1;

    private const float POSITION_OFFSET = 0.5f;
    private const float RESPAWN_TIME = 1.0f;
    private const float MAX_SPEED = 0.5f;
    private const float RADIUS_SCALE = 0.35f;

    [SerializeField] private ShipMissleObjectPool newMissle;
    [SerializeField] private GameStateController gameStateController;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private int gridSize;

    private int heightRestriction;
    private int widthRestriction;

    private ParticleSystem partSystem;
    private ParticleSystem.EmitParams emitParams;

    private CacheJob cacheJob;
    private SortJob sortJob;
    private CollisionJob collisionJob;

    private NativeList<float> respawnCounters;
    private NativeArray<SortKey> sortKeys;
    private NativeQueue<ParticleCollision> collisions;

    private ParticleSystem.Particle[] tempParticles;
    private System.Random random;

    #region Asistive Structs

    struct ParticleCollision
    {
        public int Index1;
        public int Index2;
    }

    struct SortKey : IComparable<SortKey>
    {
        public float Key;
        public int Index;

        public int CompareTo(SortKey other)
        {
            return Key.CompareTo(other.Key);
        }
    }

    #endregion

    #region Job Sctructs

    [BurstCompile]
    struct CacheJob : IJobParticleSystemParallelForBatch
    {
        [WriteOnly]
        public NativeArray<SortKey> sortKeys;

        public void Execute(ParticleSystemJobData particles, int startIndex, int count)
        {
            ParticleSystemNativeArray3 srcPositions = particles.positions;

            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
                sortKeys[i] = new SortKey { Key = srcPositions[i].x, Index = i };
        }
    }

    [BurstCompile]
    struct SortJob : IJobParticleSystem
    {
        public NativeArray<SortKey> sortKeys;

        public void Execute(ParticleSystemJobData particles)
        {
            new NativeSlice<SortKey>(sortKeys, 0, particles.count).Sort();
        }
    }

    [BurstCompile]
    struct CollisionJob : IJobParticleSystemParallelForBatch
    {
        [ReadOnly]
        public NativeArray<SortKey> sortKeys;

        [WriteOnly]
        public NativeQueue<ParticleCollision>.ParallelWriter collisions;

        public float radiusScale;
        public float maxDiameter;

        public void Execute(ParticleSystemJobData particles, int startIndex, int count)
        {
            ParticleSystemNativeArray3 positions = particles.positions;
            NativeArray<float> sizes = particles.sizes.x;

            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                int particleIndex = sortKeys[i].Index;
                float3 particlePosition = positions[particleIndex];
                float particleSize = sizes[particleIndex] * 0.5f * radiusScale;

                int i2 = i + 1;
                while (i2 < particles.count)
                {
                    int particleIndex2 = sortKeys[i2++].Index;
                    float3 particlePosition2 = positions[particleIndex2];
                    float particleSize2 = sizes[particleIndex2] * 0.5f * radiusScale;
                    float particleSizeSum = particleSize + particleSize2;

                    if (math.distancesq(particlePosition, particlePosition2) < particleSizeSum * particleSizeSum)
                    {
                        collisions.Enqueue(new ParticleCollision
                        {
                            Index1 = particleIndex,
                            Index2 = particleIndex2
                        });
                    }
                    else if (particlePosition2.x - particlePosition.x > maxDiameter)
                    {
                        break;
                    }
                }
            }
        }
    }

    #endregion

    #region Unity Functions

    private void Start()
    {
        partSystem = GetComponent<ParticleSystem>();

        tempParticles = new ParticleSystem.Particle[partSystem.main.maxParticles];
        emitParams = new ParticleSystem.EmitParams();

        emitParams.ResetAngularVelocity();
        emitParams.ResetAxisOfRotation();
        emitParams.ResetMeshIndex();
        emitParams.ResetPosition();
        emitParams.ResetRandomSeed();
        emitParams.ResetRotation();
        emitParams.ResetStartColor();
        emitParams.ResetStartLifetime();
        emitParams.ResetStartSize();
        emitParams.ResetVelocity();

        random = new System.Random(DEFAULT_RANDOM_SEED);
        SetupStartingParticles();

        ParticleSystem.MainModule main = partSystem.main;
        int maxParticleCount = main.maxParticles;

        heightRestriction = (int)Mathf.Round(cameraController.Height) + 1;
        widthRestriction = (int)Mathf.Round(cameraController.Width) + 1;

        respawnCounters = new NativeList<float>(Allocator.Persistent);
        sortKeys = new NativeArray<SortKey>(maxParticleCount, Allocator.Persistent);
        collisions = new NativeQueue<ParticleCollision>(Allocator.Persistent);

        cacheJob = new CacheJob
        {
            sortKeys = sortKeys,
        };

        sortJob = new SortJob
        {
            sortKeys = sortKeys
        };

        collisionJob = new CollisionJob
        {
            sortKeys = sortKeys,
            collisions = collisions.AsParallelWriter(),
            radiusScale = RADIUS_SCALE,
            maxDiameter = main.startSize.constantMax * RADIUS_SCALE
        };
    }

    private void Update()
    {
        for (int i = 0; i < respawnCounters.Length; i++)
        {
            respawnCounters[i] -= Time.deltaTime;
        }
    }

    private void OnParticleUpdateJobScheduled()
    {
        if (partSystem == null)
            return;

        Unity.Jobs.JobHandle handle = cacheJob.ScheduleBatch(partSystem, 2048);
        handle = sortJob.Schedule(partSystem, handle);
        handle = collisionJob.ScheduleBatch(partSystem, 1024, handle);
        handle.Complete();

        ClearColidedParticles();
        RespawnParticles();
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.TryGetComponent(out ShipMissle missle))
        {
            scoreManager.AddScore(SCORE_PER_MISSLE);
            newMissle.ReturnToPool(missle);
        }

        if (other.TryGetComponent(out ShipController ship))
        {
            gameStateController.StopGame();
        }

        respawnCounters.Add(RESPAWN_TIME);
    }

    private void OnDisable()
    {
        sortKeys.Dispose();
        collisions.Dispose();
        respawnCounters.Dispose();
    }

    #endregion

    #region Public Methods

    public void Restart()
    {
        respawnCounters.Clear();
        collisions.Clear();
        partSystem.Clear();

        random = new System.Random(DEFAULT_RANDOM_SEED);

        SetupStartingParticles();
    }

    #endregion

    #region Private Methods

    private void SetupStartingParticles()
    {
        ParticleSystem.MainModule main = partSystem.main;
        main.maxParticles = gridSize * gridSize;

        Vector3 newPos = Vector3.zero;
        Vector3 newVelocity = Vector3.zero;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                newPos.x = i - gridSize / 2 + cameraController.transform.position.x - POSITION_OFFSET;
                newPos.z = j - gridSize / 2 + cameraController.transform.position.y - POSITION_OFFSET;

                emitParams.position = newPos;

                newVelocity.x = (float)random.NextDouble() * MAX_SPEED * 2 - MAX_SPEED;
                newVelocity.z = (float)random.NextDouble() * MAX_SPEED * 2 - MAX_SPEED;

                emitParams.velocity = newVelocity;

                partSystem.Emit(emitParams, 1);
            }
        }
    }

    private void ClearColidedParticles()
    {
        int numParticlesAlive = partSystem.GetParticles(tempParticles);
        int collisionsCount = collisions.Count;

        for (int i = 0; i < collisionsCount; i++)
        {
            ParticleCollision collision = collisions.Dequeue();

            tempParticles[collision.Index1].remainingLifetime = 0.0f;
            tempParticles[collision.Index2].remainingLifetime = 0.0f;

            respawnCounters.Add(RESPAWN_TIME);
            respawnCounters.Add(RESPAWN_TIME);
        }

        partSystem.SetParticles(tempParticles, numParticlesAlive);
    }

    private void RespawnParticles()
    {
        int index = 0;

        Vector3 newPos = Vector3.zero;
        Vector3 newVelocity = Vector3.zero;

        while (index < respawnCounters.Length)
        {
            if (respawnCounters[index] <= 0.0f)
            {
                respawnCounters.RemoveAt(index);

                int randomGridX = random.Next(0, gridSize);
                int randomGridZ = random.Next(0, gridSize);

                if (randomGridX >= gridSize / 2 - widthRestriction / 2 &&
                    randomGridX <= gridSize / 2 + widthRestriction / 2)
                    randomGridX -= widthRestriction;

                if (randomGridZ >= gridSize / 2 - heightRestriction / 2 &&
                    randomGridZ <= gridSize / 2 + heightRestriction / 2)
                    randomGridZ -= heightRestriction;

                newPos.x = randomGridX - gridSize / 2 + cameraController.transform.position.x - POSITION_OFFSET;
                newPos.z = randomGridZ - gridSize / 2 + cameraController.transform.position.y - POSITION_OFFSET;

                emitParams.position = newPos;

                newVelocity.x = (float)random.NextDouble() * MAX_SPEED * 2 - MAX_SPEED;
                newVelocity.z = (float)random.NextDouble() * MAX_SPEED * 2 - MAX_SPEED;

                emitParams.velocity = newVelocity;

                partSystem.Emit(emitParams, 1);
            }
            else
            {
                index++;
            }
        }
    }
    #endregion
}