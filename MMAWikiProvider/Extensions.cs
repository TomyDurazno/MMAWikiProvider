﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MMAWikiProvider
{
    public static class Extensions
    {
        public static IEnumerable<string> GetValues(this GroupCollection collection)
        {
            foreach (Group group in collection)
            {
                if(group.Success)
                    yield return group.Value;
            }
        }

        public static bool IsNotEmpty(this string s) => !string.IsNullOrEmpty(s);

        public static string Matches(this string value, string pattern)
        {
            var vals = Regex.Match(value, pattern)
                .Groups
                .GetValues()
                .ToList();

            if (vals.Count() > 1)
                return vals[1];
            else
                return string.Empty;
        }

        public static TimeSpan Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, TimeSpan> func)
        {
            return new TimeSpan(source.Sum(item => func(item).Ticks));
        }
    }
}
