namespace ButchersMermaid.Data;

/// <summary>
/// Single source of truth for business data used across the site.
/// If any of these change, edit this file only — pages reference it.
/// </summary>
public static class BusinessInfo
{
    // ---------- Contact ----------
    public const string Email = "info@thebutchersmermaid.co.za";
    public const string PhoneE164 = "+27213000282";
    public const string PhoneDisplay = "+27 21 300 0282";

    // ---------- Address ----------
    public const string AddressLine1 = "Shop 1, Red Sails Building";
    public const string AddressLine2 = "26 Main Road";
    public const string AddressLine3 = "Hout Bay, Cape Town, 7806";
    public const string AddressShort = "Shop 1, Red Sails Building · 26 Main Road · Hout Bay";

    // ---------- External links ----------
    public const string DineplanUrl  = "https://www.dineplan.com/restaurant/the-butchers-mermaid";
    public const string UberEatsUrl  = "https://www.ubereats.com/za/store/the-butchers-mermaid/WdjWPxY4RiyUx_Du-jwVdA";
    public const string FacebookUrl  = "https://www.facebook.com/thebutchersmermaid";
    public const string InstagramUrl = "https://www.instagram.com/thebutchersmermaid";

    // ---------- Opening hours ----------
    public record OpeningHours(string Day, string Time, bool Closed);

    public static readonly OpeningHours[] Hours =
    {
        new("Monday",    "Closed",       true),
        new("Tuesday",   "09:00–21:00",  false),
        new("Wednesday", "09:00–21:00",  false),
        new("Thursday",  "09:00–21:00",  false),
        new("Friday",    "09:00–21:00",  false),
        new("Saturday",  "09:00–21:00",  false),
        new("Sunday",    "Closed",       true),
    };

    // ---------- Timezone ----------
    // Resolved once. Linux containers use IANA IDs ("Africa/Johannesburg"),
    // Windows uses the legacy ID ("South Africa Standard Time"). Try both,
    // fall back to UTC if neither is available.
    public static readonly TimeZoneInfo Sast = ResolveSast();

    private static TimeZoneInfo ResolveSast()
    {
        string[] ids = { "Africa/Johannesburg", "South Africa Standard Time" };
        foreach (var id in ids)
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        return TimeZoneInfo.Utc;
    }

    /// <summary>Formats a UTC datetime in South African Standard Time.</summary>
    public static string FormatSast(DateTime utc, string format = "yyyy-MM-dd HH:mm") =>
        TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utc, DateTimeKind.Utc), Sast)
        .ToString(format);
}