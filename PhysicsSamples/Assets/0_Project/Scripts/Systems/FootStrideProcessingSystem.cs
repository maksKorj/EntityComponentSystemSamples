using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace _0_Project.Scripts.Systems
{
    [BurstCompile][UpdateAfter(typeof(ApplyStabForceSystem))]
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
                if (!ragdoll.ValueRO.IsGrounded || math.all(ragdoll.ValueRW.MovementDirection == float3.zero))
                    return;

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
                    float3 force = new float3(0, 0, 0);

                    if (footMotion.Index != index)
                    {
                        //if (footMotion.StabForce > 100)
                            force.y = -500 * DeltaTime;
                    }
                    else
                    {
                        var direction = ragdoll.ValueRW.MovementDirection;
                        force = new float3(direction.x * footMotion.MotionForce,
                            footMotion.StabForce,
                            direction.z * footMotion.MotionForce) * DeltaTime;
                    }

                    ECB.SetComponent(chunkIndex, muscleEntity, new PhysicsVelocity
                    {
                        Linear = force,
                        Angular = PhysicsVelocityLookup[muscleEntity].Angular
                    });
                }
            }
        }
    }
}


