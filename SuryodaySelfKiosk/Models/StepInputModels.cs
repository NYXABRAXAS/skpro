using System.ComponentModel.DataAnnotations;

namespace SuryodaySelfKiosk.Models;

public class ConsentInput
{
    [Range(typeof(bool), "true", "true", ErrorMessage = "Please accept the Loan Processing Consent to continue.")]
    public bool LoanProcessingConsent { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "Please accept the Credit Bureau Consent to continue.")]
    public bool CreditBureauConsent { get; set; }

    /// <summary>Optional – not required to proceed.</summary>
    public bool CommunicationConsent { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "Please confirm you have read and agree to the declaration.")]
    public bool DeclarationAccepted { get; set; }
}

public class MobileInput
{
    [Required(ErrorMessage = "Please enter your mobile number.")]
    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10 digit mobile number.")]
    public string MobileNumber { get; set; } = string.Empty;
}

public class OtpInput
{
    [Required(ErrorMessage = "Please enter the OTP.")]
    [RegularExpression(@"^\d{4,8}$", ErrorMessage = "Enter the numeric OTP sent to your mobile.")]
    public string Otp { get; set; } = string.Empty;
}

public class AadhaarInput
{
    [Required(ErrorMessage = "Please enter your Aadhaar number.")]
    [RegularExpression(@"^\d{12}$", ErrorMessage = "Aadhaar number must be 12 digits.")]
    public string AadhaarNumber { get; set; } = string.Empty;
}

public class PanInput
{
    [Required(ErrorMessage = "Please enter your PAN number.")]
    [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]$", ErrorMessage = "Please enter a valid PAN number (e.g. ABCDE1234F).")]
    public string PanNumber { get; set; } = string.Empty;
}

public class VehicleInput : IValidatableObject
{
    [Required(ErrorMessage = "Please select New Car or Used Car.")]
    public string VehicleType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select the manufacturer.")]
    public string Manufacturer { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the model.")]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the variant.")]
    public string Variant { get; set; } = string.Empty;

    public int? RegistrationYear { get; set; }

    [Range(50000, 100000000, ErrorMessage = "Please enter a valid vehicle cost.")]
    public decimal VehicleCost { get; set; }

    [Range(1, 100000000, ErrorMessage = "Please enter a valid loan amount.")]
    public decimal RequiredLoanAmount { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RequiredLoanAmount > VehicleCost)
        {
            yield return new ValidationResult(
                "Requested loan amount cannot be greater than vehicle cost.",
                new[] { nameof(RequiredLoanAmount) });
        }

        if (VehicleType == VehicleTypes.Used)
        {
            var currentYear = DateTime.Now.Year;
            if (RegistrationYear is null)
            {
                yield return new ValidationResult(
                    "Please enter the vehicle registration year.",
                    new[] { nameof(RegistrationYear) });
            }
            else if (RegistrationYear < currentYear - 15 || RegistrationYear > currentYear)
            {
                yield return new ValidationResult(
                    "Please enter a valid registration year.",
                    new[] { nameof(RegistrationYear) });
            }
        }
    }
}

public class EmployeeIdInput
{
    [Required(ErrorMessage = "Please enter the Bank Employee ID.")]
    public string EmployeeId { get; set; } = string.Empty;
}

public static class VehicleReferenceData
{
    public static readonly string[] Manufacturers =
    {
        "Maruti Suzuki", "Hyundai", "Tata", "Mahindra", "Toyota", "Honda",
        "Kia", "Volkswagen", "Skoda", "Renault", "Nissan", "MG", "Other"
    };
}
