using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Media;
using Client_PSL.ViewModels;
using Utils;

namespace Client_PSL.Json;

public class ViewModelBaseConverter : JsonConverter<ViewModelBase>
{
    public override ViewModelBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException();

        string typeName = reader.GetString() ?? "";
        Type? type = Type.GetType(typeName);
        if (type == null || !typeof(ViewModelBase).IsAssignableFrom(type))
            return new ErrorViewModel($"Type '{typeName}' is not a valid ViewModelBase type.");

        return (ViewModelBase)Activator.CreateInstance(type)!;
    }

    public override void Write(Utf8JsonWriter writer, ViewModelBase value, JsonSerializerOptions otpions)
    {
        string typeName = value.GetType().AssemblyQualifiedName ?? throw new JsonException("ViewModelBase type does not have a valid assembly qualified name.");
        writer.WriteStringValue(typeName);
    }
}
