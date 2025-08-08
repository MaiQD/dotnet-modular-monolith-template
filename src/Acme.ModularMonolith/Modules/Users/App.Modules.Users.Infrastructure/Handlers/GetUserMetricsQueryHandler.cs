using MediatR;
using Microsoft.Extensions.Logging;
using App.Modules.Users.Application.Queries;
using App.Modules.Users.Application.DTOs;
using App.Modules.Users.Application.Mappers;
using App.Modules.Users.Domain.Repositories;
using App.SharedKernel.Results;

namespace App.Modules.Users.Infrastructure.Handlers;

public class GetUserMetricsQueryHandler : IRequestHandler<GetUserMetricsQuery, Result<IEnumerable<UserMetricDto>>>
{
    private readonly IUserMetricsRepository _userMetricsRepository;
    private readonly UserMetricMapper _userMetricMapper;
    private readonly ILogger<GetUserMetricsQueryHandler> _logger;

    public GetUserMetricsQueryHandler(
        IUserMetricsRepository userMetricsRepository,
        UserMetricMapper userMetricMapper,
        ILogger<GetUserMetricsQueryHandler> logger)
    {
        _userMetricsRepository = userMetricsRepository;
        _userMetricMapper = userMetricMapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<UserMetricDto>>> Handle(GetUserMetricsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Result<IEnumerable<Domain.Entities.UserMetric>> metricsResult;

            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                // Get metrics by date range
                metricsResult = await _userMetricsRepository.GetByUserIdAndDateRangeAsync(
                    request.UserId, request.StartDate.Value, request.EndDate.Value, cancellationToken);
            }
            else
            {
                // Get metrics with pagination
                metricsResult = await _userMetricsRepository.GetByUserIdAsync(
                    request.UserId, request.Skip, request.Take, cancellationToken);
            }

            if (metricsResult.IsFailure)
            {
                return Result.Failure<IEnumerable<UserMetricDto>>(metricsResult.Error!);
            }

            var metrics = metricsResult.Value!;
            var metricDtos = metrics.Select(m => _userMetricMapper.ToDto(m)).ToList();

            return Result.Success<IEnumerable<UserMetricDto>>(metricDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user metrics for user {UserId}", request.UserId);
            return Result.Failure<IEnumerable<UserMetricDto>>($"Failed to get user metrics: {ex.Message}");
        }
    }
}
