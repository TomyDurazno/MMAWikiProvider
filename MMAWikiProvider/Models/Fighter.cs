using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace MMAWikiProvider.Models
{
    public class Fighter
    {
        //Wikipedia suffix name, can have '_(fighter)' 
        public string Name { get; set; }

        public List<RecordRow> Record { get; set; }        

        public Fighter()
        {

        }

        public Fighter(string s)
        {
            Name = s;
            Record = new List<RecordRow>();
        }

        Fighter(Fighter f)
        {
            Name = f.Name;
            Record = f.Record.Select(r => r.Clone()).ToList();
        }

        public bool EqualsName(string name) => KeyName().Equals(Replace(name));


        public bool ContainsName(string name) => KeyName().Contains(Replace(name));

        string Replace(string x) => x.Replace("_(fighter)", string.Empty)
                                     .Replace("_", " ")
                                     .ToLower();

        public string KeyName() => Replace(Name);

        public override bool Equals(object obj)
        {
            var sobj = obj as Fighter;

            if (ReferenceEquals(this, obj))
                return true;

            return KeyName() == sobj.KeyName();
        }

        public override int GetHashCode() => HashCode.Combine(KeyName());

        public static Fighter Parse(string name, IEnumerable<IEnumerable<string>> rows)
        {
            var fighter = new Fighter();

            fighter.Name = name;

            RecordRow previousRow = default;

            RecordRow MakeRecordRow(string[] arr)
            {
                RecordRow row = default;

                if (arr.Count() == 7)
                {
                    var aux = new[]
                    {
                        arr[0], //Win
                        arr[1], //2-0
                        arr[2],//Magomed Magomedov
                        arr[3],//Decision
                        previousRow.Event.Description,
                        previousRow.Date.ToString(),
                        arr[4],//Round
                        arr[5],//Minute
                        previousRow.Location.ToString(),
                        arr[6]//Notes 
                    };

                    arr = aux;
                }

                if(arr.Count() == 9)
                {
                    // -> Can be missing, ex: Colby Covington
                    arr = arr.Append(string.Empty).ToArray();
                }

                row = new RecordRow(arr);

                previousRow = row;                              

                return row;
            }

            fighter.Record = rows.Select(r => MakeRecordRow(r.ToArray()))
                                 .ToList();

            return fighter;
        }

        public Fighter Clone() => new Fighter(this);

        #region Grouppers

        public IDictionary<string, object> BonusGroup()
        {
            var exp = new ExpandoObject() as IDictionary<string, object>;

            var grouped =  Record.Where(r => r.Notes.Bonuses.Any())
                                 .GroupBy(r => r.Method.Type)
                                 .OrderByDescending(g => g.Count());

            exp.Add("Total", grouped.Sum(g => g.Count()));

            foreach (var g in grouped)
                exp.Add(g.Key.ToString(), g.Count());

            return exp;
        }

        #endregion
    }
}
