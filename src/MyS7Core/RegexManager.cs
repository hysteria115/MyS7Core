using System.Text.RegularExpressions;

namespace MyS7Core;

/// <summary>
/// Provides compiled regular expressions for validating and parsing Siemens S7 PLC address formats.
/// Uses source generators for optimal performance and supports various PLC data types.
/// </summary>
public static partial class RegexManager
{
    /// <summary>
    /// Validates DBX (Data Block Bit) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DBX10.0" through "DB999.DBX999.7"</returns>
    /// <example>Valid formats: "DB1.DBX0.0", "DB100.DBX255.7"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.DBX\d+\.[0-7]$", RegexOptions.Compiled)]
    public static partial Regex DbxAddressRegex();
#else
    private static readonly Regex DbxAddressRegexInstance = new Regex(@"^DB\d+\.DBX\d+\.[0-7]$", RegexOptions.Compiled);
    public static Regex DbxAddressRegex() => DbxAddressRegexInstance;
#endif

    /// <summary>
    /// Validates DBW (Data Block Word - 16-bit signed integer) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DBW10"</returns>
    /// <example>Valid formats: "DB1.DBW0", "DB100.DBW254"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.DBW\d+$", RegexOptions.Compiled)]
    public static partial Regex DbwAddressRegex();
#else
    private static readonly Regex DbwAddressRegexInstance = new Regex(@"^DB\d+\.DBW\d+$", RegexOptions.Compiled);
    public static Regex DbwAddressRegex() => DbwAddressRegexInstance;
#endif

    /// <summary>
    /// Validates UDBW (Unsigned Data Block Word - 16-bit unsigned integer) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.UDBW10"</returns>
    /// <example>Valid formats: "DB1.UDBW0", "DB100.UDBW254"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.UDBW\d+$", RegexOptions.Compiled)]
    public static partial Regex UdbwAddressRegex();
#else
    private static readonly Regex UdbwAddressRegexInstance = new Regex(@"^DB\d+\.UDBW\d+$", RegexOptions.Compiled);
    public static Regex UdbwAddressRegex() => UdbwAddressRegexInstance;
#endif

    /// <summary>
    /// Validates SIT (Small Integer - 8-bit signed integer) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.SIT10"</returns>
    /// <example>Valid formats: "DB1.SIT0", "DB100.SIT255"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.SIT\d+$", RegexOptions.Compiled)]
    public static partial Regex SitAddressRegex();
#else
    private static readonly Regex SitAddressRegexInstance = new Regex(@"^DB\d+\.SIT\d+$", RegexOptions.Compiled);
    public static Regex SitAddressRegex() => SitAddressRegexInstance;
#endif

    /// <summary>
    /// Validates USIT (Unsigned Small Integer - 8-bit unsigned integer) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.USIT10"</returns>
    /// <example>Valid formats: "DB1.USIT0", "DB100.USIT255"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.USIT\d+$", RegexOptions.Compiled)]
    public static partial Regex UsitAddressRegex();
#else
    private static readonly Regex UsitAddressRegexInstance = new Regex(@"^DB\d+\.USIT\d+$", RegexOptions.Compiled);
    public static Regex UsitAddressRegex() => UsitAddressRegexInstance;
#endif

    /// <summary>
    /// Validates DIT (Double Integer - 32-bit signed integer) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DIT10"</returns>
    /// <example>Valid formats: "DB1.DIT0", "DB100.DIT252"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.DIT\d+$", RegexOptions.Compiled)]
    public static partial Regex DitAddressRegex();
#else
    private static readonly Regex DitAddressRegexInstance = new Regex(@"^DB\d+\.DIT\d+$", RegexOptions.Compiled);
    public static Regex DitAddressRegex() => DitAddressRegexInstance;
#endif

    /// <summary>
    /// Validates UDIT (Unsigned Double Integer - 32-bit unsigned integer) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.UDIT10"</returns>
    /// <example>Valid formats: "DB1.UDIT0", "DB100.UDIT252"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.UDIT\d+$", RegexOptions.Compiled)]
    public static partial Regex UditAddressRegex();
#else
    private static readonly Regex UditAddressRegexInstance = new Regex(@"^DB\d+\.UDIT\d+$", RegexOptions.Compiled);
    public static Regex UditAddressRegex() => UditAddressRegexInstance;
#endif

    /// <summary>
    /// Validates DBB (Data Block Byte) addresses with additional byte offset.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DBB10.1"</returns>
    /// <example>Valid formats: "DB1.DBB0.0", "DB100.DBB255.255"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.DBB\d+\.\d+$", RegexOptions.Compiled)]
    public static partial Regex DbbAddressRegex();
#else
    private static readonly Regex DbbAddressRegexInstance = new Regex(@"^DB\d+\.DBB\d+\.\d+$", RegexOptions.Compiled);
    public static Regex DbbAddressRegex() => DbbAddressRegexInstance;
#endif

    /// <summary>
    /// Validates DBS (Data Block String) addresses with string length parameter.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DBS10.20"</returns>
    /// <example>Valid formats: "DB1.DBS0.1", "DB100.DBS252.254"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.DBS\d+\.\d+$", RegexOptions.Compiled)]
    public static partial Regex DbsAddressRegex();
#else
    private static readonly Regex DbsAddressRegexInstance = new Regex(@"^DB\d+\.DBS\d+\.\d+$", RegexOptions.Compiled);
    public static Regex DbsAddressRegex() => DbsAddressRegexInstance;
#endif

