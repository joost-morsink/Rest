using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Test.Helpers
{
    public abstract class PaymentInstrument
    {
        private PaymentInstrument() { }

        public sealed class CreditCard : PaymentInstrument
        {
            public string CardNumber { get; set; }
            public string ExpiryDate { get; set; }
        }
        public sealed class BankAccount : PaymentInstrument
        {
            public string AccountNumber { get; set; }
        }
    }
}
