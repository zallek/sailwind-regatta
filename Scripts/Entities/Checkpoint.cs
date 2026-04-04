using UnityEngine;

namespace SailwindRegatta
{
    internal class Checkpoint
    {
        public CheckpointName Name { get; }
        public PortName PortName { get; }
        public float Radius { get; }
        public Vector3 Offset { get; }

        public Checkpoint(CheckpointName name, PortName portName, float radius = 30f) : this(name, portName, radius, Vector3.zero) { }

        public Checkpoint(CheckpointName name, PortName portName, float radius, Vector3 offset)
        {
            Name = name;
            PortName = portName;
            Radius = radius;
            Offset = offset;
        }
    }
}
