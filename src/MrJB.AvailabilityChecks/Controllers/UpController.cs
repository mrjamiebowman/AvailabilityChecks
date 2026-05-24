using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrJB.AvailabilityChecks.Domain.Configuration;

namespace MrJB.AvailabilityChecks.Controllers;

[ApiController]
[Route("[controller]")]
public class UpController : ControllerBase
{
    // logger
    private ILogger<UpController> _logger;

    // config
    private ApplicationConfiguration _applicationConfiguration;
    private List<AvailaibilityConfiguration> _availaibilityConfiguration;

    public UpController(ILogger<UpController> logger, ApplicationConfiguration applicationConfiguration, List<AvailaibilityConfiguration> availaibilityConfiguration)
    {
        _logger = logger;        
        _applicationConfiguration = applicationConfiguration;
        _availaibilityConfiguration = availaibilityConfiguration;
    }

    /// <summary>
    ///  This is what an availability check endpoint should look like. Simple. Just an "OK", response...
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    public ActionResult Up()
    {
        return Ok("UP");
    }

    [HttpGet("Log")]
    [AllowAnonymous]
    public ActionResult LogTest(string logKey)
    {
        if (String.IsNullOrWhiteSpace(_applicationConfiguration.LogKey) || String.IsNullOrWhiteSpace(logKey))
        {
            return NotFound();
        }

        if (_applicationConfiguration.LogKey != logKey)
        {
            return NotFound();
        }

        // log
        _logger.LogDebug("[+] Log.Debug(), Log Test");
        _logger.LogInformation("[+] Log.Information(), Log Test");
        _logger.LogWarning("[+] Log.Warning(), Log Test");
        _logger.LogError("[+] Log.Error(), Log Test");

        return Ok("OK");
    }
}
