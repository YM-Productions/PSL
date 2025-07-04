using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Media;

namespace Client_PSL.Json;

public class NormalizingGoubleConverter : JsonConverter<double>
{
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetDouble();

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            writer.WriteNumberValue(0);
        else
            writer.WriteNumberValue(value);
    }
}
