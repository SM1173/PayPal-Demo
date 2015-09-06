using MvcApplication1.Models;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using MvcApplication1.Models.PaymentDetails;

namespace MvcApplication1.Models.CardDetails
{
    public class CardDetailsModel
    {
        public static Payment GetCardPaymentDetailsForPaypal(double amount)
        {

            Item cardItem = CreateCardItem(amount);

            List<Item> itms = new List<Item>();
            itms.Add(cardItem);
            ItemList itemList = new ItemList();
            itemList.items = itms;

            CreditCard crdtCard = GetPaymentCardDetails();
            Transaction tran = GetCardTransactionDetails(itemList, amount);

            // Now, we have to make a list of trasaction and add the trasactions object
            // to this list. You can create one or more object as per your requirements

            List<Transaction> transactions = new List<Transaction>();
            transactions.Add(tran);

            FundingInstrument fundInstrument = new FundingInstrument();
            fundInstrument.credit_card = crdtCard;

            // The Payment creation API requires a list of FundingIntrument

            List<FundingInstrument> fundingInstrumentList = new List<FundingInstrument>();
            fundingInstrumentList.Add(fundInstrument);

            // Now create Payer object and assign the fundinginstrument list to the object
            Payer payr = new Payer();
            payr.funding_instruments = fundingInstrumentList;
            payr.payment_method = "credit_card";

            // finally create the payment object and assign the payer object & transaction list to it
            Payment payment = new Payment();
            payment.intent = "sale";
            payment.payer = payr;
            payment.transactions = transactions;

            return payment;

        }

        private static Item CreateCardItem(double amount)
        {
            Item cardItem = new Item
            {
                currency = "GBP",
                description = "Card Payment",
                name = "DemoPyt",
                price = amount.ToString(),
                quantity = "1",
                sku = "1"
            };
            return cardItem;
        }

        private static Address GetCardPaymentAddress()
        {
            Address billingAddress = new Address();
            billingAddress.city = "London";
            billingAddress.country_code = "GB";
            billingAddress.line1 = "11 New Street";
            billingAddress.postal_code = "N21 3JX";
            billingAddress.state = "";
            return billingAddress;
        }
        
        private static CreditCard GetPaymentCardDetails()
        {
            CreditCard creditCard = new CreditCard();
            creditCard.billing_address = GetCardPaymentAddress();
            creditCard.cvv2 = "123";
            creditCard.expire_month = 10;
            creditCard.expire_year = 2020;
            creditCard.first_name = "Joe";
            creditCard.last_name = "Smith";
            creditCard.number = "4137354109223726";
            creditCard.type = "visa";

            return creditCard;
        }

        private static Transaction GetCardTransactionDetails(ItemList itemList, double amount)
        {

            Details details = new Details();
            details.shipping = "0";
            details.subtotal = amount.ToString();
            details.tax = "0";

            Amount amnt = new Amount();
            amnt.currency = "GBP";
            // Total = shipping tax + subtotal.
            amnt.total = amount.ToString();
            amnt.details = details;

            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);

            var guid = Convert.ToString((new Random()).Next(100000));

            Guid uniqueGuid = Guid.NewGuid();

            

            // Now make a trasaction object and assign the Amount object
            Transaction tran = new Transaction();
            tran.amount = amnt;
            tran.description = "Money due";
            tran.item_list = itemList;
            tran.invoice_number = "PAYPAL-" + guid.ToString(); // uniqueGuid.ToString().ToUpper(); // "Invoice # " + timestamp;

            return tran;
        }

    }
}