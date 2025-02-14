using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace _0_Project.Scripts.Systems
{
    [BurstCompile]
    public partial struct HandleRagdollStabForceSystem : ISystem
    {
        private ComponentLookup<MuscleComponent> _muscleLookup;
        [ReadOnly] private ComponentLookup<FootComponent> _footLookup;

        public void OnCreate(ref SystemState state)
        {
            _muscleLookup = state.GetComponentLookup<MuscleComponent>(isReadOnly: false);
            _footLookup = state.GetComponentLookup<FootComponent>(isReadOnly: true);
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();

            _footLookup.Update(ref state);
            _muscleLookup.Update(ref state);

            foreach (var (ragdoll, entity) in SystemAPI.Query<RefRW<RagdollComponent>>().WithEntityAccess())
            {
                var isGrounded = IsGrounded(ragdoll);
                ragdoll.ValueRW.IsGrounded = isGrounded;

                foreach (var muscleEntity in ragdoll.ValueRW.MuscleEntities)
                {
                    if (!_muscleLookup.HasComponent(muscleEntity))
                        continue;

                    var muscle = _muscleLookup[muscleEntity];
                    muscle.ApplyForce = isGrounded;

                    _muscleLookup[muscleEntity] = muscle;
                }
            }
        }

        private bool IsGrounded(RefRW<RagdollComponent> ragdoll)
        {
            foreach (var entity in ragdoll.ValueRW.MuscleEntities)
            {
                if (_footLookup.HasComponent(entity) && _footLookup[entity].IsGrounded)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /*[BurstCompile]
    public partial struct DisableRagdollStabForceJob : IJobEntity
    {
        public ComponentLookup<MuscleComponent> MuscleLookup;

        private void Execute()
        {

        }
    }*/
}
