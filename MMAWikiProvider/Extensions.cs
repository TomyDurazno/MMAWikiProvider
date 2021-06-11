using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MMAWikiProvider.Extensions
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
    public static class EmbeddedResource
    {
        public static string GetResourceFileAsString(string namespaceAndFileName)
        {
            try
            {
                using (var stream = typeof(EmbeddedResource).GetTypeInfo().Assembly.GetManifestResourceStream(namespaceAndFileName))
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                    return reader.ReadToEnd();
            }

            catch (Exception exception)
            {
                /*ApplicationProvider.WriteToLog<EmbeddedResource>().Error(exception.Message);
                throw new Exception($"Failed to read Embedded Resource {namespaceAndFileName}");*/
                return string.Empty;
            }
        }
    }

}
