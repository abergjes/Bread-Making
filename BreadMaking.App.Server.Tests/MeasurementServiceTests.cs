using BreadMaking.App.Server.Data;
using BreadMaking.App.Server.Services;
using BreadMaking.App.Shared;
using BreadMaking.App.Shared.Dtos;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BreadMaking.App.Server.Tests;

public class MeasurementServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly int _logId;

    public MeasurementServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        var bake = new Bake { RecipeId = 1, StartedAt = DateTimeOffset.UtcNow };
        _db.Bakes.Add(bake);
        _db.SaveChanges();

        var log = new BakeStepLog
        {
            BakeId             = bake.Id,
            RecipeStepId       = 101,
            PlannedDurationMin = 5,
            Status             = StepStatus.Running,
        };
        _db.BakeStepLogs.Add(log);
        _db.SaveChanges();
        _logId = log.Id;

        _db.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // ── Happy-path ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Add_ValidDoughTemp_ReturnsDtoWithCorrectFields()
    {
        var svc = new MeasurementService(_db, new NullNotificationService());
        var dto = await svc.AddAsync(_logId, new AddMeasurementRequest { MeasurementTypeId = 1, Value = 24.0 });

        Assert.Equal(1, dto.MeasurementTypeId);
        Assert.Equal("Dough temp", dto.TypeName);
        Assert.Equal(24.0, dto.Value);
        Assert.Equal("°C", dto.Unit);
    }

    [Fact]
    public async Task Add_ValidValue_StampsRecordedAtServerSide()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var svc = new MeasurementService(_db, new NullNotificationService());
        var dto = await svc.AddAsync(_logId, new AddMeasurementRequest { MeasurementTypeId = 2, Value = 55.0 });
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        Assert.InRange(dto.RecordedAt, before, after);
    }

    [Fact]
    public async Task Add_ValidValue_PersistsToDatabase()
    {
        var svc = new MeasurementService(_db, new NullNotificationService());
        await svc.AddAsync(_logId, new AddMeasurementRequest { MeasurementTypeId = 1, Value = 25.5 });

        var count = await _db.Measurements.CountAsync(m => m.BakeStepLogId == _logId);
        Assert.Equal(1, count);
    }

    // ── Validation ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Add_PhAboveMax_ThrowsValidationException()
    {
        // MeasurementType 3 = pH, MaxValid = 7.0
        var svc = new MeasurementService(_db, new NullNotificationService());
        await Assert.ThrowsAsync<ValidationException>(() =>
            svc.AddAsync(_logId, new AddMeasurementRequest { MeasurementTypeId = 3, Value = 14.0 }));
    }

    [Fact]
    public async Task Add_PhBelowMin_ThrowsValidationException()
    {
        // MeasurementType 3 = pH, MinValid = 3.0
        var svc = new MeasurementService(_db, new NullNotificationService());
        await Assert.ThrowsAsync<ValidationException>(() =>
            svc.AddAsync(_logId, new AddMeasurementRequest { MeasurementTypeId = 3, Value = 1.0 }));
    }

    [Fact]
    public async Task Add_ValidationError_MessageContainsExpectedRange()
    {
        var svc = new MeasurementService(_db, new NullNotificationService());
        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            svc.AddAsync(_logId, new AddMeasurementRequest { MeasurementTypeId = 3, Value = 14.0 }));

        Assert.Contains("7", ex.Message); // mentions max valid
        Assert.Contains("pH", ex.Message);
    }

    [Fact]
    public async Task Add_DoughTempBelowMin_ThrowsValidationException()
    {
        // MeasurementType 1 = Dough temp, MinValid = 10
        var svc = new MeasurementService(_db, new NullNotificationService());
        await Assert.ThrowsAsync<ValidationException>(() =>
            svc.AddAsync(_logId, new AddMeasurementRequest { MeasurementTypeId = 1, Value = 5.0 }));
    }

    // ── Not-found guards ─────────────────────────────────────────────────────

    [Fact]
    public async Task Add_UnknownStepLogId_ThrowsKeyNotFoundException()
    {
        var svc = new MeasurementService(_db, new NullNotificationService());
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.AddAsync(99999, new AddMeasurementRequest { MeasurementTypeId = 1, Value = 24.0 }));
    }

    [Fact]
    public async Task Add_UnknownMeasurementTypeId_ThrowsKeyNotFoundException()
    {
        var svc = new MeasurementService(_db, new NullNotificationService());
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.AddAsync(_logId, new AddMeasurementRequest { MeasurementTypeId = 99, Value = 24.0 }));
    }

    // ── Boundary values ──────────────────────────────────────────────────────

    [Fact]
    public async Task Add_ValueAtExactMin_Succeeds()
    {
        // pH min = 3.0
        var svc = new MeasurementService(_db, new NullNotificationService());
        var dto = await svc.AddAsync(_logId, new AddMeasurementRequest { MeasurementTypeId = 3, Value = 3.0 });
        Assert.Equal(3.0, dto.Value);
    }

    [Fact]
    public async Task Add_ValueAtExactMax_Succeeds()
    {
        // pH max = 7.0
        var svc = new MeasurementService(_db, new NullNotificationService());
        var dto = await svc.AddAsync(_logId, new AddMeasurementRequest { MeasurementTypeId = 3, Value = 7.0 });
        Assert.Equal(7.0, dto.Value);
    }
}
