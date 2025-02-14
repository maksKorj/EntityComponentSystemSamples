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
    public partial struct MovementDirectionTargetSystem : ISystem
    {
        [ReadOnly] private ComponentLookup<TorsoComponent> _torsoLookup;
        [ReadOnly] private ComponentLookup<LocalToWorld> _localToWorldLookup;

        public void OnCreate(ref SystemState state)
        {
            _torsoLookup = state.GetComponentLookup<TorsoComponent>(isReadOnly: true);
            _localToWorldLookup = state.GetComponentLookup<LocalToWorld>(isReadOnly: true);
        }

        public void OnUpdate(ref SystemState state)
        {
            _torsoLookup.Update(ref state);
            _localToWorldLookup.Update(ref state);

            state.Dependency = new MovementDirectionTargetJob
            {
                TorsoLookup =  _torsoLookup,
                LocalToWorldLookup = _localToWorldLookup
            }.Schedule(state.Dependency);
            state.Dependency.Complete();
        }
    }

    [BurstCompile]
    public partial struct MovementDirectionTargetJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<TorsoComponent> TorsoLookup;
        [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;

        public EntityCommandBuffer.ParallelWriter ECB;

        public void Execute(RefRW<RagdollComponent> ragdoll, MovementTargetComponent movementTargetComponent, Entity entity, [ChunkIndexInQuery] int chunkIndex)
        {
            if (!ragdoll.ValueRO.IsGrounded)
                return;

            foreach (var muscleEntity in ragdoll.ValueRO.MuscleEntities)
            {
                if (!TorsoLookup.HasComponent(muscleEntity))
                    continue;

                if (!LocalToWorldLookup.HasComponent(muscleEntity))
                    return;

                float3 updatePosition = new float3(movementTargetComponent.Position.x, LocalToWorldLookup[muscleEntity].Position.y, movementTargetComponent.Position.z);
                ragdoll.ValueRW.MovementDirection = math.normalize(updatePosition - LocalToWorldLookup[muscleEntity].Position);
            }
        }
    }
}
