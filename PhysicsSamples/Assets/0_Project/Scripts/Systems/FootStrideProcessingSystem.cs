using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace _0_Project.Scripts.Systems
{
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
            state.Dependency.Complete();

            float deltaTime = SystemAPI.Time.DeltaTime;

            _strideLookup.Update(ref state);
            _footMotionLookup.Update(ref state);
            _physicsVelocityLookup.Update(ref state);

            foreach (var (ragdoll, entity) in SystemAPI.Query<RefRW<RagdollComponent>>().WithEntityAccess())
            {
                if(ragdoll.ValueRW.IsGrounded == false)
                    continue;
                
                int index = 0;
                if (_strideLookup.HasComponent(entity))
                {
                    index = _strideLookup[entity].CurrentLegIndex;
                }

                foreach (var muscleEntity in ragdoll.ValueRW.MuscleEntities)
                {
                    if (!_footMotionLookup.HasComponent(muscleEntity))
                        continue;

                    if(!_physicsVelocityLookup.HasComponent(muscleEntity))
                        continue;

                    var footMotion = _footMotionLookup[muscleEntity];
                    if(footMotion.Index != index)
                        continue;

                    var velocity = _physicsVelocityLookup[muscleEntity];
                    velocity.Linear += new float3(footMotion.Force, footMotion.Force, 0) * deltaTime;
                    _physicsVelocityLookup[muscleEntity] = velocity;
                }
            }
        }
    }

    /*[BurstCompile]
    public partial struct FootStrideProcessingJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref StrideComponent stride)
        {
            stride.Timer -= DeltaTime;
            if(stride.Timer >  0)
                return;

            stride.Timer = 0.35f;
            stride.CurrentLegIndex ^= 1;
        }
    }*/
}
