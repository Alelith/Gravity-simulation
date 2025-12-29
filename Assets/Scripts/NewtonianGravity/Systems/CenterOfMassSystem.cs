using NewtonianGravity.Entities;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace NewtonianGravity.Systems
{
    /// <summary>
    /// System that calculates the center of mass of all celestial bodies
    /// and stores it in a singleton entity.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VelocityAccelerationSystem))]
    public partial struct CenterOfMassSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // Create singleton entity to store the system center
            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, new SystemCenter 
            { 
                Position = double3.zero,
                TotalMass = 0 
            });
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            double3 centerOfMass = double3.zero;
            double totalMass = 0.0;

            // Calculate weighted center of mass
            foreach (var body in SystemAPI.Query<RefRO<CelestialBody>>())
            {
                centerOfMass += body.ValueRO.Position * body.ValueRO.Mass;
                totalMass += body.ValueRO.Mass;
            }

            // Avoid division by zero
            if (totalMass > 0)
            {
                centerOfMass /= totalMass;
            }

            // Update the singleton with the calculated center
            SystemAPI.SetSingleton(new SystemCenter 
            { 
                Position = centerOfMass,
                TotalMass = totalMass
            });
        }
    }
}
