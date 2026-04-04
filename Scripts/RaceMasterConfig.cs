using UnityEngine;

namespace SailwindRegatta
{
    internal class RaceMasterConfig
    {
        public CheckpointName Port { get; }
        public Vector3 Position { get; }
        public Vector3 EulerAngles { get; }

        public RaceMasterConfig(CheckpointName port, Vector3 position, Vector3 eulerAngles)
        {
            Port = port;
            Position = position;
            EulerAngles = eulerAngles;
        }
    }
}
