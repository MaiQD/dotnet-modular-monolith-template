using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using App.Modules.Users.Domain.Entities;
using App.Modules.Users.Domain.Repositories;
using App.Modules.Users.Infrastructure.Persistence;
using App.SharedKernel.Results;

namespace App.Modules.Users.Infrastructure.Repositories;

public class UserMetricsRepository : IUserMetricsRepository
{
    private readonly UsersDbContext _db;
    private readonly ILogger<UserMetricsRepository> _logger;

    public UserMetricsRepository(UsersDbContext db, ILogger<UserMetricsRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<UserMetric>> CreateAsync(UserMetric userMetric, CancellationToken cancellationToken = default)
    {
        try
        {
            await _db.UserMetrics.AddAsync(userMetric, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success(userMetric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user metric for user {UserId}. Error: {ErrorMessage}", userMetric.UserId, ex.Message);
            return Result.Failure<UserMetric>($"Failed to create user metric: {ex.Message}");
        }
    }

    public async Task<Result<UserMetric>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userMetric = await _db.UserMetrics.AsNoTracking().FirstOrDefaultAsync(um => um.Id == id, cancellationToken);
            return (userMetric != null 
                ? Result.Success(userMetric) 
                : Result.Failure<UserMetric>("User metric not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user metric by ID {UserMetricId}. Error: {ErrorMessage}", id, ex.Message);
            return Result.Failure<UserMetric>($"Failed to get user metric: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserMetric>>> GetByUserIdAsync(string userId, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var userMetrics = await _db.UserMetrics
                .AsNoTracking()
                .Where(um => um.UserId == userId)
                .OrderByDescending(um => um.Date)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
            
            return Result.Success(userMetrics.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user metrics for user {UserId} with skip {Skip} and take {Take}. Error: {ErrorMessage}", userId, skip, take, ex.Message);
            return Result.Failure<IEnumerable<UserMetric>>($"Failed to get user metrics: {ex.Message}");
        }
    }

    public async Task<Result<UserMetric>> GetLatestByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userMetric = await _db.UserMetrics
                .AsNoTracking()
                .Where(um => um.UserId == userId)
                .OrderByDescending(um => um.Date)
                .FirstOrDefaultAsync(cancellationToken);
            
            return userMetric != null 
                ? Result.Success(userMetric) 
                : Result.Failure<UserMetric>("No user metrics found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get latest user metric for user {UserId}. Error: {ErrorMessage}", userId, ex.Message);
            return Result.Failure<UserMetric>($"Failed to get latest user metric: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserMetric>>> GetByUserIdAndDateRangeAsync(
        string userId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userMetrics = await _db.UserMetrics
                .AsNoTracking()
                .Where(um => um.UserId == userId && um.Date >= startDate && um.Date <= endDate)
                .OrderByDescending(um => um.Date)
                .ToListAsync(cancellationToken);
            
            return Result.Success<IEnumerable<UserMetric>>(userMetrics.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user metrics by date range for user {UserId} from {StartDate} to {EndDate}. Error: {ErrorMessage}", userId, startDate, endDate, ex.Message);
            return Result.Failure<IEnumerable<UserMetric>>($"Failed to get user metrics by date range: {ex.Message}");
        }
    }

    public async Task<Result<UserMetric>> GetByUserIdAndDateAsync(
        string userId, 
        DateTime date, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userMetric = await _db.UserMetrics
                .AsNoTracking()
                .FirstOrDefaultAsync(um => um.UserId == userId && um.Date.Date == date.Date, cancellationToken);
            
            return userMetric != null 
                ? Result.Success(userMetric) 
                : Result.Failure<UserMetric>("User metric not found for the specified date");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user metric by date for user {UserId} on {Date}. Error: {ErrorMessage}", userId, date, ex.Message);
            return Result.Failure<UserMetric>($"Failed to get user metric by date: {ex.Message}");
        }
    }

    public async Task<Result<UserMetric>> UpdateAsync(UserMetric userMetric, CancellationToken cancellationToken = default)
    {
        try
        {
            userMetric.UpdatedAt = DateTime.UtcNow;
            _db.UserMetrics.Update(userMetric);
            var count = await _db.SaveChangesAsync(cancellationToken);
            return count > 0 ? Result.Success(userMetric) : Result.Failure<UserMetric>("User metric not found or not modified");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user metric with ID {UserMetricId}. Error: {ErrorMessage}", userMetric.Id, ex.Message);
            return Result.Failure<UserMetric>($"Failed to update user metric: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _db.UserMetrics.FirstOrDefaultAsync(um => um.Id == id, cancellationToken);
            if (entity == null) return Result.Failure("User metric not found");
            _db.UserMetrics.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user metric with ID {UserMetricId}. Error: {ErrorMessage}", id, ex.Message);
            return Result.Failure($"Failed to delete user metric: {ex.Message}");
        }
    }

    public async Task<Result<long>> GetCountByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _db.UserMetrics.LongCountAsync(um => um.UserId == userId, cancellationToken);
            return Result.Success(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user metrics count for user {UserId}. Error: {ErrorMessage}", userId, ex.Message);
            return Result.Failure<long>($"Failed to get user metrics count: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ExistsForUserAndDateAsync(
        string userId, 
        DateTime date, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _db.UserMetrics.AnyAsync(um => um.UserId == userId && um.Date.Date == date.Date, cancellationToken);
            return Result.Success(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check user metric existence for user {UserId} on {Date}. Error: {ErrorMessage}", userId, date, ex.Message);
            return Result.Failure<bool>($"Failed to check user metric existence: {ex.Message}");
        }
    }
}
