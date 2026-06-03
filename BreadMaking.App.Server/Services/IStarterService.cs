using BreadMaking.App.Shared.Dtos;

namespace BreadMaking.App.Server.Services;

public interface IStarterService
{
    Task<List<StarterDto>>       GetAllAsync();
    Task<StarterDto?>            GetAsync(int id);
    Task<StarterDto>             CreateAsync(CreateStarterRequest req);
    Task<StarterFeedLogDto>      LogFeedAsync(int starterId, LogFeedRequest req);
    Task<List<StarterFeedLogDto>> GetFeedsAsync(int starterId);
    Task<List<StarterFeedLogDto>> GetRecentFeedsAsync(int count = 5);
}
