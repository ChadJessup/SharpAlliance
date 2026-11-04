using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpAlliance.Core;

/// <summary>Formats with C printf conversion specifiers (%s %d %02d %c %x %5.2f ...), as the ported code still passes them.</summary>
public static class CPrintf
{
    private static readonly Regex Spec = new(@"%(?<flags>[-+ 0#]*)(?<width>\d+)?(?:\.(?<prec>\d+))?(?:ll|l|h)?(?<conv>[sdiucxXfp%])", RegexOptions.Compiled);

    public static bool LooksLikePrintf(string format) => Spec.IsMatch(format);

    public static string Format(string format, params object?[] args)
    {
        var next = 0;
        return Spec.Replace(format, m =>
        {
            var conv = m.Groups["conv"].Value[0];
            if (conv == '%')
            {
                return "%";
            }

            var arg = next < args.Length ? args[next++] : null;
            var flags = m.Groups["flags"].Value;
            var width = m.Groups["width"].Success ? int.Parse(m.Groups["width"].Value) : 0;
            var prec = m.Groups["prec"].Success ? int.Parse(m.Groups["prec"].Value) : -1;

            string text = conv switch
            {
                's' => arg?.ToString() ?? string.Empty,
                'c' => arg is char ch ? ch.ToString() : ((char)Convert.ToInt32(arg, CultureInfo.InvariantCulture)).ToString(),
                'f' => Convert.ToDouble(arg, CultureInfo.InvariantCulture).ToString("F" + (prec < 0 ? 6 : prec), CultureInfo.InvariantCulture),
                'x' => Convert.ToInt64(arg, CultureInfo.InvariantCulture).ToString("x"),
                'X' => Convert.ToInt64(arg, CultureInfo.InvariantCulture).ToString("X"),
                _ => FormatInteger(arg, flags),
            };

            if (conv == 's' && prec >= 0 && text.Length > prec)
            {
                text = text[..prec];
            }

            if (text.Length >= width)
            {
                return text;
            }

            if (flags.Contains('-'))
            {
                return text.PadRight(width);
            }

            if (flags.Contains('0') && conv != 's' && conv != 'c')
            {
                var sign = text.StartsWith('-') || text.StartsWith('+') ? text[..1] : string.Empty;
                return sign + text[sign.Length..].PadLeft(width - sign.Length, '0');
            }

            return text.PadLeft(width);
        });
    }

    private static string FormatInteger(object? arg, string flags)
    {
        var text = arg is IConvertible ? Convert.ToInt64(arg, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture) : "0";
        if (flags.Contains('+') && !text.StartsWith('-'))
        {
            text = "+" + text;
        }

        return text;
    }
}
