using Unity.Entities;
using Unity.Mathematics;

namespace NewtonianGravity.Entities
{
    /// <summary>
    /// Singleton component that stores the center of mass of the entire system.
    /// </summary>
    public struct SystemCenter : IComponentData
    {
        public double3 Position;
        public double TotalMass;
    }
}
