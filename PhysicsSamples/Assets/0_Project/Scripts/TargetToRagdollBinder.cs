using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace _0_Project.Scripts
{
    public class TargetToRagdollBinder : MonoBehaviour
    {
        [SerializeField] private Transform _target;

        private void Update()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // Query entities with MovementTargetComponent
            EntityQuery query = entityManager.CreateEntityQuery(typeof(MovementTargetComponent));

            NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                var movementTarget = entityManager.GetComponentData<MovementTargetComponent>(entity);
                movementTarget.Position = _target.position;
                entityManager.SetComponentData(entity, movementTarget);
            }

            entities.Dispose();
        }
    }
}
