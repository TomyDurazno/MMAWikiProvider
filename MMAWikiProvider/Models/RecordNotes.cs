using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MMAWikiProvider.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Bonus
    {        
        FightOfTheNight,
        PerformanceOfTheNight,
        KnockoutOfTheNight,
        SubmissionOfTheNight,
    }


    public class RecordNotes
    {
        #region String Patterns

        public static class Patterns
        {
            public static string lostUFCChampionship = "Lost the UFC (.*) Championship";

            public static string divisionDebut = "(.*) debut.";

            public static string divisionReturn = "Return to ([a-zA-Z]+)";

            public static string catchWeightBout = "Catchweight (.*) bout";

            public static string missedWeight = "([a-zA-Z]+) missed weight";

            public static string testedPositiveFor = @"([a-zA-Z]+) tested positive for ([^\n\r]+\.)";

            public static string ufcChampionship = "UFC (.*) Championship";

            public static string performanceofTheNight = "Performance of the Night";

            public static string fightofTheNight = "Fight of the Night";

            public static string knockOutofTheNight = "Knockout of the Night";

            public static string submissionofTheNight = "Submission of the Night";

            public static string defended = "(Defended|defended)";

            public static string defendedUFCChampionship = "Defended the UFC (.*) Championship";

            public static string defendedAndUnifiedUFCChampionship = "Defended and unified the UFC (.*) Championship";

            public static string retainedUFCChampionship = "Retained the UFC (.*) Championship";

            public static string wonVacantUFCChampionship = "Won the vacant UFC (.*) Championship";

            public static string wonUFCChampionship = "Won the UFC (.*) Championship";

            public static string wonInterimUFCChampionship = "Won the interim UFC (.*) Championship";

            public static string wonAndUnifiedUFCChampionship = "Won and unified the UFC (.*) Championship";

            public static string laterPromotedToUndisputedChampion = "Later promoted to undisputed champion";

            public static string laterPromotedToUFCChampion = "Later promoted to UFC (.*) Champion";
        }

        #endregion

        public KeyValuePair<string, string> MatchesTestedPositiveFor()
        {
            var vals = Regex.Match(Value, Patterns.testedPositiveFor)
                            .Groups
                            .GetValues()
                            .ToArray();
            if (vals.Any())
                return KeyValuePair.Create(vals[1], vals[2]);

            return default;
        }

        public IEnumerable<Bonus> GetBonus()
        {
            //Bonus

            if (Regex.Match(Value, Patterns.performanceofTheNight)
                     .Groups
                     .GetValues()
                     .Any())
                yield return Bonus.PerformanceOfTheNight;

            if (Regex.Match(Value, Patterns.fightofTheNight)
                     .Groups
                     .GetValues()
                     .Any())
                yield return Bonus.FightOfTheNight;

            if (Regex.Match(Value, Patterns.knockOutofTheNight)
                    .Groups
                    .GetValues()
                    .Any())
                yield return Bonus.KnockoutOfTheNight;

            if (Regex.Match(Value, Patterns.submissionofTheNight)
                     .Groups
                     .GetValues()
                     .Any())
                yield return Bonus.SubmissionOfTheNight;
        }

        #region Public Getters

        //public KeyValuePair<string, string> TestedPositiveFor => MatchesTestedPositiveFor();

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? LaterPromotedToUndisputedChampion => Value.Contains(Patterns.laterPromotedToUndisputedChampion) ? true : null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? LaterPromotedToUFCChampion => Value.Matches(Patterns.laterPromotedToUFCChampion).IsNotEmpty() ? true : null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? LostUFCChampionship => Value.Matches(Patterns.lostUFCChampionship).IsNotEmpty() ? true : null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? WonOrDefendedUFCChampionship => (WonUFCChampionship == true) || (DefendedUFCChampionship == true) ? true : null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? WonUFCChampionship => (WonUndisputedUFCChampionship.HasValue || WonInterimUFCChampionship.HasValue) ? true : null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? WonInterimUFCChampionship => Value.Matches(Patterns.wonInterimUFCChampionship).IsNotEmpty() ? true : null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? WonUndisputedUFCChampionship => (Value.Matches(Patterns.wonUFCChampionship).IsNotEmpty()  ||
                                          Value.Matches(Patterns.wonAndUnifiedUFCChampionship).IsNotEmpty() ||
                                          Value.Matches(Patterns.wonVacantUFCChampionship).IsNotEmpty()) ? true : null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? DefendedUFCChampionship => (Value.Matches(Patterns.defendedUFCChampionship).IsNotEmpty() ||
                                               Value.Matches(Patterns.defendedAndUnifiedUFCChampionship).IsNotEmpty() ||
                                               Value.Matches(Patterns.retainedUFCChampionship).IsNotEmpty()) ? true : null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? IsUFCChampionshipBout { get => Value.Matches(Patterns.ufcChampionship).IsNotEmpty() ? true : null; }

        public IEnumerable<Bonus> Bonuses { get => GetBonus(); }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? BothBonus => (Bonuses.Contains(Bonus.FightOfTheNight) &&
                                (Bonuses.Contains(Bonus.PerformanceOfTheNight) || Bonuses.Contains(Bonus.KnockoutOfTheNight) || Bonuses.Contains(Bonus.SubmissionOfTheNight))) ? true : null;
        
        public string Value { get; set; }

        #endregion

        #region Constructors

        public RecordNotes()
        {

        }

        public RecordNotes(string s)
        {
            Value = s.Trim();
        }

        public RecordNotes Clone() => new RecordNotes(Value);

        #endregion
    }
}
