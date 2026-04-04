using UnityEngine;

namespace SailwindRegatta
{
    internal class RaceMasterConfig
    {
        public CheckpointName Port { get; }
        public Vector3 Position { get; }
        public Vector3 EulerAngles { get; }

        // Index into Port.ports[] — determines which port's dude is cloned as the character mesh.
        public int Avatar { get; }

        public RaceMasterConfig(CheckpointName port, Vector3 position, Vector3 eulerAngles, int avatar = 0)
        {
            Port = port;
            Position = position;
            EulerAngles = eulerAngles;
            Avatar = avatar;
        }
    }
}
