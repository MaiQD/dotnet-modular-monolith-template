using MediatR;
using App.Modules.Users.Application.DTOs;
using App.SharedKernel.Results;
using System.ComponentModel.DataAnnotations;

namespace App.Modules.Users.Application.Queries;

public class GetLatestUserMetricQuery(string userId) : IRequest<Result<UserMetricDto>>
{
    public GetLatestUserMetricQuery() : this(string.Empty)
    {
    }
    [Required]
    public string UserId { get; set; } = userId;
};