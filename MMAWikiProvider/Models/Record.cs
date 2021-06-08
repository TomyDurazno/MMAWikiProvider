using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UFCWikiProvider.Models
{
    public enum FightResult
    {
        Win,
        Loss,
        Draw,
        NC
    }

    #region RecordRow

    [JsonConverter(typeof(RecordRowConverter))]
    public class RecordRow
    {
        public FightResult Result { get; set; }
        public string Record { get; set; }
        public Fighter Opponent { get; set; }
        public Method Method { get; set; }

        public FightEvent Event { get; set; }

        //January 24, 2021
        public DateTime? Date { get; set; }

        public int? Round { get; set; }

        public TimeSpan? Time { get; set; }

        public Location Location { get; set; }

        public RecordNotes Notes { get; set; }

        public RecordRow()
        {

        }

        public RecordRow(string[] row)
        {
            #region Result

            if (Enum.TryParse<FightResult>(row[0], out var result))
            {
                Result = result;
            }

            #endregion

            #region Record

            Record = row[1].Trim();

            #endregion

            #region Opponent

            Opponent = new Fighter(row[2].Trim());

            #endregion

            #region Method

            Method = new Method(row[3]);

            #endregion

            #region Event

            Event = new FightEvent(row[4]);

            #endregion

            #region Date

            if (DateTime.TryParse(row[5], out var date))
            {
                Date = date;
            }

            #endregion

            #region Round

            if (int.TryParse(row[6], out var n))
            {
                Round = n;
            }

            #endregion

            #region Time

            if (TimeSpan.TryParse(row[7], out var span))
            {
                Time = span;
            }

            #endregion

            #region Location

            Location = new Location(row[8]);

            #endregion

            #region Notes

            Notes = new RecordNotes(row[9]);

            #endregion
        }
    }

    #region JSON Converter

    public class RecordRowConverter : JsonConverter<RecordRow>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return true;
        }

        public override RecordRow Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var startDepth = reader.CurrentDepth;
            var dto = new RecordRow();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == startDepth)
                    return dto;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propName = (reader.GetString() ?? "");
                    reader.Read();

                    switch (propName)
                    {
                        case nameof(dto.Result):
                            dto.Result = Enum.Parse<FightResult>(reader.GetInt32().ToString());
                            break;

                        case nameof(dto.Record):
                            if (reader.TokenType != JsonTokenType.Null)
                            {
                                dto.Record = reader.GetString();
                            }
                            break;
                        case nameof(dto.Opponent):
                            if (JsonDocument.TryParseValue(ref reader, out JsonDocument jsonDoc))
                                dto.Opponent = JsonSerializer.Deserialize<Fighter>(jsonDoc.RootElement.GetRawText());
                            break;

                        case nameof(dto.Method):
                            if (JsonDocument.TryParseValue(ref reader, out JsonDocument jsonDoc2))
                            {
                                var text = jsonDoc2.RootElement.GetRawText();                                
                                dto.Method = JsonSerializer.Deserialize<Method>(text);
                            }
                            break;

                        case nameof(dto.Event):
                            if (JsonDocument.TryParseValue(ref reader, out JsonDocument jsonDoc3))
                                dto.Event = JsonSerializer.Deserialize<FightEvent>(jsonDoc3.RootElement.GetRawText());
                            break;

                        case nameof(dto.Date):
                            if (reader.TokenType != JsonTokenType.Null)
                            {
                                dto.Date = reader.GetDateTime();
                            }
                            break;

                        case nameof(dto.Round):

                            if (reader.TokenType != JsonTokenType.Null)
                            {
                                dto.Round = reader.GetInt32();
                            }
                            break;

                        case nameof(dto.Time):
                            if (JsonDocument.TryParseValue(ref reader, out JsonDocument jsonDoc6))
                            {
                                if(jsonDoc6.RootElement.ValueKind != JsonValueKind.Null)
                                {
                                    var ticks = jsonDoc6.RootElement.GetProperty("Ticks");

                                    dto.Time = TimeSpan.FromTicks(ticks.GetInt64());
                                }
                            }
                            break;
                        case nameof(dto.Location):
                            if (JsonDocument.TryParseValue(ref reader, out JsonDocument jsonDoc4))
                                dto.Location = JsonSerializer.Deserialize<Location>(jsonDoc4.RootElement.GetRawText());
                            break;
                        case nameof(dto.Notes):
                            if (JsonDocument.TryParseValue(ref reader, out JsonDocument jsonDoc5))
                                dto.Notes = JsonSerializer.Deserialize<RecordNotes>(jsonDoc5.RootElement.GetRawText());
                            break;

                    }
                }           
            }
            
            return dto;
        }

        public override void Write(Utf8JsonWriter writer, RecordRow value, JsonSerializerOptions options)
        {
            var dic = new ExpandoObject();

            foreach (var prop in value.GetType().GetProperties())
                dic.TryAdd(prop.Name, prop.GetValue(value));         

            JsonSerializer.Serialize(writer, dic);
        }
    }
    #endregion

    #endregion

    #region Extensions

    public static class RecordExtensions
    {
        public static int Wins(this List<RecordRow> record) => record.Count(r => r.Result == FightResult.Win);

        public static int Losses(this List<RecordRow> record) => record.Count(r => r.Result == FightResult.Loss);

        public static int Draws(this List<RecordRow> record) => record.Count(r => r.Result == FightResult.Draw);

        public static int Submissions(this List<RecordRow> record) => record.Count(r => r.Method.IsSubmission());

        public static int KOTKO(this List<RecordRow> record) => record.Count(r => r.Method.IsKO_TKO());

        public static int WonOrDefendedUFCChampionship(this List<RecordRow> record) => record.Count(r => r.Notes.DefendedUFCChampionship) + record.Count(r => r.Notes.WonUFCChampionship);
    }

    #endregion
}
