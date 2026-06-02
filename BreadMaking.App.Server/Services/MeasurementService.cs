using System.ComponentModel.DataAnnotations;
using BreadMaking.App.Server.Data;
using BreadMaking.App.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BreadMaking.App.Server.Services;

public class MeasurementService(AppDbContext db) : IMeasurementService
{
    public async Task<MeasurementDto> AddAsync(int bakeStepLogId, AddMeasurementRequest request)
    {
        // Verify the step log exists
        var logExists = await db.BakeStepLogs.AnyAsync(l => l.Id == bakeStepLogId);
        if (!logExists)
            throw new KeyNotFoundException($"BakeStepLog {bakeStepLogId} not found.");

        var type = await db.MeasurementTypes.FindAsync(request.MeasurementTypeId)
            ?? throw new KeyNotFoundException($"MeasurementType {request.MeasurementTypeId} not found.");

        if (type.MinValid.HasValue && request.Value < type.MinValid.Value)
            throw new ValidationException(
                $"{type.Name} must be ≥ {type.MinValid} {type.Unit}. Got {request.Value}.");

        if (type.MaxValid.HasValue && request.Value > type.MaxValid.Value)
            throw new ValidationException(
                $"{type.Name} must be ≤ {type.MaxValid} {type.Unit}. Got {request.Value}.");

        var measurement = new Measurement
        {
            BakeStepLogId     = bakeStepLogId,
            MeasurementTypeId = request.MeasurementTypeId,
            Value             = request.Value,
            Unit              = type.Unit,
            RecordedAt        = DateTimeOffset.UtcNow, // server-stamped — never client-supplied
        };

        db.Measurements.Add(measurement);
        await db.SaveChangesAsync();

        return new MeasurementDto
        {
            Id                = measurement.Id,
            MeasurementTypeId = measurement.MeasurementTypeId,
            TypeName          = type.Name,
            Value             = measurement.Value,
            Unit              = measurement.Unit,
            RecordedAt        = measurement.RecordedAt,
        };
    }
}
