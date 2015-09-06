using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PayPal.Api;

namespace MvcApplication1.ViewModels
{
    public class PaypalViewModel
    {
        public bool IsPayPalPayment { get; set; }
        public double Amount { get; set; }
        public string TransactionGuid { get; set; } //used as invoice id

        public Payment PayPalPayment = new Payment();
        public Invoice PayPalInvoice { get; set; }

        public string ErrMessage { get; set; }
        public PayPal.PayPalException PayPal_Exception { get; set; }
        public PayPal.Api.Error PayPalRestError { get; set; }
        public PayPal.Api.ErrorDetails PayPalErrorDetails { get; set; }
    }
}