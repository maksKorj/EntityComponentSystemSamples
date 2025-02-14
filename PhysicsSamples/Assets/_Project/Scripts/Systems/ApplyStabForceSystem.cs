using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace _0_Project.Scripts.Systems
{
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
            if(muscle.ApplyForce == false)
                return;

            velocity.Linear += new float3(0, muscle.StabForce * DeltaTime, 0);
        }
    }
}
