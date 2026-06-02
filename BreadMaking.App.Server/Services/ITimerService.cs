using BreadMaking.App.Shared.Dtos;

namespace BreadMaking.App.Server.Services;

public interface ITimerService
{
    Task<BakeStepLogDto> StartAsync(int bakeStepLogId);
    Task<BakeStepLogDto> PauseAsync(int bakeStepLogId);
    Task<BakeStepLogDto> CompleteAsync(int bakeStepLogId);
    Task<BakeStepLogDto> AdjustPlannedAsync(int bakeStepLogId, int deltaMinutes);
}
