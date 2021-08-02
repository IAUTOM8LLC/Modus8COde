using System;

namespace IAutoM8.InfusionSoft.Interfaces
{
    public interface IInfusionSoftConfiguration
    {
        string ApplicationName { get;}
        string ApiKey { get; }
        Uri GetApiUri();
        string OrderFormUri { get; }
        string AffiliateProgramUrl { get; }
        string SilverAffiliateUrl { get; }
        string GoldAffiliateUrl { get; }
    }
}
