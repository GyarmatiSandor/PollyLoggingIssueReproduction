# Polly telemetry logging reproduction

The goal of this repository is to demonstrate a **default Polly telemetry behavior** that I believe has security implications and should be reconsidered.

## Issue description

When Polly Telemetry is enabled, Polly starts logging events such as:
- `ExecutionAttempt`
- `ResiliencePipelineExecuted`

By default, these events are logged at the **Information** level and **include the result of the resiliency pipeline execution**.

Because pipeline results can contain sensitive or confidential data (e.g., domain objects, PII, tokens), logging them at Information level by default can be a security concern.

This repository provides a minimal ASP.NET Core sample that shows this behavior using Polly’s default configuration (no special logs setup).

## Relevant code

- `Program.cs`
  Registers a Polly `ResiliencePipeline` and enables Polly Telemetry using the default configuration.
- `WeatherForecastController.cs`
  Uses the registered ResiliencePipeline to execute the `/weatherforecast` endpoint in a retry pipeline. The underlying operation sometimes fails to trigger retries and produce more telemetry events.

## How to reproduce

1. Run the application
```
dotnet run
``` 
2. Call one of the endpoints
For example:
```
curl http://localhost:5276/weatherforecast
```
3. Observe logs
In the application output, look for entries similar to:
- `ExecutionAttempt`
- `ResiliencePipelineExecuted`

You should see that these log entries, at Information level, include the result object returned by the pipeline.

<img width="3789" height="152" alt="image" src="https://github.com/user-attachments/assets/e622d503-5d64-4510-aa1c-0ab08509a339" />

This behavior occurs without any explicit opt-in to logging result payloads, and that is the core concern this repository is meant to highlight.
