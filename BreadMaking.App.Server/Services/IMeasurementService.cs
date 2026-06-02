using BreadMaking.App.Shared.Dtos;

namespace BreadMaking.App.Server.Services;

public interface IMeasurementService
{
    Task<MeasurementDto> AddAsync(int bakeStepLogId, AddMeasurementRequest request);
}
