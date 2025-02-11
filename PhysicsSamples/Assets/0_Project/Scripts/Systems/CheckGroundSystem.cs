using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace _0_Project.Scripts.Systems
{
    [BurstCompile]
    public partial struct CheckGroundSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<FootComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

            new CheckGroundJob
            {
                CollisionWorld = collisionWorld
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct CheckGroundJob : IJobEntity
    {
        [ReadOnly] public CollisionWorld CollisionWorld;

        private void Execute(ref FootComponent foot, in LocalTransform transform)
        {
            float3 start = transform.Position;
            float3 direction = math.down();
            float maxDistance = 0.2f;

            var raycastInput = new RaycastInput
            {
                Start = start,
                End = start + direction * maxDistance,
                Filter = new CollisionFilter()
                {
                    BelongsTo = 1 << 11,
                    CollidesWith = 1 << 10
                }
            };

            Debug.DrawLine(start, start + direction * maxDistance, Color.green, 0.1f);
            foot.IsGrounded = CollisionWorld.CastRay(raycastInput, out RaycastHit _);
        }
    }
}
