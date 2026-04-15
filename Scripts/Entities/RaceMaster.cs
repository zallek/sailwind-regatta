using UnityEngine;

namespace SailwindRegatta
{
    internal class RaceMaster
    {
        public PortName PortName { get; }
        public Vector3 Position { get; }
        public Vector3 EulerAngles { get; }

        // Index into Port.ports[] — determines which port's dude is cloned as the character mesh.
        public int Avatar { get; }
        public int RaceId { get; }
        public bool CanStartRace { get; }

        public RaceMaster(PortName portName, Vector3 position, Vector3 eulerAngles, int avatar = 0, int raceId = 0, bool canStartRace = true)
        {
            PortName = portName;
            Position = position;
            EulerAngles = eulerAngles;
            Avatar = avatar;
            RaceId = raceId;
            CanStartRace = canStartRace;
        }
    }
}
