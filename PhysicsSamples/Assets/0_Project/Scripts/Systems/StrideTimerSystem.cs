using Unity.Burst;
using Unity.Entities;
using Unity.Physics;

namespace _0_Project.Scripts.Systems
{
    [BurstCompile]
    public partial struct StrideTimerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<StrideComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            var jobHandle = new StrideTimerJob
            {
                DeltaTime = deltaTime
            }.ScheduleParallel(state.Dependency);

            state.Dependency = jobHandle;
        }
    }

    [BurstCompile]
    public partial struct StrideTimerJob : IJobEntity
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
    }
}
