using Riok.Mapperly.Abstractions;
using App.Modules.Users.Domain.Entities;
using App.Modules.Users.Application.DTOs;

namespace App.Modules.Users.Application.Mappers;

[Mapper]
public partial class UserMetricMapper
{
    [MapProperty(nameof(@MapBmiCategory), nameof(UserMetricDto.BmiCategory))]
    public partial UserMetricDto ToDto(UserMetric userMetric);
    
    private string MapBmiCategory(UserMetric userMetric) => userMetric.GetBmiCategory();
}
