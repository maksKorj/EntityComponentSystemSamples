using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _0_Project.Scripts
{
    public struct RagdollComponent : IComponentData
    {
        public bool IsGrounded;
        public float3 MovementDirection;

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

    public struct TorsoComponent : IComponentData
    {
        public float RotationSpeed;
    }

    public struct MovementTargetComponent : IComponentData
    {
        public float3 Position;
    }
}
