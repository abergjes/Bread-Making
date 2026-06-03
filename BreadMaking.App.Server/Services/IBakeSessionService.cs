using BreadMaking.App.Shared.Dtos;

namespace BreadMaking.App.Server.Services;

public interface IBakeSessionService
{
    Task<BakeDto> CreateFromRequestAsync(StartBakeRequest request);
    Task<BakeDto?> GetBakeAsync(int id);
    Task<List<BakeListItemDto>> GetBakeListAsync();
    Task<StartBakeRequest?> GetBakeInputsAsync(int id);
    Task<bool> UpdateNotesAsync(int id, string? notes);
    Task<bool> SaveOutcomeAsync(int bakeId, BakeOutcomeDto dto);
}
