using Unity.Entities;
using Unity.Mathematics;

namespace _0_Project.Scripts
{
    public struct FootComponent : IComponentData
    {
        public bool IsGrounded;
    }

    public struct FootMotionComponent : IComponentData
    {
        public int Index;
        public float Force;

        public FootMotionComponent(int index, float force)
        {
            Index = index;
            Force = force;
        }
    }

    public struct StrideComponent : IComponentData
    {
        public float Timer;
        public int CurrentLegIndex;
    }
}
