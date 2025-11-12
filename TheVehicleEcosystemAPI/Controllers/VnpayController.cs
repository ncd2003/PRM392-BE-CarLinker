using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using System;
using System.Threading.Tasks;
using VNPAY.NET;
using VNPAY.NET.Enums; 
using VNPAY.NET.Models; 

namespace TheVehicleEcosystemAPI.Controllers 
{
    [ApiController]
    [Route("api/[controller]")]
    public class VnpayController : ControllerBase
    {
        private readonly IVnpay _vnpay;
        private readonly IOrderRepository _orderRepository;
        public VnpayController(IVnpay vnpay, IOrderRepository orderRepository)
        {
            _vnpay = vnpay;
            _orderRepository = orderRepository;
        }

        [HttpGet("CreatePaymentUrl")]
        public ActionResult<string> CreatePaymentUrl(long orderId, double moneyToPay, string description)
        {
            try
            {
                var ipAddress = GetClientIpAddress();

                var request = new PaymentRequest { 
                    PaymentId = orderId,
                    Money = moneyToPay,
                    Description = description,
                    IpAddress = ipAddress,
                    BankCode = BankCode.ANY,
                    CreatedDate = DateTime.Now,
                    Currency = Currency.VND,
                    Language = DisplayLanguage.Vietnamese
                };

                var paymentUrl = _vnpay.GetPaymentUrl(request);
                var response = new
                {
                    status = 200,
                    message = "Success",
                    data = paymentUrl
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Callback")]
        public async Task<ActionResult<string>> Callback()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                    var resultDescription = $"{paymentResult.PaymentResponse.Description}. {paymentResult.TransactionStatus.Description}.";
                    int paymentId = (int)paymentResult.PaymentId;

                    if (paymentResult.IsSuccess)
                    {
                        await _orderRepository.UpdateOrderStatus(paymentId, OrderStatus.CONFIRMED);
                        return Redirect($"carlinker://payment-success?orderId={paymentId}");

                    }
                    // Thanh toán thất bại
                    await _orderRepository.UpdateOrderStatus(paymentId, OrderStatus.FAILED);
                    return Redirect($"carlinker://payment-failed?orderId={paymentId}");
                }
                catch (Exception ex)
                {
                    // TODO: Redirect về trang lỗi của Frontend
                    return BadRequest(ex.Message);
                }
            }
            return NotFound("Không tìm thấy thông tin thanh toán.");
        }

        private string GetClientIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Nếu không có IP hoặc là localhost
            if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1")
            {
                // Thử lấy từ header X-Forwarded-For (qua proxy)
                ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

                // Nếu không có, thử X-Real-IP
                if (string.IsNullOrEmpty(ipAddress))
                    ipAddress = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();

                // Nếu vẫn không có, dùng mặc định
                if (string.IsNullOrEmpty(ipAddress))
                    ipAddress = "127.0.0.1";
            }

            // Nếu X-Forwarded-For có nhiều IP (qua nhiều proxy), lấy cái đầu tiên
            if (ipAddress.Contains(","))
                ipAddress = ipAddress.Split(",")[0].Trim();

            return ipAddress;
        }
    }
}