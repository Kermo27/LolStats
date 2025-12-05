using Microsoft.AspNetCore.Mvc;

namespace LolStatsTracker.API.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected Guid GetProfileId()
    {
        if (Request.Headers.TryGetValue("X-Profile-Id", out var profileIdStr) &&
            Guid.TryParse(profileIdStr, out var profileId))
        {
            return profileId;
        }
        
        return Guid.Empty;
    }
    
    protected ActionResult ProfileIdMissingError() => BadRequest("Profile ID header is missing.");
}
