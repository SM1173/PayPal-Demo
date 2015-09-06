using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PayPal.Api;

namespace MvcApplication1.Models.PaymentDetails
{
    public class PayPalCommon
    {

        public static Item CreatePayPalItem(double amount, string pytName, string pytDescription)
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

        public static Details CreatePayPalDetails(double amount)
        {
            Details details = new Details();
            details.shipping = "0";
            details.subtotal = amount.ToString();
            details.tax = "0";

            return details;
        }

        public static Amount CreatePayPalAmount(double pytAmount, Details details)
        {
            Amount amount = new Amount()
            {
                currency = "GBP",
                total = pytAmount.ToString(), // Total must be equal to sum of shipping, tax and subtotal.
                details = details
            };
            return amount;
        }

        public static List<Transaction> CreatePayPalAmount(Amount payPalAmount, ItemList itemList, string guid)
        {
            List<Transaction> transactionList = new List<Transaction>();

            transactionList.Add(new Transaction()
            {
                description = "Transaction description.",
                invoice_number = guid, // "your invoice number: " + timestamp,
                amount = payPalAmount,
                item_list = itemList
            });

            return transactionList;
        }

    }
}