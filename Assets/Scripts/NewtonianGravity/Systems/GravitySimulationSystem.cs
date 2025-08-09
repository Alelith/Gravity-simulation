using NewtonianGravity.Entities;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utilities;

namespace NewtonianGravity.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct SyncGameObjectPositionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (body, transform) in SystemAPI.Query<RefRO<CelestialBody>, RefRW<LocalTransform>>())
                transform.ValueRW.Position = (float3)body.ValueRO.Position;
        }
    }
    
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct PositionSystem : ISystem
    {
        
        public void OnCreate(ref SystemState state)
        {
            // Initialize forces on each body
            foreach (var body in SystemAPI.Query<RefRW<CelestialBody>>())
            {
                // Initialize forces
                double3 forces = double3.zero;

                // Calculate gravitational forces
                foreach (var otherBody in SystemAPI.Query<RefRO<CelestialBody>>())
                {
                    // Calculate distance and direction
                    double distanceSquared = math.lengthsq(otherBody.ValueRO.Position - body.ValueRW.Position);
                    double3 direction = math.normalize(otherBody.ValueRO.Position - body.ValueRW.Position);
                    double distance = math.sqrt(distanceSquared);

                    // Skip if bodies are too close
                    if (distance < 1e-5) continue;

                    // Calculate gravitational force
                    forces += direction * Constants.G * body.ValueRW.Mass * otherBody.ValueRO.Mass / distanceSquared;
                }

                // Update acceleration
                body.ValueRW.Acceleration = forces / body.ValueRW.Mass;
            }
        }
        
        public void OnUpdate(ref SystemState state)
        {
            // Calculate time step
            double deltaTime = SystemAPI.Time.DeltaTime;
            double deltaTimeSquared = deltaTime * deltaTime;

            // Update positions
            foreach (var body in SystemAPI.Query<RefRW<CelestialBody>>())
                body.ValueRW.Position += body.ValueRW.Velocity * deltaTime + 0.5 * body.ValueRW.Acceleration * deltaTimeSquared;
        }
    }

    [BurstCompile]
    [UpdateAfter(typeof(PositionSystem))]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VelocityAccelerationSystem : ISystem
    {
        /*
         * a_n = î * G * M / r^2
         * v_n = v_(n-1) + 1/2 * (a_n + a_(n-1)) * dt
         */
        public void OnUpdate(ref SystemState state)
        {
            // Calculate time step
            double deltaTime = SystemAPI.Time.DeltaTime;

            // Update velocities and accelerations
            foreach (var body in SystemAPI.Query<RefRW<CelestialBody>>())
            {
                double3 forces = double3.zero;

                // Calculate gravitational forces
                foreach (var otherBody in SystemAPI.Query<RefRO<CelestialBody>>())
                {
                    // Calculate distance and direction
                    double distanceSquared = math.lengthsq(otherBody.ValueRO.Position - body.ValueRW.Position);
                    double3 direction = math.normalize(otherBody.ValueRO.Position - body.ValueRW.Position);
                    double distance = math.sqrt(distanceSquared);

                    // Skip if bodies are too close
                    if (distance < 1e-5) continue;

                    // Calculate gravitational force
                    forces += direction * Constants.G * body.ValueRW.Mass * otherBody.ValueRO.Mass / distanceSquared;
                }

                // Update acceleration
                double3 newAcceleration = forces / body.ValueRW.Mass;

                // Update velocity
                body.ValueRW.Velocity += 0.5 * (body.ValueRW.Acceleration + newAcceleration) * deltaTime;
                body.ValueRW.Acceleration = newAcceleration;
            }
        }
    }
}
