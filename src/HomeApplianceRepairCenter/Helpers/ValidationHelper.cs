using System.Globalization;
using System.Text.RegularExpressions;

namespace HomeApplianceRepairCenter.Helpers;

// A static class cannot be instantiated, allowing us to call its methods directly using the class name.
/// <summary>
/// Static helper that validates raw input before it reaches the domain objects.
/// </summary>
public static class ValidationHelper
{
    /// <summary>Date format used by the whole system.</summary>
    public const string DateFormat = "yyyy-MM-dd";

    // Three capital letters, a dash, six digits (e.g. REF-100234). \z = end of text, no trailing newline.
    // The pattern requires 3 uppercase letters, a dash, and 6 digits. Valid: AAA-123456, Invalid: aa-123.
    private static readonly Regex SerialNumberPattern = new(@"^[A-Z]{3}-[0-9]{6}\z", RegexOptions.Compiled);

     // Called in the Appliance constructor to check the serial number format before creating the object.
    /// <summary>Checks the serial number format (AAA-123456).</summary>
    public static bool IsValidSerialNumber(string? serialNumber)
        => !string.IsNullOrWhiteSpace(serialNumber) && SerialNumberPattern.IsMatch(serialNumber);

    // minLength enforces a minimum text length, and Trim() removes outer spaces before checking the length.
    /// <summary>Checks that a text is not blank and has at least <paramref name="minLength"/> characters.</summary>
    public static bool IsValidText(string? text, int minLength = 2)
        => !string.IsNullOrWhiteSpace(text) && text.Trim().Length >= minLength;

    // Used by PartsCost and LaborHours in Appliance, because a cost or a duration cannot be negative.
    /// <summary>Checks that a cost or a duration is not negative.</summary>
    public static bool IsNonNegative(decimal value) => value >= 0m;

    /// <summary>Checks that a capacity or a size is greater than zero.</summary>
    public static bool IsPositive(decimal value) => value > 0m;

    // The out parameter passes the successfully parsed DateTime back to the caller, and if the text is invalid, it returns false.
    /// <summary>Parses a date written as yyyy-MM-dd; returns false if the text is not a real date.</summary>
    public static bool TryParseDate(string? input, out DateTime date)
        => DateTime.TryParseExact(input, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    // The rule ensures a promised delivery date is not set earlier than the date the appliance was received, used by ExpectedDeliveryDate.
    /// <summary>The promised delivery date cannot be earlier than the received date.</summary>
    public static bool IsValidDeliveryDate(DateTime receivedDate, DateTime expectedDeliveryDate)
        => expectedDeliveryDate.Date >= receivedDate.Date;
}
