namespace Aihrly.Api.Domain;

public static class EnumParser
{
    // Accepts PascalCase, snake_case, or kebab-case: "culture-fit" → "CultureFit", "reference_check" → "ReferenceCheck"
    public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct, Enum
    {
        var normalized = string.Concat(
            value.Split(['-', '_'])
                 .Select(word => word.Length > 0
                     ? char.ToUpper(word[0]) + word[1..].ToLower()
                     : string.Empty)
        );

        return Enum.TryParse(normalized, ignoreCase: true, out result);
    }

    
    public static TEnum Parse<TEnum>(string value) where TEnum : struct, Enum
    {
        TryParse(value, out TEnum result);
        return result;
    }
}
