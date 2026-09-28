namespace ButchersMermaid.Data.Entities;

public class Enquiry
{
    public int Id { get; set; }

    public EnquiryType Type { get; set; }

    // ----- Contact fields (shared between both form types) -----
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    // ----- Wholesale-specific -----
    public string? BusinessName { get; set; }
    public string? BusinessType { get; set; }
    public string? MonthlyVolume { get; set; }

    // ----- Contact-specific -----
    public string? Subject { get; set; }

    // ----- Both -----
    public string Message { get; set; } = string.Empty;

    // ----- Metadata -----
    public DateTime SubmittedAt { get; set; }
    public string? SubmittedFromIp { get; set; }

    public bool IsRead { get; set; }

    /// <summary>
    /// True if the confirmation email was successfully handed to Resend.
    /// Null means it hasn't been attempted yet.
    /// </summary>
    public bool? EmailSent { get; set; }

    /// <summary>Error message if the email failed to send.</summary>
    public string? EmailError { get; set; }
}