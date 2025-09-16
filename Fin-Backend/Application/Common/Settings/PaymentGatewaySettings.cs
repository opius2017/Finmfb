namespace FinTech.Core.Application.Common.Settings;

/// <summary>
/// Configuration settings for payment gateway integration
/// </summary>
public class PaymentGatewaySettings
{
    /// <summary>
    /// Base URL for the payment gateway API
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Secret key for authenticating with the payment gateway
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Public key for client-side operations
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Default success redirect URL for payment completion
    /// </summary>
    public string DefaultSuccessUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Default cancel redirect URL for payment cancellation
    /// </summary>
    public string DefaultCancelUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Default webhook URL for receiving payment notifications
    /// </summary>
    public string WebhookUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Secret hash for validating webhook requests
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;
}