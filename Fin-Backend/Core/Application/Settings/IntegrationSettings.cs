using System.Collections.Generic;

namespace FinTech.Core.Application.Settings
{
    /// <summary>
    /// Settings for biometric authentication integration
    /// </summary>
    public class BiometricSettings
    {
        public bool Enabled { get; set; } = false;
        public string Provider { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string BaseUrl { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
        public List<string> SupportedBiometricTypes { get; set; } = new List<string>();
        public bool RequireLivenessCheck { get; set; } = true;
        public double MatchThreshold { get; set; } = 0.8;
    }

    /// <summary>
    /// Settings for credit bureau integration
    /// </summary>
    public class CreditBureauSettings
    {
        public bool Enabled { get; set; } = false;
        public string Provider { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string BaseUrl { get; set; }
        public int TimeoutSeconds { get; set; } = 60;
        public string DefaultCurrency { get; set; } = "NGN";
        public bool AutoPullCreditReport { get; set; } = false;
        public List<string> SupportedBureaus { get; set; } = new List<string>();
        public Dictionary<string, string> BureauEndpoints { get; set; } = new Dictionary<string, string>();
    }
}