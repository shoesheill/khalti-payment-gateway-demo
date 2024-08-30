using Khalti_Payment_Gateway_Demo.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using payment_gateway_nepal;
using System.Diagnostics;

namespace Khalti_Payment_Gateway_Demo.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly string k_secretKey = "live_secret_key_68791341fdd94846a146f0457ff7b455";
		private readonly string e_secretKey = "8gBm/:&EnhH.1/q";
		private readonly bool sandBoxMode = true;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		public async Task<IActionResult> Index([FromQuery] string pidx, [FromQuery] string data)
		{

			if (!string.IsNullOrWhiteSpace(pidx))
			{
				await VerifyPayment(pidx);
			}
			else if (!string.IsNullOrWhiteSpace(data))
			{
				await VerifyEsewaPayment(data);
			}
			return View();
		}
		public async Task<ActionResult> PayWithKhalti()
		{
			string currentUrl = new Uri($"{Request.Scheme}://{Request.Host}").AbsoluteUri;
			PaymentManager paymentManager = new PaymentManager(PaymentMethod.Khalti, PaymentVersion.v2, PaymentMode.Sandbox, k_secretKey);
			dynamic request = new
			{
				return_url = currentUrl,
				website_url = currentUrl,
				amount = 1300,
				purchase_order_id = "test12",
				purchase_order_name = "test",
				customer_info = new k_customer_info
				{
					name = "Sushil Shreshta",
					email = "shoesheill@gmail.com",
					phone = "9846000027"
				},
				product_details = new List<k_product_detail>
				{
					new k_product_detail{identity= "1234567890", name= "Khalti logo", total_price= 1300, quantity= 1,unit_price= 1300 }
				},
				amount_breakdown = new List<k_amount_breakdown>
				{
					new k_amount_breakdown{ label= "Mark Price",amount=1000 },
					new k_amount_breakdown{label="VAT", amount=300 }
				}
			};
			ApiResponse response = await paymentManager.ProcessPayment<ApiResponse>(request);
			k_init_response k_Init_Response = JsonConvert.DeserializeObject<k_init_response>(JsonConvert.SerializeObject(response.data));
			return Redirect(k_Init_Response.payment_url);
		}
		private async Task<ActionResult> VerifyPayment(string pidx)
		{
			PaymentManager paymentManager = new PaymentManager(PaymentMethod.Khalti, PaymentVersion.v2, PaymentMode.Sandbox, k_secretKey);
			k_response response = await paymentManager.VerifyPayment<k_response>(pidx);
			if (response != null && response.status != null) //
				if (response != null && !string.IsNullOrEmpty(response.status) && string.Equals(response.status, "completed", StringComparison.OrdinalIgnoreCase))
				{
					ViewBag.Message = string.Format($"Payment with khalti completed successfully with pidx: {response.pidx} and amount: {response.total_amount}");
					///Verify Payment Amount on ProcessPayment and total_amount on verification response
				}
				else
				{
					//verification failed see the json object or KhaltiPayResponse.detail
					ViewBag.Message = string.Format("Payment with khalti failed");
				}
			return View();
		}


		public async Task<IActionResult> PayWitheSewa()
		{
			PaymentManager paymentManager = new PaymentManager(PaymentMethod.eSewa, PaymentVersion.v2, PaymentMode.Sandbox, e_secretKey);
			string currentUrl = new Uri($"{Request.Scheme}://{Request.Host}").AbsoluteUri;
			dynamic request = new
			{
				Amount = 100,
				TaxAmount = 10,
				TotalAmount = 110,
				TransactionUuid = "bk-" + new Random().Next(10000, 100000).ToString(),
				ProductCode = "EPAYTEST",
				ProductServiceCharge = 0,
				ProductDeliveryCharge = 0,
				SuccessUrl = currentUrl,
				FailureUrl = currentUrl,
				SignedFieldNames = "total_amount,transaction_uuid,product_code",
			};
			var response = await paymentManager.ProcessPayment<ApiResponse>(request);
			return Redirect(response.data);
		}
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult> VerifyEsewaPayment(string data)
		{
			PaymentManager paymentManager = new PaymentManager(PaymentMethod.eSewa, PaymentVersion.v2, PaymentMode.Sandbox, string.Empty);
			eSewaResponse response = await paymentManager.VerifyPayment<eSewaResponse>(data);
			if (!string.IsNullOrEmpty(nameof(response)) && string.Equals(response.status, "complete", StringComparison.OrdinalIgnoreCase))
			{
				ViewBag.Message = string.Format($"Payment with eSewa completed successfully with data: {response.transaction_code} and amount: {response.total_amount}");
			}
			else
			{
				ViewBag.Message = string.Format("Payment with eSewa failed");
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