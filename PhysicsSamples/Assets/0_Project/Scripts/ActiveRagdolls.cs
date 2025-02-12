using Unity.Collections;
using Unity.Entities;

namespace _0_Project.Scripts
{
    public struct RagdollComponent : IComponentData
    {
        public bool IsGrounded;
        public FixedList128Bytes<Entity> MuscleEntities;
    }

    public struct MuscleComponent : IComponentData
    {
        public bool ApplyForce;
        public float StabForce;

        public MuscleComponent(float stabForce, bool applyForce = true)
        {
            StabForce = stabForce;
            ApplyForce = applyForce;
        }
    }
}
