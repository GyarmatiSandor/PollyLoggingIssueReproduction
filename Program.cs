using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Targets;
using NLog.Config;
using Polly;
using Polly.Retry;

var loggerFactory = CreateLoggerFactory();
var logger = loggerFactory.CreateLogger("PollyDemo");

await RetryWithPollyExampleAsync(logger);

NLog.LogManager.Shutdown();

static ILoggerFactory CreateLoggerFactory()
{
	var config = new LoggingConfiguration();
	var consoleTarget = new ColoredConsoleTarget("console")
	{
		Layout = "${longdate}|${level:uppercase=true}|${logger}|${message} ${exception:format=tostring}"
	};
	config.AddRuleForAllLevels(consoleTarget);
	NLog.LogManager.Configuration = config;

	return LoggerFactory.Create(builder =>
	{
		builder.ClearProviders();
		builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
		builder.AddNLog();
	});
}

static async Task RetryWithPollyExampleAsync(Microsoft.Extensions.Logging.ILogger logger)
{
	var retryOptions = new RetryStrategyOptions
	{
		ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>(),
		MaxRetryAttempts = 3,
		Delay = TimeSpan.FromMilliseconds(200),
		BackoffType = DelayBackoffType.Linear,
		OnRetry = args =>
		{
			logger.LogWarning(args.Outcome.Exception, "Retry {Attempt} in {DelayMs}ms", args.AttemptNumber, args.RetryDelay.TotalMilliseconds);
			return default;
		}
	};

	var pipeline = new ResiliencePipelineBuilder()
		.AddRetry(retryOptions)
		.Build();

	try
	{
		var result = await pipeline.ExecuteAsync(static async _ => await RandomlyFailingOperationAsync());
		logger.LogInformation("Succeeded with result: {Result}", result);
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "Failed after retries");
	}
}

static Task<string> RandomlyFailingOperationAsync()
{
	// Randomly fail about ~60% of the time.
	if (Random.Shared.NextDouble() < 0.6)
		throw new InvalidOperationException("Random failure");

	return Task.FromResult("OK");
}
