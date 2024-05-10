using MMAWikiProvider.Common;
using System;
using System.Text.Json.Serialization;

namespace MMAWikiProvider.Models
{
    public class StatsDTO
    {
        public string Name { get; set; }
        public int Fights { get; set; }
        public int UFCFights { get; set; }
        [JsonConverter(typeof(TimeSpanToStringConverter))]
        public TimeSpan? TotalTime { get; set; }
        [JsonConverter(typeof(TimeSpanToStringConverter))]
        public TimeSpan? AverageTime { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int NoContest { get; set; }

        public int Submissions { get; set; }
        public int KOTKO { get; set; }
        public int? WonOrDefendedUFCChampionship { get; set; }
        public int WinLoseRatio { get; set; }

    }
}
