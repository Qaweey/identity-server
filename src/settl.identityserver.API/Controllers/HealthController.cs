using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace settl.identityserver.API.Controllers
{
    [ApiController]
    [EnableCors("AllowAll")]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult CheckApiHealth()
        {
            return Ok(new { status = "ok" });
        }
    }
}