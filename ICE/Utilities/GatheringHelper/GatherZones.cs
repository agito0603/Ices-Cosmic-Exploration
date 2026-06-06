using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Utilities.GatheringHelper;

public static unsafe partial class GatheringUtil
{
    public class MapInfo
    {
        public List<uint> MissionIds { get; set; } = new();
        public uint TerritoryId { get; set; } = 0;
        public uint X { get; set; } = 0;
        public uint Y { get; set; } = 0;
        public uint Radius { get; set; } = 0;
        public uint IconId { get; set; } = 0;
    }

    public static Dictionary<uint, MapInfo> GatherSpots = new();
    public static Dictionary<uint, MapInfo> CriticalSpots = new();
}
