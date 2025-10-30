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
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1")
                {
                    ipAddress = "127.0.0.1";
                }

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
                return Created(paymentUrl, paymentUrl);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("IpnAction")]
        public async Task<IActionResult> IpnAction()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                    int paymentId = (int)paymentResult.PaymentId;

                    if (paymentResult.IsSuccess)
                    {
                        // TODO: Xử lý nghiệp vụ (cập nhật đơn hàng,...)
                        await _orderRepository.UpdateOrderStatus(paymentId, OrderStatus.CONFIRMED);
                        return Ok(new { RspCode = "00", Message = "Confirm Success" });
                    }

                    // Thanh toán thất bại
                    await _orderRepository.UpdateOrderStatus(paymentId, OrderStatus.FAILED);
                    return Ok(new { RspCode = "00", Message = "Confirm Success" });
                }
                catch (Exception ex)
                {
                    return Ok(new { RspCode = "99", Message = $"Unknown error: {ex.Message}" });
                }
            }
            return Ok(new { RspCode = "97", Message = "Invalid Checksum" });
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
                        return Redirect($"http://localhost:5173/order-success?orderId={paymentId}");

                    }
                    // Thanh toán thất bại
                    await _orderRepository.UpdateOrderStatus(paymentId, OrderStatus.FAILED);
                    return Redirect($"http://localhost:5173/order-failed?orderId={paymentId}");
                }
                catch (Exception ex)
                {
                    // TODO: Redirect về trang lỗi của Frontend
                    return BadRequest(ex.Message);
                }
            }
            return NotFound("Không tìm thấy thông tin thanh toán.");
        }
    }
}