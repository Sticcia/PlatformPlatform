using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using StackFrame = Microsoft.ApplicationInsights.DataContracts.StackFrame;

namespace PlatformPlatform.SharedKernel.ApiCore.Endpoints;

public static class TrackEndpoints
{
    public static void MapTrackEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/track", Track);
    }

    private static TrackResponseSuccessDto Track(
        List<TrackRequestDto> trackRequests,
        TelemetryClient telemetryClient,
        ILogger<string> logger
    )
    {
        foreach (var trackRequestDto in trackRequests)
        {
            switch (trackRequestDto.Data.BaseType)
            {
                case "PageviewData":
                {
                    var telemetry = new PageViewTelemetry
                    {
                        Name = trackRequestDto.Data.BaseData.Name,
                        Url = new Uri(trackRequestDto.Data.BaseData.Url),
                        Duration = trackRequestDto.Data.BaseData.Duration,
                        Timestamp = trackRequestDto.Time,
                        Id = trackRequestDto.Data.BaseData.Id
                    };

                    CopyContextTags(telemetry.Context, trackRequestDto);
                    CopyDictionary(trackRequestDto.Data.BaseData.Properties, telemetry.Properties);
                    CopyDictionary(trackRequestDto.Data.BaseData.Measurements, telemetry.Metrics);

                    telemetryClient.TrackPageView(telemetry);
                    break;
                }
                case "PageviewPerformanceData":
                {
                    var telemetry = new PageViewPerformanceTelemetry
                    {
                        Name = trackRequestDto.Data.BaseData.Name,
                        Url = new Uri(trackRequestDto.Data.BaseData.Url),
                        Duration = trackRequestDto.Data.BaseData.Duration,
                        Timestamp = trackRequestDto.Time,
                        Id = trackRequestDto.Data.BaseData.Id,
                        PerfTotal = trackRequestDto.Data.BaseData.PerfTotal,
                        NetworkConnect = trackRequestDto.Data.BaseData.NetworkConnect,
                        SentRequest = trackRequestDto.Data.BaseData.SentRequest,
                        ReceivedResponse = trackRequestDto.Data.BaseData.ReceivedResponse,
                        DomProcessing = trackRequestDto.Data.BaseData.DomProcessing
                    };

                    CopyContextTags(telemetry.Context, trackRequestDto);
                    CopyDictionary(trackRequestDto.Data.BaseData.Properties, telemetry.Properties);
                    CopyDictionary(trackRequestDto.Data.BaseData.Measurements, telemetry.Metrics);

                    telemetryClient.Track(telemetry);
                    break;
                }
                case "ExceptionData":
                {
                    var exceptionDetailsInfos = GetExceptionDetailsInfos(trackRequestDto);
                    var telemetry = new ExceptionTelemetry(exceptionDetailsInfos,
                        trackRequestDto.Data.BaseData.SeverityLevel, trackRequestDto.Data.BaseType,
                        trackRequestDto.Data.BaseData.Properties, new Dictionary<string, double>())
                    {
                        SeverityLevel = trackRequestDto.Data.BaseData.SeverityLevel,
                        Timestamp = trackRequestDto.Time
                    };

                    CopyContextTags(telemetry.Context, trackRequestDto);
                    CopyDictionary(trackRequestDto.Data.BaseData.Properties, telemetry.Properties);
                    CopyDictionary(trackRequestDto.Data.BaseData.Measurements, telemetry.Metrics);

                    telemetryClient.TrackException(telemetry);
                    break;
                }
                case "MetricData":
                {
                    var metric = trackRequestDto.Data.BaseData.Metrics[0];
                    var telemetry = new MetricTelemetry
                    {
                        Name = metric.Name,
                        Sum = metric.Value,
                        Count = metric.Count,
                        Timestamp = trackRequestDto.Time
                    };

                    CopyContextTags(telemetry.Context, trackRequestDto);
                    CopyDictionary(trackRequestDto.Data.BaseData.Properties, telemetry.Properties);

                    telemetryClient.TrackMetric(telemetry);
                    break;
                }
                case "RemoteDependencyData":
                    // Ignore remote dependency data
                    break;
                default:
                    logger.LogWarning($"Unsupported telemetry type: {trackRequestDto.Data.BaseType}");
                    break;
            }
        }

        return new TrackResponseSuccessDto(true, "Telemetry sent.");
    }

    private static IEnumerable<ExceptionDetailsInfo> GetExceptionDetailsInfos(TrackRequestDto trackRequestDto)
    {
        var exceptionDetailsInfos = trackRequestDto.Data.BaseData.Exceptions
            .Select(exception => new ExceptionDetailsInfo(
                0,
                0,
                exception.TypeName,
                exception.Message,
                exception.HasFullStack,
                exception.Stack,
                exception.ParsedStack.Select(parsedStack => new StackFrame(
                    parsedStack.Assembly,
                    parsedStack.FileName,
                    parsedStack.Level,
                    parsedStack.Line,
                    parsedStack.Method
                ))
            ));
        return exceptionDetailsInfos;
    }

    private static void CopyContextTags(TelemetryContext context, TrackRequestDto trackRequestDto)
    {
        context.User.Id = trackRequestDto.Tags["ai.user.id"];
        context.Session.Id = trackRequestDto.Tags["ai.session.id"];
        context.Device.Id = trackRequestDto.Tags["ai.device.id"];
        context.Device.Type = trackRequestDto.Tags["ai.device.type"];
        context.Operation.Name = trackRequestDto.Tags["ai.operation.name"];
        context.Operation.Id = trackRequestDto.Tags["ai.operation.id"];
        context.GetInternalContext().SdkVersion = trackRequestDto.Tags["ai.internal.sdkVersion"];
    }

    private static void CopyDictionary<TValue>(IDictionary<string, TValue>? source, IDictionary<string, TValue> target)
    {
        if (source == null) return;

        foreach (var pair in source)
        {
            if (string.IsNullOrEmpty(pair.Key) || target.ContainsKey(pair.Key)) continue;
            target[pair.Key] = pair.Value;
        }
    }
}

[UsedImplicitly]
public record TrackResponseSuccessDto(bool Success, string Message);

[UsedImplicitly]
public record TrackRequestDto(
    DateTimeOffset Time,
    // ReSharper disable once InconsistentNaming
    string IKey,
    string Name,
    Dictionary<string, string> Tags,
    TrackRequestDataDto Data
);

[UsedImplicitly]
public record TrackRequestDataDto(string BaseType, TrackRequestBaseDataDto BaseData);

[UsedImplicitly]
public record TrackRequestBaseDataDto(
    string Name,
    string Url,
    TimeSpan Duration,
    TimeSpan PerfTotal,
    TimeSpan NetworkConnect,
    TimeSpan SentRequest,
    TimeSpan ReceivedResponse,
    TimeSpan DomProcessing,
    Dictionary<string, string> Properties,
    Dictionary<string, double> Measurements,
    List<TrackRequestMetricsDto> Metrics,
    List<TrackRequestExceptionDto> Exceptions,
    SeverityLevel SeverityLevel,
    string Id
);

[UsedImplicitly]
public record TrackRequestMetricsDto(string Name, int Kind, double Value, int Count);

[UsedImplicitly]
public record TrackRequestExceptionDto(
    string TypeName,
    string Message,
    bool HasFullStack,
    string Stack,
    List<TrackRequestParsedStackDto> ParsedStack
);

[UsedImplicitly]
public record TrackRequestParsedStackDto(string Assembly, string FileName, string Method, int Line, int Level);