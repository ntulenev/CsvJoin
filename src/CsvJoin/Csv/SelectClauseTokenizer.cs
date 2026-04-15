using System.Text;

namespace CsvJoin.Csv;

/// <summary>
/// Splits a SELECT clause into individual projection expressions.
/// </summary>
internal static class SelectClauseTokenizer
{
    /// <summary>
    /// Splits a SELECT clause while respecting brackets, string literals, and function calls.
    /// </summary>
    /// <param name="input">The raw SELECT clause content.</param>
    /// <returns>The parsed projection expressions.</returns>
    public static IReadOnlyList<string> Split(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var parts = new List<string>();
        var current = new StringBuilder();
        var insideBracket = false;
        var insideString = false;
        var parenthesisDepth = 0;

        for (var index = 0; index < input.Length; index++)
        {
            var character = input[index];
            if (character == '[')
            {
                insideBracket = true;
            }
            else if (character == ']')
            {
                insideBracket = false;
            }
            else if (character == '\'')
            {
                current.Append(character);
                if (insideString && index + 1 < input.Length && input[index + 1] == '\'')
                {
                    current.Append(input[index + 1]);
                    index++;
                    continue;
                }

                insideString = !insideString;
                continue;
            }
            else if (!insideString && character == '(')
            {
                parenthesisDepth++;
            }
            else if (!insideString && character == ')')
            {
                parenthesisDepth--;
            }
            else if (character == ',' && !insideBracket && !insideString && parenthesisDepth == 0)
            {
                AddPart(parts, current);
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        AddPart(parts, current);
        return parts;
    }

    private static void AddPart(List<string> parts, StringBuilder current)
    {
        var value = current.ToString().Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new FormatException("SELECT clause contains an empty column expression.");
        }

        parts.Add(value);
    }
}
