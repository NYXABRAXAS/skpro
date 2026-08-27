using QRCoder;

namespace SuryodaySelfKiosk.Services;

public interface IQrCodeService
{
    /// <summary>Returns a PNG data URI (base64) for the given payload, renderable in an &lt;img&gt; tag.</summary>
    string GetQrDataUri(string payload, int pixelsPerModule = 10);
}

/// <summary>
/// Server-side QR generation using QRCoder's fully-managed PNG renderer
/// (no System.Drawing / native dependency).
/// </summary>
public class QrCodeService : IQrCodeService
{
    public string GetQrDataUri(string payload, int pixelsPerModule = 10)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data).GetGraphic(pixelsPerModule);
        return $"data:image/png;base64,{Convert.ToBase64String(png)}";
    }
}
