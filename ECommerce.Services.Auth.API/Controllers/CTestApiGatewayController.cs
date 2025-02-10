using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Services.Auth.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class CTestApiGatewayController : ControllerBase
    {
        [HttpGet("connection-test")]
        public IActionResult Test()
        {
            return Ok("Auth API Service is running");
        }
    }
}