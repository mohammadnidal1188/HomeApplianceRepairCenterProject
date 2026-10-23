using System.Globalization;
using System.Text.RegularExpressions;

namespace HomeApplianceRepairCenter.Helpers;

/// <summary>
/// Static helper that validates raw input before it reaches the domain objects.
/// </summary>
public static class ValidationHelper
{
    /// <summary>Date format used by the whole system.</summary>
    public const string DateFormat = "yyyy-MM-dd";

    // Three capital letters, a dash, six digits (e.g. REF-100234). \z = end of text, no trailing newline.
    private static readonly Regex SerialNumberPattern = new(@"^[A-Z]{3}-[0-9]{6}\z", RegexOptions.Compiled);

    /// <summary>Checks the serial number format (AAA-123456).</summary>
    public static bool IsValidSerialNumber(string? serialNumber)
        => !string.IsNullOrWhiteSpace(serialNumber) && SerialNumberPattern.IsMatch(serialNumber);

    /// <summary>Checks that a text is not blank and has at least <paramref name="minLength"/> characters.</summary>
    public static bool IsValidText(string? text, int minLength = 2)
        => !string.IsNullOrWhiteSpace(text) && text.Trim().Length >= minLength;

    /// <summary>Checks that a cost or a duration is not negative.</summary>
    public static bool IsNonNegative(decimal value) => value >= 0m;

    /// <summary>Checks that a capacity or a size is greater than zero.</summary>
    public static bool IsPositive(decimal value) => value > 0m;

    /// <summary>Parses a date written as yyyy-MM-dd; returns false if the text is not a real date.</summary>
    public static bool TryParseDate(string? input, out DateTime date)
        => DateTime.TryParseExact(input, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    /// <summary>The promised delivery date cannot be earlier than the received date.</summary>
    public static bool IsValidDeliveryDate(DateTime receivedDate, DateTime expectedDeliveryDate)
        => expectedDeliveryDate.Date >= receivedDate.Date;
}
