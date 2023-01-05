using Khalti.Payment.Gateway;
using Khalti_Payment_Gateway_Demo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Khalti_Payment_Gateway_Demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static string secretKey = "live_secret_key_68791341fdd94846a146f0457ff7b455";
        private static bool sandBoxMode = true;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index([FromQuery] string pidx)
        {
            PaymentApi.Version = "v2";
            if (!string.IsNullOrWhiteSpace(pidx))
            {
                await VerifyPayment(pidx);
            }
            return View();
        }
        public async Task<ActionResult> PayWithKhalti()
        {
            string currentUrl = new Uri($"{Request.Scheme}://{Request.Host}").AbsoluteUri;
            KhaltiPayment khaltiPayment = new KhaltiPayment();
            //KhaltiRequest khaltiRequest = new KhaltiRequest
            dynamic khaltiRequest = new
            {
                return_url = currentUrl,
                website_url = currentUrl,
                amount = 1300,
                purchase_order_id = "test12",
                purchase_order_name = "test",
                customer_info = new Customer_Info
                {
                    name = "Ashim Upadhaya",
                    email = "example@gmail.com",
                    phone = "9811496763"
                },
                product_details = new List<Product_Detail>
                {
                    new Product_Detail{identity= "1234567890", name= "Khalti logo", total_price= 1300, quantity= 1,unit_price= 1300 }
                },
                amount_breakdown = new List<Amount_Breakdown>
                {
                    new Amount_Breakdown{ label= "Mark Price",amount=1000 },
                    new Amount_Breakdown{label="VAT", amount=300 }
                }
            };
            //Can receive dynamic object and bind on custom object
            dynamic responsed = await khaltiPayment.ProcessPayment<dynamic>(secretKey, sandBoxMode, khaltiRequest);
            KhaltiResponse response = await khaltiPayment.ProcessPayment<KhaltiResponse>(secretKey, sandBoxMode, khaltiRequest);
            if (!string.IsNullOrEmpty(response.pidx))
                return Redirect(response.payment_url);
            else return View();
        }
        private async Task<ActionResult> VerifyPayment(string pidx)
        {
            var khaltiPayment = new KhaltiPayment();
            //Can receive dynamic object and bind on custom object
            //object response = await khaltiPayment.VerifyPayment<object>(secretKey, sandBoxMode, pidx); 
            KhaltiPayResponse response = await khaltiPayment.VerifyPayment<KhaltiPayResponse>(secretKey, sandBoxMode, pidx);
            //if (response.status == Convert.ToInt32(HttpStatusCode.OK)) //
            if (response != null && !string.IsNullOrEmpty(response.status) && response.status.ToLower() == "completed")
            {
                ViewBag.Message = string.Format("Payment with khalti completed successfully with pidx: {0} and amount: {1}", response.pidx, response.total_amount);
                ///Verify Payment Amount on ProcessPayment and total_amount on verification response
            }
            else
            {
                //verification failed see the json object or KhaltiPayResponse.detail
                ViewBag.Message = string.Format("Payment with khalti failed");
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}