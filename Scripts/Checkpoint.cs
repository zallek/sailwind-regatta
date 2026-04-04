using UnityEngine;

namespace SailwindRegatta
{
    internal class Checkpoint
    {
        public CheckpointName Name { get; }
        public float Radius { get; }
        public Vector3 Offset { get; }

        public Checkpoint(CheckpointName name, float radius = 30f) : this(name, radius, Vector3.zero) { }

        public Checkpoint(CheckpointName name, float radius, Vector3 offset)
        {
            Name = name;
            Radius = radius;
            Offset = offset;
        }
    }
}
