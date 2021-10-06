using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace AKSWebApi.Controllers
{
    [ApiController]
    [IgnoreAntiforgeryToken]
    [ApiVersion("1.0")]
    [Route("aks-api/v{v:apiVersion}/[action]")]
    public abstract class BaseApiController : ControllerBase
    {
    }
}
