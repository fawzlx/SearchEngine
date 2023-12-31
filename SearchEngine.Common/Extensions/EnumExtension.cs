﻿using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SearchEngine.Common.Extensions;

public static class EnumExtensions
{
    public static string? ToDisplay(this Enum value, DisplayProperty property = DisplayProperty.Name)
    {
        if (value is null)
            throw new ArgumentNullException($"");

        var attribute = value.GetType().GetField(value.ToString())!
            .GetCustomAttributes<DisplayAttribute>(false).FirstOrDefault();

        if (attribute == null)
            return value.ToString();

        var propValue = attribute.GetType().GetProperty(property.ToString())!.GetValue(attribute, null);
        return propValue?.ToString();
    }
}

public enum DisplayProperty
{
    Name
}