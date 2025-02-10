using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace _0_Project.Scripts
{
    public struct MuscleComponent : IComponentData
    {
        public float StabForce;

        public MuscleComponent(float stabForce)
        {
            StabForce = stabForce;
        }
    }
}
