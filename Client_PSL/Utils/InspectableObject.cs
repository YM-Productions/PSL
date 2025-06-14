using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Utils;

/// <summary>
/// Represents a property that can be inspected at runtime, typically for debugging,
/// UI display, or serialization purposes. Each <see cref="InspectableProperty"/> contains
/// a property name and its corresponding value as strings.
/// </summary>
/// <remarks>
/// This class is useful for scenarios where object properties need to be displayed
/// or logged in a generic way, such as in property grids, inspectors, or diagnostic tools.
/// </remarks>
/// <example>
/// <code>
/// var prop = new InspectableProperty("Id", "42");
/// Console.WriteLine($"{prop.Name}: {prop.Value}");
/// </code>
/// </example>
public class InspectableProperty
{
    /// <summary>
    /// Gets or sets the name of the property.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the value of the property as a string.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InspectableProperty"/> class with the specified name and value.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The value of the property as a string.</param>
    public InspectableProperty(string name, string value)
    {
        Name = name;
        Value = value;
    }
}

/// <summary>
/// Represents a wrapper for any object, providing runtime inspection capabilities.
/// </summary>
/// <remarks>
/// The <c>InspectableObject</c> class allows you to inspect the type and selected properties of an object at runtime.
/// It uses reflection to retrieve fields marked with <see cref="DataMemberAttribute"/> and exposes their names and values
/// as <see cref="InspectableProperty"/> instances. This is useful for debugging, diagnostics, or building property inspectors in UI tools.
/// </remarks>
/// <example>
/// <code>
/// var myObj = new MyClass();
/// var inspectable = new InspectableObject(myObj);
/// Console.WriteLine(inspectable.TypeName);
/// foreach (var prop in inspectable.Properties)
/// {
///     Console.WriteLine($"{prop.Name}: {prop.Value}");
/// }
/// </code>
/// </example>
public class InspectableObject
{
    /// <summary>
    /// The underlying object being inspected.
    /// </summary>
    public object Obj;

    /// <summary>
    /// Gets the type name of the underlying object.
    /// </summary>
    public string TypeName { get => Obj.GetType().Name; }

    /// <summary>
    /// Gets an array of <see cref="InspectableProperty"/> representing the object's inspectable properties.
    /// </summary>
    public InspectableProperty[] Properties { get => GetProperties(); }

    /// <summary>
    /// Initializes a new instance of the <see cref="InspectableObject"/> class with the specified object.
    /// </summary>
    /// <param name="obj">The object to be inspected.</param>
    public InspectableObject(object obj)
    {
        Obj = obj;
    }

    /// <summary>
    /// Uses reflection to retrieve fields marked with <see cref="DataMemberAttribute"/> and returns them as inspectable properties.
    /// </summary>
    /// <returns>An array of <see cref="InspectableProperty"/> containing the names and values of the marked fields.</returns>
    private InspectableProperty[] GetProperties()
    {
        FieldInfo[] fields = Obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        List<InspectableProperty> propertiesList = new();

        foreach (FieldInfo field in fields)
        {
            if (field.GetCustomAttribute<DataMemberAttribute>() is DataMemberAttribute dataMember)
            {
                propertiesList.Add(new InspectableProperty(
                    dataMember.Name ?? field.Name,
                    field.GetValue(Obj)?.ToString() ?? "null"
                ));
            }
        }

        return propertiesList.ToArray();
    }
}
