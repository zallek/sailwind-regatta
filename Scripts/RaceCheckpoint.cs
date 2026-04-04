using UnityEngine;

namespace SailwindRegatta
{
    internal class RaceCheckpoint
    {
        public string PortName { get; }
        public float Radius { get; }
        public Vector3 Offset { get; }

        public RaceCheckpoint(PortName port, float radius = 800f)
            : this(port.ToPortString(), radius, Vector3.zero) { }

        public RaceCheckpoint(PortName port, float radius, Vector3 offset)
            : this(port.ToPortString(), radius, offset) { }

        public RaceCheckpoint(string portName, float radius = 800f)
            : this(portName, radius, Vector3.zero) { }

        public RaceCheckpoint(string portName, float radius, Vector3 offset)
        {
            PortName = portName;
            Radius = radius;
            Offset = offset;
        }
    }
}
