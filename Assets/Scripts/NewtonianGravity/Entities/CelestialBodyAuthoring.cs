using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Utilities;

namespace NewtonianGravity.Entities
{
    [RequireComponent(typeof(LineRenderer))]
    /// <summary>
    /// Authoring component for celestial bodies.
    /// </summary>
    public class CelestialBodyAuthoring : MonoBehaviour
    {
        [Header("Data")]
        [Tooltip("Mass of the celestial body in kilograms.")]
        public double mass;
        [Tooltip("Initial velocity of the celestial body in meters per second.")]
        public Vector3 initialVelocity;
        
        [Header("Debug")]
        [SerializeField]
        [Tooltip("Number of steps to simulate the trajectory.")]
        int trajectorySteps = 100;
        [SerializeField]
        [Tooltip("Time step for each simulation step.")]
        double timeStep = 0.1;
        
        LineRenderer lineRenderer;

        // Baker for converting the authoring component to a Unity entity
        class CelestialBodyBaker : Baker<CelestialBodyAuthoring>
        {
            public override void Bake(CelestialBodyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CelestialBody
                {
                    Mass = authoring.mass,
                    Position = new(authoring.transform.position),
                    Velocity = new(authoring.initialVelocity),
                    Acceleration = double3.zero
                });
            }
        }

        [ExecuteAlways]
        void Start() => lineRenderer = GetComponent<LineRenderer>();

        [ExecuteAlways]
        void OnDrawGizmos()
        {
            if (!lineRenderer)
                lineRenderer = GetComponent<LineRenderer>();
            
            CalculateTrajectory();
            lineRenderer.startColor = GetComponent<MeshRenderer>().sharedMaterial.color;
            lineRenderer.endColor = ComplementaryColor(GetComponent<MeshRenderer>().sharedMaterial.color);
            lineRenderer.widthMultiplier = 0.2f;
        }
        
        void CalculateTrajectory()
        {
            // Configure LineRenderer
            lineRenderer.positionCount = trajectorySteps;
            Vector3[] positions = new Vector3[trajectorySteps];

            // Clone initial properties of all bodies
            var celestialBodies = FindObjectsOfType<CelestialBodyAuthoring>();
            var simulatedPositions = new double3[celestialBodies.Length];
            var simulatedVelocities = new double3[celestialBodies.Length];
            var masses = new double[celestialBodies.Length];

            for (int i = 0; i < celestialBodies.Length; i++)
            {
                simulatedPositions[i] = new(celestialBodies[i].transform.position);
                simulatedVelocities[i] = new(celestialBodies[i].initialVelocity);
                masses[i] = celestialBodies[i].mass;
            }

            // Current body index
            int currentBodyIndex = Array.IndexOf(celestialBodies, this);

            // Simulate trajectories
            for (int step = 0; step < trajectorySteps; step++)
            {
                double3 totalForce = double3.zero;

                for (int i = 0; i < celestialBodies.Length; i++)
                {
                    // Calculate forces for each body
                    double3 forceOnCurrent = double3.zero;

                    for (int j = 0; j < celestialBodies.Length; j++)
                    {
                        if (i == j) continue;

                        double3 direction = simulatedPositions[j] - simulatedPositions[i];
                        double distanceSquared = math.lengthsq(direction);
                        double distance = math.sqrt(distanceSquared);

                        if (distance < 1e-5) continue;

                        forceOnCurrent += math.normalize(direction) * 
                                          (Constants.G * masses[i] * masses[j] / distanceSquared);
                    }

                    // Update acceleration, velocity, and position
                    double3 acceleration = forceOnCurrent / masses[i];
                    simulatedVelocities[i] += acceleration * timeStep;
                    simulatedPositions[i] += simulatedVelocities[i] * timeStep;
                }

                // Register current body position for trajectory
                positions[step] = new Vector3((float)simulatedPositions[currentBodyIndex].x,
                                              (float)simulatedPositions[currentBodyIndex].y,
                                              (float)simulatedPositions[currentBodyIndex].z);
            }

            // Assign calculated positions to LineRenderer
            lineRenderer.SetPositions(positions);
        }


        Color ComplementaryColor(Color color) => new Color(1 - color.r, 1 - color.g, 1 - color.b);
    }

    public struct CelestialBody : IComponentData
    {
        public double Mass;
        public double3 Position;
        public double3 Velocity;
        public double3 Acceleration;
    }
}