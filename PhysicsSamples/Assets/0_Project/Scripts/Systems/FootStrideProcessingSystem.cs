using _0_Project.Scripts;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

[BurstCompile]
public partial struct FootStrideProcessingSystem : ISystem
{
    [ReadOnly] private ComponentLookup<StrideComponent> _strideLookup;
    [ReadOnly] private ComponentLookup<FootMotionComponent> _footMotionLookup;
    private ComponentLookup<PhysicsVelocity> _physicsVelocityLookup;

    public void OnCreate(ref SystemState state)
    {
        _footMotionLookup = state.GetComponentLookup<FootMotionComponent>(isReadOnly: true);
        _strideLookup = state.GetComponentLookup<StrideComponent>(isReadOnly: true);
        _physicsVelocityLookup = state.GetComponentLookup<PhysicsVelocity>(isReadOnly: false);
    }

    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        _strideLookup.Update(ref state);
        _footMotionLookup.Update(ref state);
        _physicsVelocityLookup.Update(ref state);

        var strideLookup = _strideLookup;
        var footMotionLookup = _footMotionLookup;
        var physicsVelocityLookup = _physicsVelocityLookup;

        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        var ecbParallel = ecb.AsParallelWriter();

        state.Dependency = new FootStrideJob
        {
            StrideLookup = strideLookup,
            FootMotionLookup = footMotionLookup,
            PhysicsVelocityLookup = physicsVelocityLookup,
            DeltaTime = deltaTime,
            ECB = ecbParallel
        }.ScheduleParallel(state.Dependency);

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager); 
        ecb.Dispose();
    }

    [BurstCompile]
    private partial struct FootStrideJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<StrideComponent> StrideLookup;
        [ReadOnly] public ComponentLookup<FootMotionComponent> FootMotionLookup;
        [ReadOnly] public ComponentLookup<PhysicsVelocity> PhysicsVelocityLookup;

        public EntityCommandBuffer.ParallelWriter ECB;
        public float DeltaTime;

        public void Execute(RefRW<RagdollComponent> ragdoll, Entity entity, [ChunkIndexInQuery] int chunkIndex)
        {
            if (!ragdoll.ValueRO.IsGrounded) return;

            int index = 0;
            if (StrideLookup.HasComponent(entity))
            {
                index = StrideLookup[entity].CurrentLegIndex;
            }

            foreach (var muscleEntity in ragdoll.ValueRO.MuscleEntities)
            {
                if (!FootMotionLookup.HasComponent(muscleEntity))
                    continue;

                if (!PhysicsVelocityLookup.HasComponent(muscleEntity))
                    continue;

                var footMotion = FootMotionLookup[muscleEntity];
                if (footMotion.Index != index)
                    continue;

                ECB.SetComponent(chunkIndex, muscleEntity, new PhysicsVelocity
                {
                    Linear = PhysicsVelocityLookup[muscleEntity].Linear + new float3(footMotion.Force, footMotion.Force, 0) * DeltaTime,
                    Angular = PhysicsVelocityLookup[muscleEntity].Angular
                });
            }
        }
    }
}
