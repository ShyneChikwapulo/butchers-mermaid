using System.ComponentModel.DataAnnotations;

namespace ButchersMermaid.Components.Shared;

/// <summary>
/// Form model used by both the contact and wholesale enquiry forms.
/// Fields that are only relevant to one form type are nullable and validated
/// conditionally by the page.
/// </summary>
public class EnquiryFormModel
{
    [Required(ErrorMessage = "Please enter your name.")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "That doesn't look like a valid email address.")]
    [StringLength(320)]
    public string Email { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Phone { get; set; }

    // ----- Wholesale only -----
    [StringLength(200)]
    public string? BusinessName { get; set; }

    [StringLength(100)]
    public string? BusinessType { get; set; }

    [StringLength(50)]
    public string? MonthlyVolume { get; set; }

    // ----- Contact only -----
    [StringLength(200)]
    public string? Subject { get; set; }

    // ----- Both -----
    [Required(ErrorMessage = "Please enter a message.")]
    [StringLength(5000, MinimumLength = 10,
        ErrorMessage = "Message must be at least 10 characters.")]
    public string Message { get; set; } = string.Empty;

    // ----- Honeypot: hidden from humans, must stay empty -----
    public string? Website { get; set; }
}