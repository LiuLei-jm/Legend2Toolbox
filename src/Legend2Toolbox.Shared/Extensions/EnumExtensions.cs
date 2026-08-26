using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Legend2Toolbox.Shared.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        var field = value.GetType()
            .GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DisplayAttribute>();
        return attribute?.Name ?? value.ToString();
    }
}
