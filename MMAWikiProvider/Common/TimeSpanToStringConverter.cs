﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MMAWikiProvider.Common
{
    public class TimeSpanToStringConverter : JsonConverter<TimeSpan?>
    {
        public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => TimeSpan.Parse(reader.GetString());
        public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
    }
}
