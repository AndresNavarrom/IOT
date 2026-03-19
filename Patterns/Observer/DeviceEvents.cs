using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMLIoT.Patterns.Observer
{
    internal class DeviceEvents
    {
        public const string Registered = "REGISTERED";
        public const string Removed = "REMOVED";
        public const string TurnedOn = "TURNED_ON";
        public const string TurnedOff = "TURNED_OFF";
        public const string AlarmTriggered = "ALARM_TRIGGERED";
        public const string RecordingStarted = "RECORDING_STARTED";
        public const string StatusChanged = "STATUS_CHANGED";
    }
}
