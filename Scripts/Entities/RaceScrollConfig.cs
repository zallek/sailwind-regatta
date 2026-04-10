using UnityEngine;

namespace SailwindRegatta
{
    internal class RaceScrollConfig
    {
        public PortName PortName        { get; }
        public int RaceId      { get; }
        public Vector3 Position    { get; }
        public Vector3 EulerAngles { get; }

        public RaceScrollConfig(PortName portName, int raceId, Vector3 position, Vector3 eulerAngles)
        {
            PortName    = portName;
            RaceId      = raceId;
            Position    = position;
            EulerAngles = eulerAngles;
        }
    }
}
