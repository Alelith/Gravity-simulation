using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using NewtonianGravity.Entities;

namespace NewtonianGravity.Utilities
{
    /// <summary>
    /// MonoBehaviour proxy that reads the system center from DOTS and updates its transform.
    /// This allows the camera (or other GameObjects) to follow the center of the system.
    /// </summary>
    public class SystemCenterProxy : MonoBehaviour
    {
        void Update()
        {
            // Get the World and EntityManager
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;

            var entityManager = world.EntityManager;

            // Try to get the SystemCenter singleton
            var query = entityManager.CreateEntityQuery(typeof(SystemCenter));
            if (query.IsEmpty)
            {
                query.Dispose();
                return;
            }

            // Read the center position
            var systemCenter = query.GetSingleton<SystemCenter>();
            query.Dispose();

            // Update this GameObject's position to match the system center
            transform.position = new Vector3(
                (float)systemCenter.Position.x,
                (float)systemCenter.Position.y,
                (float)systemCenter.Position.z
            );
        }
    }
}
