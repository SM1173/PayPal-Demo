using MvcApplication1.Models;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication1.Models.CardDetails;
using MvcApplication1.ViewModels;
using System.Globalization;

using Newtonsoft.Json;

namespace MvcApplication1.Controllers
{
    public class PaypalController : Controller
    {
        //
        // GET: /Paypal/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ConfirmPaymentView()
        {
            return View();
        }

        public ActionResult SuccessView(PaypalViewModel model)
        {
            PaypalViewModel model1 = (PaypalViewModel)TempData["paypalviewmodel"];

            var apiContext = Configuration.GetAPIContext();

            //**************************************
            //Getting back error here:
            //{"name":"BUSINESS_ERROR","message":"Invalid encrypted id.","debug_id":"8b3f27ecb634d"}
            //so hard code invoiceId from PayPal example to show workflow

            //string invoiceId = model1.TransactionGuid;
            string invoiceId = "INV2-W6VG-MFK4-HQRT-RS6Z";
            try
            {
                model1.PayPalInvoice = Invoice.Get(apiContext, invoiceId);
            }
            catch (PayPal.PayPalException ex)
            {
                Logger.Log("Error: " + ex.Message);
                model1.PayPal_Exception = ex;
                model1.ErrMessage = "You payment was successful, however there's been a problem trying to display your receipt";
                return View(model1);
            }

            return View(model1);
        }

        [HttpPost]
        public ActionResult ConfirmPaymentView(PaypalViewModel model)
        {
            PaypalViewModel model1 = (PaypalViewModel)TempData["paypalviewmodel1"];
            try
            {
                // ### Api Context
                // Pass in a `APIContext` object to authenticate 
                // the call and to send a unique request id 
                // (that ensures idempotency). The SDK generates
                // a request id if you do not pass one explicitly. 
                // See [Configuration.cs](/Source/Configuration.html) to know more about APIContext.
                APIContext apiContext = Configuration.GetAPIContext();

                // Create is a Payment class function which actually sends the payment details to the paypal API for the payment. The function is passed with the ApiContext which we received above.

                Payment createdPayment = model1.PayPalPayment.Create(apiContext);

                //if the createdPayment.State is "approved" it means the payment was successfull else not

                if (createdPayment.state.ToLower() != "approved")
                {
                    model1.PayPalRestError.message = createdPayment.state.ToString();
                    return View("FailureView", model1);
                }
            }
            catch (PayPal.PayPalException ex)
            {
                Logger.Log("Error: " + ex.Message);
                model1.ErrMessage = ex.Message;
                return View("FailureView", model1);
            }

            TempData["paypalviewmodel"] = model1;
            return RedirectToAction("SuccessView");
            //return View("SuccessView");

        }


        public ActionResult CardPayment(PaypalViewModel model)
        {
            //create and item for which you are taking payment
            //if you need to add more items in the list
            //Then you will need to create multiple item objects or use some loop to instantiate object

            PaypalViewModel model1 = (PaypalViewModel)TempData["paypalviewmodel"];

            Payment pyt = CardDetailsModel.GetCardPaymentDetailsForPaypal(model1.Amount);

            model.PayPalPayment = pyt;
            model.Amount = model1.Amount;
            model.TransactionGuid = pyt.transactions[0].invoice_number.ToString();

            //before payment, move details to confirm payment view

            TempData["paypalviewmodel1"] = model;

            return View("ConfirmPaymentView", model);
        }


        public ActionResult PaypalPayment()
        {
            PaypalViewModel model1 = (PaypalViewModel)TempData["paypalviewmodel"];
            APIContext apiContext = Configuration.GetAPIContext();

            try
            {
                string payerId = Request.Params["PayerID"];

                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist
                    //it is returned by the create function call of the payment class

                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Paypal/PaypalPayment?";

                    //guid we are generating for storing the paymentID received in session
                    //after calling the create function and it is used in the payment execution

                    var guid = Convert.ToString((new Random()).Next(100000));
                    string uniqueGuid = "PAYPAL-" + guid.ToString();

                    model1.TransactionGuid = uniqueGuid.ToString();

                    //CreatePayment function gives us the payment approval url on which payer is redirected for paypal acccount payment
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid, model1.Amount, uniqueGuid);

                    //get links returned from paypal in response to Create function call

                    var links = createdPayment.links.GetEnumerator();

                    string paypalRedirectUrl = null;

                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;

                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment
                            paypalRedirectUrl = lnk.href;
                        }
                    }

                    // saving the paymentID in the key guid
                    Session.Add(guid, createdPayment.id);

                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This section is executed when we have received all the payments parameters
                    // from the previous call to the function Create
                    // Executing a payment

                    var guid = Request.Params["guid"];

                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);

                    if (executedPayment.state.ToLower() != "approved")
                    {
                        model1.PayPalRestError.message = executedPayment.state.ToString();
                        return View("FailureView", model1);
                    }
                    else
                    {
                        return RedirectToAction("SuccessView", model1);
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error"+ ex.Message);
                model1.ErrMessage = ex.Message;
                return View("FailureView", model1);
            }

            //return View("SuccessView");
        }

        private PayPal.Api.Payment payment;

        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            this.payment = new Payment() { id = paymentId };
            return this.payment.Execute(apiContext, paymentExecution);
        }

        //****************
        //TO DO - remove common functions to separate class
        private Payment CreatePayment(APIContext apiContext, string redirectUrl, double pytAmount, string guid)
        {

            var itemList = new ItemList() { items = new List<Item>() };

            itemList.items.Add(new Item()
            {
                name = "Item Name",
                currency = "GBP",
                price = pytAmount.ToString(),
                quantity = "1",
                sku = "sku"
            });

            var payer = new Payer() { payment_method = "paypal" };

            // Configure Redirect Urls here with RedirectUrls object
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl,
                return_url = redirectUrl
            };


            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = pytAmount.ToString()
            };

            var amount = new Amount()
            {
                currency = "GBP",
                total = pytAmount.ToString(), // Total must be equal to sum of shipping, tax and subtotal.
                details = details
            };

            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);
            var transactionList = new List<Transaction>();

            transactionList.Add(new Transaction()
            {
                description = "Transaction description.",
                invoice_number = guid, // "your invoice number: " + timestamp,
                amount = amount,
                item_list = itemList
            });

            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            // Create a payment using a APIContext
            return this.payment.Create(apiContext);

        }
    }
}
