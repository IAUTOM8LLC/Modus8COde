using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Global.Options
{
    public class BraintreeSettings
    {
        public string BraintreeEnvironment { get; set; }
        public string BraintreeMerchantId { get; set; }
        public string BraintreePublicKey { get; set; }
        public string BraintreePrivateKey { get; set; }
        public string BraintreeMerchantAccountId { get; set; }
    }
}
