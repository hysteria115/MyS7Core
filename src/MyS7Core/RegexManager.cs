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
    [GeneratedRegex(@"^DB\d+\.DBX\d+\.[0-7]$", RegexOptions.Compiled)]
    public static partial Regex DbxAddressRegex();

    /// <summary>
    /// Validates DBW (Data Block Word - 16-bit signed integer) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DBW10"</returns>
    /// <example>Valid formats: "DB1.DBW0", "DB100.DBW254"</example>
    [GeneratedRegex(@"^DB\d+\.DBW\d+$", RegexOptions.Compiled)]
    public static partial Regex DbwAddressRegex();

    /// <summary>
    /// Validates UDBW (Unsigned Data Block Word - 16-bit unsigned integer) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.UDBW10"</returns>
    /// <example>Valid formats: "DB1.UDBW0", "DB100.UDBW254"</example>
    [GeneratedRegex(@"^DB\d+\.UDBW\d+$", RegexOptions.Compiled)]
    public static partial Regex UdbwAddressRegex();

    /// <summary>
    /// Validates SIT (Small Integer - 8-bit signed integer) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.SIT10"</returns>
    /// <example>Valid formats: "DB1.SIT0", "DB100.SIT255"</example>
    [GeneratedRegex(@"^DB\d+\.SIT\d+$", RegexOptions.Compiled)]
    public static partial Regex SitAddressRegex();

    /// <summary>
    /// Validates USIT (Unsigned Small Integer - 8-bit unsigned integer) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.USIT10"</returns>
    /// <example>Valid formats: "DB1.USIT0", "DB100.USIT255"</example>
    [GeneratedRegex(@"^DB\d+\.USIT\d+$", RegexOptions.Compiled)]
    public static partial Regex UsitAddressRegex();

    /// <summary>
    /// Validates DIT (Double Integer - 32-bit signed integer) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DIT10"</returns>
    /// <example>Valid formats: "DB1.DIT0", "DB100.DIT252"</example>
    [GeneratedRegex(@"^DB\d+\.DIT\d+$", RegexOptions.Compiled)]
    public static partial Regex DitAddressRegex();

    /// <summary>
    /// Validates UDIT (Unsigned Double Integer - 32-bit unsigned integer) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.UDIT10"</returns>
    /// <example>Valid formats: "DB1.UDIT0", "DB100.UDIT252"</example>
    [GeneratedRegex(@"^DB\d+\.UDIT\d+$", RegexOptions.Compiled)]
    public static partial Regex UditAddressRegex();

    /// <summary>
    /// Validates DBB (Data Block Byte) addresses with additional byte offset.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DBB10.1"</returns>
    /// <example>Valid formats: "DB1.DBB0.0", "DB100.DBB255.255"</example>
    [GeneratedRegex(@"^DB\d+\.DBB\d+\.\d+$", RegexOptions.Compiled)]
    public static partial Regex DbbAddressRegex();

    /// <summary>
    /// Validates DBS (Data Block String) addresses with string length parameter.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DBS10.20"</returns>
    /// <example>Valid formats: "DB1.DBS0.1", "DB100.DBS252.254"</example>
    [GeneratedRegex(@"^DB\d+\.DBS\d+\.\d+$", RegexOptions.Compiled)]
    public static partial Regex DbsAddressRegex();

    /// <summary>
    /// Validates DBF (Data Block Float - 32-bit floating point) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DBF10"</returns>
    /// <example>Valid formats: "DB1.DBF0", "DB100.DBF252"</example>
    [GeneratedRegex(@"^DB\d+\.DBF\d+$", RegexOptions.Compiled)]
    public static partial Regex DbfAddressRegex();

    /// <summary>
    /// Validates DDB (Double Data Block - 64-bit floating point) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DDB10"</returns>
    /// <example>Valid formats: "DB1.DDB0", "DB100.DDB248"</example>
    [GeneratedRegex(@"^DB\d+\.DDB\d+$", RegexOptions.Compiled)]
    public static partial Regex DdbAddressRegex();

    /// <summary>
    /// Validates DDWD (Data Block Double Word Hex - 32-bit DWord displayed as hexadecimal) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DDWD10"</returns>
    /// <example>Valid formats: "DB1.DDWD0", "DB100.DDWD252"</example>
    [GeneratedRegex(@"^DB\d+\.DDWD\d+$", RegexOptions.Compiled)]
    public static partial Regex DdwdAddressRegex();

    /// <summary>
    /// Validates DBD (Data Block Double Word - 32-bit) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DBD10"</returns>
    /// <example>Valid formats: "DB1.DBD0", "DB100.DBD252"</example>
    [GeneratedRegex(@"^DB\d+\.DBD\d+$", RegexOptions.Compiled)]
    public static partial Regex DbdAddressRegex();

    /// <summary>
    /// Validates DWD (Double Word Data) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DWD10"</returns>
    /// <example>Valid formats: "DB1.DWD0", "DB100.DWD252"</example>
    [GeneratedRegex(@"^DB\d+\.DWD\d+$", RegexOptions.Compiled)]
    public static partial Regex DwdAddressRegex();

    /// <summary>
    /// Validates LWD (Long Word Data) addresses.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.LWD10"</returns>
    /// <example>Valid formats: "DB1.LWD0", "DB100.LWD248"</example>
    [GeneratedRegex(@"^DB\d+\.LWD\d+$", RegexOptions.Compiled)]
    public static partial Regex LwdAddressRegex();

    /// <summary>
    /// Validates DBT (Data Block Time) addresses for time values.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DBT10"</returns>
    /// <example>Valid formats: "DB1.DBT0", "DB100.DBT252"</example>
    [GeneratedRegex(@"^DB\d+\.DBT\d+$", RegexOptions.Compiled)]
    public static partial Regex DbtAddressRegex();

    /// <summary>
    /// Validates DTL (Date and Time Long) addresses for extended date/time values.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DTL10"</returns>
    /// <example>Valid formats: "DB1.DTL0", "DB100.DTL240"</example>
    [GeneratedRegex(@"^DB\d+\.DTL\d+$", RegexOptions.Compiled)]
    public static partial Regex DtlAddressRegex();

    /// <summary>
    /// Validates DAT (Date) addresses for date-only values.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.DAT10"</returns>
    /// <example>Valid formats: "DB1.DAT0", "DB100.DAT254"</example>
    [GeneratedRegex(@"^DB\d+\.DAT\d+$", RegexOptions.Compiled)]
    public static partial Regex DatAddressRegex();

    /// <summary>
    /// Validates LDT (Long Date Time) addresses for extended date/time operations.
    /// </summary>
    /// <returns>A compiled regex that matches addresses like "DB1.LDT10"</returns>
    /// <example>Valid formats: "DB1.LDT0", "DB100.LDT240"</example>
    [GeneratedRegex(@"^DB\d+\.LDT\d+$", RegexOptions.Compiled)]
    public static partial Regex LdtAddressRegex();

}
