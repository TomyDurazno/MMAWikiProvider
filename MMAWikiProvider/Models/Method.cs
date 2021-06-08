using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UFCWikiProvider.Models
{
    public enum EffectiveMethodType
    {
        KOTKO,
        Submission,
        Decision
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MethodType
    {
        KO,
        TKO,
        Submission,
        Technical_Submission,
        Decision
    }

    public class Method
    {
        public string Description { get; set; }
        public MethodType Type { get; set; }


        public EffectiveMethodType EffectiveType()
        {
            if (IsKO_TKO())
                return EffectiveMethodType.KOTKO;

            if (IsSubmission())
                return EffectiveMethodType.Submission;

            return EffectiveMethodType.Decision;
        }
        public bool IsSubmission()=> Type == MethodType.Submission || Type == MethodType.Technical_Submission;
        public bool IsKO_TKO() => Type == MethodType.KO || Type == MethodType.TKO;

        public Method()
        { 
        
        }

        public Method(string s)
        {
            var auxs = s.Trim();

            var firstBracket = auxs.IndexOf("(");

            if(firstBracket != -1) //without description
            {
                var desc = s.AsSpan().Slice(firstBracket).ToString();

                var stype = s.Replace(desc, string.Empty).Trim();

                if (stype.Contains(" "))
                    auxs = stype.Replace(" ", "_");

                Description = desc.Trim();
            }

            if (Enum.TryParse<MethodType>(auxs, out var type))
                Type = type;

            Description = Description ?? string.Empty;
        }
    }
}
