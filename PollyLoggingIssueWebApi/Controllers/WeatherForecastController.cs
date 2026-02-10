using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Registry;

namespace PollyLoggingIssueWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

	private readonly ILogger<WeatherForecastController> _logger;
	private readonly ResiliencePipeline _pipeline;

	public WeatherForecastController(ILogger<WeatherForecastController> logger, IServiceProvider serviceProvider)
	{
		_logger = logger;
        var pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
        _pipeline = pipelineProvider.GetPipeline("Something");
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
		var result = _pipeline.Execute(static _ => RandomlyFailingOperation());
		_logger.LogInformation("Random operation result: {Result}", result);

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

	private static string RandomlyFailingOperation()
	{
		if (Random.Shared.NextDouble() < 0.3)
			throw new InvalidOperationException("Random failure");

		return "This is some very important customer data, that I want to process";
	}
}
