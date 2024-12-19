# eSewa and Khalti Payment Gateway Integration for .NET Core Web Applications

This document provides detailed steps to integrate **eSewa** and **Khalti**, the leading payment gateways in Nepal, into a .NET Core web application.

## Introduction

This package allows you to integrate web payment functionality for **eSewa** and **Khalti**. It supports:

1. **Initiating payments**
2. **Verifying payments**
3. **Sandbox environment** for testing

## Installation

Install the NuGet package:

```bash
> dotnet add package plugin.web.eSewa.and.Khalti
```

## Sandbox Secret Keys

Use the following sandbox keys for testing and development:

### eSewa

- **Secret Key**: `8gBm/:&EnhH.1/q`

### Khalti

- **Secret Key**: `live_secret_key_68791341fdd94846a146f0457ff7b455`

### Note

Set the `sandBoxMode` flag to `true` for development and testing.

```csharp
private readonly string Khalti_SecretKey = "live_secret_key_68791341fdd94846a146f0457ff7b455";
private readonly string eSewa_SecretKey = "8gBm/:&EnhH.1/q";
private readonly bool sandBoxMode = true;
```

> Use your own data for live environment.

---

## eSewa Payment Integration

### 1. Payment Initialization

Use the following code to initialize an eSewa payment:

```csharp
public async Task<IActionResult> PayWitheSewa()
{
    PaymentManager paymentManager = new PaymentManager(
        PaymentMethod.eSewa,
        PaymentVersion.v2,
        PaymentMode.Sandbox,
        eSewa_SecretKey
    );

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

    var response = await paymentManager.InitiatePaymentAsync<ApiResponse>(request);
    return Redirect(response.data);
}
```

### 2. Payment Verification

Verify the payment using this method:

```csharp
public async Task<IActionResult> VerifyEsewaPayment(string data)
{
    PaymentManager paymentManager = new PaymentManager(
        PaymentMethod.eSewa,
        PaymentVersion.v2,
        PaymentMode.Sandbox,
        string.Empty
    );

    eSewaResponse response = await paymentManager.VerifyPaymentAsync<eSewaResponse>(data);

    if (!string.IsNullOrEmpty(nameof(response)) && 
        string.Equals(response.status, "complete", StringComparison.OrdinalIgnoreCase))
    {
        ViewBag.Message = string.Format(
            $"Payment with eSewa completed successfully with data: {response.transaction_code} and amount: {response.total_amount}"
        );
    }
    else
    {
        ViewBag.Message = string.Format("Payment with eSewa failed");
    }
    return View();
}
```

---

## Khalti Payment Integration

### 1. Payment Initialization

Initialize Khalti payment using the code below:

```csharp
public async Task<ActionResult> PayWithKhalti()
{
    string currentUrl = new Uri($"{Request.Scheme}://{Request.Host}").AbsoluteUri;
    PaymentManager paymentManager = new PaymentManager(
        PaymentMethod.Khalti,
        PaymentVersion.v2,
        PaymentMode.Sandbox,
        Khalti_SecretKey
    );

    dynamic request = new
    {
        return_url = currentUrl,
        website_url = currentUrl,
        amount = 1300,
        purchase_order_id = "test12",
        purchase_order_name = "test",
        customer_info = new KhaltiCustomerInfo()
        {
            name = "Sushil Shreshta",
            email = "shoesheill@gmail.com",
            phone = "9846000027"
        },
        product_details = new List<KhaltiProductDetail>
        {
            new KhaltiProductDetail()
            {
                identity = "1234567890",
                name = "Khalti logo",
                total_price = 1300,
                quantity = 1,
                unit_price = 1300
            }
        },
        amount_breakdown = new List<KhaltiAmountBreakdown>
        {
            new KhaltiAmountBreakdown() { label = "Mark Price", amount = 1000 },
            new KhaltiAmountBreakdown() { label = "VAT", amount = 300 }
        }
    };

    ApiResponse response = await paymentManager.InitiatePaymentAsync<ApiResponse>(request);
    KhaltiInitResponse k_Init_Response = JsonConvert.DeserializeObject<KhaltiInitResponse>(JsonConvert.SerializeObject(response.data));
    return Redirect(k_Init_Response.payment_url);
}
```

### 2. Payment Verification

Verify the Khalti payment using the following method:

```csharp
private async Task<ActionResult> VerifyPayment(string pidx)
{
    PaymentManager paymentManager = new PaymentManager(
        PaymentMethod.Khalti,
        PaymentVersion.v2,
        PaymentMode.Sandbox,
        Khalti_SecretKey
    );

    KhaltiResponse response = await paymentManager.VerifyPaymentAsync<KhaltiResponse>(pidx);

    if (response != null && string.Equals(response.status, "completed", StringComparison.OrdinalIgnoreCase))
    {
        ViewBag.Message = string.Format(
            $"Payment with Khalti completed successfully with pidx: {response.pidx} and amount: {response.total_amount}"
        );
    }
    else
    {
        ViewBag.Message = "Payment with Khalti failed";
    }
    return View();
}
```

---

## Test Login Credentials

### eSewa

- **Username**: `9806800001/2/3/4/5`
- **Password**: `Nepal@123`
- **Token**: `123456`

### Khalti

- **Mobile Number**: `9800000001/2/3/4/5`
- **Pin**: `1111`
- **OTP**: `987654`

---

## References

- [eSewa Developer Guide](https://developer.esewa.com.np/pages/Introduction)
- [Khalti Developer Guide](https://docs.khalti.com/khalti-epayment/)
