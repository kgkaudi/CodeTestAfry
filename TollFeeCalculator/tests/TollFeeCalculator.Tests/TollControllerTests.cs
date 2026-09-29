using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TollFeeCalculator.Api.Controllers;
using TollFeeCalculator.Api.Models;
using TollFeeCalculator.Core;
using Xunit;

namespace TollFeeCalculator.Tests;

public class TollControllerTests
{
    // ValidationProblem() resolves ProblemDetailsFactory from HttpContext.RequestServices,
    // so the controller needs a minimal service provider even outside a real request pipeline.
    private static readonly IServiceProvider Services = BuildServiceProvider();

    private readonly TollController _sut = CreateController();

    private static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMvc();
        return services.BuildServiceProvider();
    }

    private static TollController CreateController() => new(new TollCalculator(FeeSchedule.Default, new Holiday2013Provider()))
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = Services }
        }
    };

    private static JsonElement VehicleType(string name) => JsonDocument.Parse($"\"{name}\"").RootElement;

    private static DateTime Mon(int h, int m) => new(2013, 3, 11, h, m, 0);

    [Fact]
    public void Valid_request_returns_200_with_the_calculated_fee()
    {
        var request = new CalculateRequest(VehicleType("Car"), new List<DateTime> { Mon(7, 30) });

        var result = _sut.Calculate(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<TollResult>(ok.Value);
        Assert.Equal(18, body.TotalFee);
    }

    [Fact]
    public void Unknown_vehicle_type_returns_400_with_vehicleType_error()
    {
        var request = new CalculateRequest(VehicleType("Bicycle"), new List<DateTime> { Mon(7, 30) });

        var result = _sut.Calculate(request);

        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        var details = Assert.IsType<ValidationProblemDetails>(problem.Value);
        Assert.True(details.Errors.ContainsKey(nameof(CalculateRequest.VehicleType)));
    }

    [Fact]
    public void Empty_passages_returns_400_with_passages_error()
    {
        var request = new CalculateRequest(VehicleType("Car"), new List<DateTime>());

        var result = _sut.Calculate(request);

        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        var details = Assert.IsType<ValidationProblemDetails>(problem.Value);
        Assert.True(details.Errors.ContainsKey(nameof(CalculateRequest.Passages)));
    }

    [Fact]
    public void Null_passages_returns_400_with_passages_error()
    {
        var request = new CalculateRequest(VehicleType("Car"), null);

        var result = _sut.Calculate(request);

        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        var details = Assert.IsType<ValidationProblemDetails>(problem.Value);
        Assert.True(details.Errors.ContainsKey(nameof(CalculateRequest.Passages)));
    }

    [Fact]
    public void Passage_outside_2013_returns_400_with_passages_error()
    {
        var request = new CalculateRequest(VehicleType("Car"), new List<DateTime> { new(2014, 3, 11, 7, 30, 0) });

        var result = _sut.Calculate(request);

        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        var details = Assert.IsType<ValidationProblemDetails>(problem.Value);
        Assert.True(details.Errors.ContainsKey(nameof(CalculateRequest.Passages)));
    }

    [Fact]
    public void Both_fields_invalid_returns_both_errors()
    {
        var request = new CalculateRequest(VehicleType("Bicycle"), new List<DateTime>());

        var result = _sut.Calculate(request);

        var problem = Assert.IsType<BadRequestObjectResult>(result.Result);
        var details = Assert.IsType<ValidationProblemDetails>(problem.Value);
        Assert.True(details.Errors.ContainsKey(nameof(CalculateRequest.VehicleType)));
        Assert.True(details.Errors.ContainsKey(nameof(CalculateRequest.Passages)));
    }
}
