using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace _0_Project.Scripts.Systems
{
    [BurstCompile]
    public partial struct RotateIntoMovementDirection : ISystem
    {
        [ReadOnly] private ComponentLookup<TorsoComponent> _torsoLookup;
        [ReadOnly] private ComponentLookup<PhysicsVelocity> _physicsVelocityLookup;
        [ReadOnly] private ComponentLookup<LocalTransform> _localTransformLookup;

        public void OnCreate(ref SystemState state)
        {
            _torsoLookup = state.GetComponentLookup<TorsoComponent>(isReadOnly: true);
            _physicsVelocityLookup = state.GetComponentLookup<PhysicsVelocity>(isReadOnly: true);
            _localTransformLookup = state.GetComponentLookup<LocalTransform>(isReadOnly: true);
        }

        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            _torsoLookup.Update(ref state);
            _physicsVelocityLookup.Update(ref state);
            _localTransformLookup.Update(ref state);

            var strideLookup = _torsoLookup;

            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var ecbParallel = ecb.AsParallelWriter();

            state.Dependency = new RotateIntoMovementJob
            {
                TorsoLookup = strideLookup,
                PhysicsVelocityLookup = _physicsVelocityLookup,
                LocalTransformLookup = _localTransformLookup,
                DeltaTime = deltaTime,
                ECB = ecbParallel
            }.ScheduleParallel(state.Dependency);

            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [BurstCompile]
    public partial struct RotateIntoMovementJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<TorsoComponent> TorsoLookup;
        [ReadOnly] public ComponentLookup<PhysicsVelocity> PhysicsVelocityLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;

        public EntityCommandBuffer.ParallelWriter ECB;
        public float DeltaTime;

        public void Execute(RefRW<RagdollComponent> ragdoll, Entity entity, [ChunkIndexInQuery] int chunkIndex)
        {
            if (!ragdoll.ValueRO.IsGrounded) return;

            foreach (var muscleEntity in ragdoll.ValueRO.MuscleEntities)
            {
                if (!TorsoLookup.HasComponent(muscleEntity) || !LocalTransformLookup.HasComponent(muscleEntity))
                    continue;

                var torso = TorsoLookup[muscleEntity];
                var direction = math.normalize(ragdoll.ValueRW.MovementDirection);

                if (math.lengthsq(direction) < 0.0001f)
                    continue;

                var physicsVelocity = PhysicsVelocityLookup[muscleEntity];
                var localTransform = LocalTransformLookup[muscleEntity];

                var currentForward = math.rotate(localTransform.Rotation, new float3(0, 0, 1));
                float angleDifference = math.acos(math.clamp(math.dot(currentForward, direction), -1f, 1f));

                if (angleDifference < 0.05f)
                {
                    physicsVelocity.Angular = float3.zero;
                    ECB.SetComponent(chunkIndex, muscleEntity, physicsVelocity);
                    continue;
                }

                var rotationAxis = math.normalize(math.cross(currentForward, direction));
                var angularVelocity = rotationAxis * torso.RotationSpeed * DeltaTime;

                physicsVelocity.Angular = angularVelocity;
                ECB.SetComponent(chunkIndex, muscleEntity, physicsVelocity);
            }
        }
    }
}