    /// <summary>
    /// Validates DBF (Data Block Float - 32-bit floating point) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DBF10"</returns>
    /// <example>Valid formats: "DB1.DBF0", "DB100.DBF252"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.DBF\d+$", RegexOptions.Compiled)]
    public static partial Regex DbfAddressRegex();
#else
    private static readonly Regex DbfAddressRegexInstance = new Regex(@"^DB\d+\.DBF\d+$", RegexOptions.Compiled);
    public static Regex DbfAddressRegex() => DbfAddressRegexInstance;
#endif

    /// <summary>
    /// Validates DDB (Double Data Block - 64-bit floating point) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DDB10"</returns>
    /// <example>Valid formats: "DB1.DDB0", "DB100.DDB248"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.DDB\d+$", RegexOptions.Compiled)]
    public static partial Regex DdbAddressRegex();
#else
    private static readonly Regex DdbAddressRegexInstance = new Regex(@"^DB\d+\.DDB\d+$", RegexOptions.Compiled);
    public static Regex DdbAddressRegex() => DdbAddressRegexInstance;
#endif

    /// <summary>
    /// Validates DDWD (Data Block Double Word Hex - 32-bit DWord displayed as hexadecimal) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DDWD10"</returns>
    /// <example>Valid formats: "DB1.DDWD0", "DB100.DDWD252"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.DDWD\d+$", RegexOptions.Compiled)]
    public static partial Regex DdwdAddressRegex();
#else
    private static readonly Regex DdwdAddressRegexInstance = new Regex(@"^DB\d+\.DDWD\d+$", RegexOptions.Compiled);
    public static Regex DdwdAddressRegex() => DdwdAddressRegexInstance;
#endif

    /// <summary>
    /// Validates DBD (Data Block Double Word - 32-bit) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DBD10"</returns>
    /// <example>Valid formats: "DB1.DBD0", "DB100.DBD252"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.DBD\d+$", RegexOptions.Compiled)]
    public static partial Regex DbdAddressRegex();
#else
    private static readonly Regex DbdAddressRegexInstance = new Regex(@"^DB\d+\.DBD\d+$", RegexOptions.Compiled);
    public static Regex DbdAddressRegex() => DbdAddressRegexInstance;
#endif

    /// <summary>
    /// Validates DWD (Double Word Data) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DWD10"</returns>
    /// <example>Valid formats: "DB1.DWD0", "DB100.DWD252"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.DWD\d+$", RegexOptions.Compiled)]
    public static partial Regex DwdAddressRegex();
#else
    private static readonly Regex DwdAddressRegexInstance = new Regex(@"^DB\d+\.DWD\d+$", RegexOptions.Compiled);
    public static Regex DwdAddressRegex() => DwdAddressRegexInstance;
#endif

    /// <summary>
    /// Validates LWD (Long Word Data) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.LWD10"</returns>
    /// <example>Valid formats: "DB1.LWD0", "DB100.LWD248"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.LWD\d+$", RegexOptions.Compiled)]
    public static partial Regex LwdAddressRegex();
#else
    private static readonly Regex LwdAddressRegexInstance = new Regex(@"^DB\d+\.LWD\d+$", RegexOptions.Compiled);
    public static Regex LwdAddressRegex() => LwdAddressRegexInstance;
#endif

    /// <summary>
    /// Validates DBT (Data Block Time) addresses for time values.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DBT10"</returns>
    /// <example>Valid formats: "DB1.DBT0", "DB100.DBT252"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.DBT\d+$", RegexOptions.Compiled)]
    public static partial Regex DbtAddressRegex();
#else
    private static readonly Regex DbtAddressRegexInstance = new Regex(@"^DB\d+\.DBT\d+$", RegexOptions.Compiled);
    public static Regex DbtAddressRegex() => DbtAddressRegexInstance;
#endif

    /// <summary>
    /// Validates DTL (Date and Time Long) addresses for extended date/time values.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DTL10"</returns>
    /// <example>Valid formats: "DB1.DTL0", "DB100.DTL240"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.DTL\d+$", RegexOptions.Compiled)]
    public static partial Regex DtlAddressRegex();
#else
    private static readonly Regex DtlAddressRegexInstance = new Regex(@"^DB\d+\.DTL\d+$", RegexOptions.Compiled);
    public static Regex DtlAddressRegex() => DtlAddressRegexInstance;
#endif

    /// <summary>
    /// Validates DAT (Date) addresses for date-only values.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DAT10"</returns>
    /// <example>Valid formats: "DB1.DAT0", "DB100.DAT254"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.DAT\d+$", RegexOptions.Compiled)]
    public static partial Regex DatAddressRegex();
#else
    private static readonly Regex DatAddressRegexInstance = new Regex(@"^DB\d+\.DAT\d+$", RegexOptions.Compiled);
    public static Regex DatAddressRegex() => DatAddressRegexInstance;
#endif

    /// <summary>
    /// Validates LDT (Long Date Time) addresses for extended date/time operations.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.LDT10"</returns>
    /// <example>Valid formats: "DB1.LDT0", "DB100.LDT240"</example>
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^DB\d+\.LDT\d+$", RegexOptions.Compiled)]
    public static partial Regex LdtAddressRegex();
#else
    private static readonly Regex LdtAddressRegexInstance = new Regex(@"^DB\d+\.LDT\d+$", RegexOptions.Compiled);
    public static Regex LdtAddressRegex() => LdtAddressRegexInstance;
#endif

}
