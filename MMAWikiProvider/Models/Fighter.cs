﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UFCWikiProvider.Models
{
    public class Fighter
    {
        public string RealName() => Name.Replace("_(fighter)", string.Empty);

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
    }
}