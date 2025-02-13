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
        public float StabForce;
        public float MotionForce;

        public FootMotionComponent(int index, float stabForce, float motionForce)
        {
            Index = index;
            StabForce = stabForce;
            MotionForce = motionForce;
        }
    }

    public struct StrideComponent : IComponentData
    {
        public float Timer;
        public int CurrentLegIndex;
    }
}
