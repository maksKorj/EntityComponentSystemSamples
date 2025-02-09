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

    [BurstCompile]
    public partial struct ApplyStabForceSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MuscleComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            new ApplyStabForceJob { DeltaTime = deltaTime }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct ApplyStabForceJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref PhysicsVelocity velocity, in MuscleComponent muscle)
        {
            velocity.Linear += new float3(0, muscle.StabForce * DeltaTime, 0);
        }
    }
}
