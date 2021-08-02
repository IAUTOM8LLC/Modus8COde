using IAutoM8.InfusionSoft.Infrastructure;
using IAutoM8.InfusionSoft.Interfaces;
using Microsoft.Extensions.Options;
using System;

namespace IAutoM8.InfusionSoft
{
    class InfusionSoftConfiguration: IInfusionSoftConfiguration
    {
        private const string UriMask = "https://{0}.infusionsoft.com/api/xmlrpc";
        private const string OrderFormUriMask = "https://{0}.infusionsoft.com/app/orderForms/{1}";
        private const string AffiliateCookieUriMask = "https://{0}.infusionsoft.com/go/{1}/{2}";
        private const string AffiliateProgramUriMask = "https://{0}.infusionsoft.com/app/authentication/login?isClientOrAffiliate=true";
        private readonly IOptions<InfusionSoftDataOptions> _dataOptions;

        public InfusionSoftConfiguration(IOptions<InfusionSoftOption> options,
            IOptions<InfusionSoftDataOptions> dataOptions)
        {
            ApplicationName = options.Value.Application.ToLowerInvariant();
            ApiKey = options.Value.Key;
            _dataOptions = dataOptions;
        }

        #region IInfusionSoftConfiguration Members

        public string ApplicationName { get;}

        public string ApiKey { get; }

        private Uri _uri;
        public Uri GetApiUri()
        {
            if (_uri == null)
            {
                _uri = new Uri(string.Format(UriMask, ApplicationName));
            }

            return _uri;
        }
        #endregion

        public string OrderFormUri => string.Format(OrderFormUriMask, ApplicationName, _dataOptions.Value.OrderFormId);

        public string SilverAffiliateUrl => string.Format(AffiliateCookieUriMask, ApplicationName,
            _dataOptions.Value.RefCodeSilver, "{0}");
        public string GoldAffiliateUrl => string.Format(AffiliateCookieUriMask, ApplicationName,
            _dataOptions.Value.RefCodeGold, "{0}");
        public string AffiliateProgramUrl => string.Format(AffiliateProgramUriMask, ApplicationName);
    }
}
