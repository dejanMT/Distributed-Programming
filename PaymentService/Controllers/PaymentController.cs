using Microsoft.AspNetCore.Mvc;
using PaymentService.Models;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService.Services.PaymentServices _paymentService;

        public PaymentController(PaymentService.Services.PaymentServices paymentService)
        {
            _paymentService = paymentService;
        }

        // Create a new payment
        [HttpPost]
        public async Task<IActionResult> Pay([FromBody] Payment payment, [FromQuery] bool discount = false)
        {
            try
            {
                var result = await _paymentService.CreatePayment(payment, discount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Get all payments for a user by email
        [HttpGet("{email}")]
        public async Task<IActionResult> GetPayments(string email)
        {
            var payments = await _paymentService.GetPayments(email);
            return Ok(payments);
        }
    }
}
