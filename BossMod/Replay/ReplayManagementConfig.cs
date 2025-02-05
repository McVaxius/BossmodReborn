﻿namespace BossMod
{
    [ConfigDisplay(Name = "Replay settings", Order = 0)]
    public class ReplayManagementConfig : ConfigNode
    {
        [PropertyDisplay("Show replay management UI")]
        public bool ShowUI = false;

        [PropertyDisplay("Auto record replays on duty start")]
        public bool AutoRecord = false;

        [PropertyDisplay("Auto stop replays on duty end")]
        public bool AutoStop = false;

        [PropertyDisplay("Max replays to keep before removal")]
        [PropertyIntSlider(0, 1000, Speed = 1f, Logarithmic = false)]
        public int MaxReplays = 0;

        [PropertyDisplay("Store server packets in the replay")]
        public bool DumpServerPackets = false;

        [PropertyDisplay("Store client packets in the replay")]
        public bool DumpClientPackets = false;

        [PropertyDisplay("Format for recorded logs")]
        public ReplayLogFormat WorldLogFormat = ReplayLogFormat.BinaryCompressed;
    }
}
