using BreadMaking.App.Shared.Dtos;

namespace BreadMaking.App.Services;

public class CloneBakeState
{
    private StartBakeRequest? _pending;

    public bool HasPending => _pending is not null;

    public void Set(StartBakeRequest req) => _pending = req;

    public StartBakeRequest? TakeAndClear()
    {
        var req = _pending;
        _pending = null;
        return req;
    }
}
