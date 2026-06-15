namespace BreadMaking.App.Models;

public record GrainProfile(
    string DisplayName,
    string Icon,
    string Description,
    bool IsGlutenFree,
    bool IsEnriched,
    bool IsLowGlutenAncient,
    int MaxRestMinutes,
    int SoakerMinutes,
    string HydrationNote,
    string MixingNote,
    bool IsSteamed = false
);
