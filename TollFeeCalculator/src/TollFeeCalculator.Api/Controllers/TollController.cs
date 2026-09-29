using Microsoft.AspNetCore.Mvc;
using TollFeeCalculator.Api.Models;
using TollFeeCalculator.Api.Validation;
using TollFeeCalculator.Core;

namespace TollFeeCalculator.Api.Controllers;

[ApiController]
[Route("api/toll")]
public sealed class TollController : ControllerBase
{
    private readonly TollCalculator _calculator;

    public TollController(TollCalculator calculator) => _calculator = calculator;

    /// <summary>Calculates the toll fee for a vehicle's passages on one or more days in 2013.</summary>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(TollResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<TollResult> Calculate([FromBody] CalculateRequest request)
    {
        if (!VehicleTypeParser.TryParse(request.VehicleType, out var vehicleType))
            ModelState.AddModelError(nameof(request.VehicleType),
                $"Must be one of: {string.Join(", ", Enum.GetNames<VehicleType>())}.");

        if (request.Passages is null || request.Passages.Count == 0)
            ModelState.AddModelError(nameof(request.Passages), "At least one passage timestamp is required.");
        else if (request.Passages.Any(p => p.Year != Holiday2013Provider.SupportedYear))
            ModelState.AddModelError(nameof(request.Passages),
                $"Only passages in {Holiday2013Provider.SupportedYear} are supported.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        return Ok(_calculator.Calculate(vehicleType, request.Passages!));
    }
}