using Dapper;
using Npgsql;
using Planner.Api.Models;

namespace Planner.Api.Repositories;

public sealed class SlotRepository(NpgsqlDataSource dataSource) : ISlotRepository
{
    public async Task<IEnumerable<Slot>> GetSlotsAsync(
        DateOnly startDate,
        DateOnly endDate,
        string? userEmail = null,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                s.id,
                s.date,
                s.start_time   AS starttime,
                s.end_time     AS endtime,
                s.title,
                s.description,
                s.capacity,
                COUNT(b.id)::int AS bookedcount,
                CASE
                    WHEN COUNT(b.id) >= s.capacity THEN 'full'
                    WHEN COUNT(b.id) > 0           THEN 'booked'
                    ELSE 'available'
                END AS status,
                COALESCE(bool_or(b.email = @UserEmail), FALSE) AS isbookedbyMe,
                MAX(CASE WHEN b.email = @UserEmail THEN b.id::text END)::uuid AS bookingid
            FROM slots s
            LEFT JOIN bookings b ON b.slot_id = s.id
            WHERE s.date BETWEEN @StartDate AND @EndDate
            GROUP BY s.id
            ORDER BY s.date, s.start_time
            """;

        await using var conn = await dataSource.OpenConnectionAsync(ct);
        var rows = await conn.QueryAsync<SlotRow>(sql, new
        {
            StartDate = startDate,
            EndDate = endDate,
            UserEmail = userEmail ?? string.Empty
        });
        return rows.Select(MapRow);
    }

    public async Task<Slot?> GetSlotByIdAsync(
        Guid id,
        string? userEmail = null,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                s.id,
                s.date,
                s.start_time   AS starttime,
                s.end_time     AS endtime,
                s.title,
                s.description,
                s.capacity,
                COUNT(b.id)::int AS bookedcount,
                CASE
                    WHEN COUNT(b.id) >= s.capacity THEN 'full'
                    WHEN COUNT(b.id) > 0           THEN 'booked'
                    ELSE 'available'
                END AS status,
                COALESCE(bool_or(b.email = @UserEmail), FALSE) AS isbookedbyMe,
                MAX(CASE WHEN b.email = @UserEmail THEN b.id::text END)::uuid AS bookingid
            FROM slots s
            LEFT JOIN bookings b ON b.slot_id = s.id
            WHERE s.id = @Id
            GROUP BY s.id
            """;

        await using var conn = await dataSource.OpenConnectionAsync(ct);
        var row = await conn.QueryFirstOrDefaultAsync<SlotRow>(sql, new
        {
            Id = id,
            UserEmail = userEmail ?? string.Empty
        });
        return row is null ? null : MapRow(row);
    }

    public async Task<Slot> UpdateBookedCountAsync(Guid slotId, CancellationToken ct = default)
    {
        var slot = await GetSlotByIdAsync(slotId, ct: ct)
            ?? throw new KeyNotFoundException($"Slot {slotId} not found.");
        return slot;
    }

    // ── Mapping interne ───────────────────────────────────────────────────────

    private sealed class SlotRow
    {
        public Guid Id { get; init; }
        public DateOnly Date { get; init; }
        public TimeOnly StartTime { get; init; }
        public TimeOnly EndTime { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public int Capacity { get; init; }
        public int BookedCount { get; init; }
        public string Status { get; init; } = "available";
        public bool IsBookedByMe { get; init; }
        public Guid? BookingId { get; init; }
    }

    private static Slot MapRow(SlotRow r) => new()
    {
        Id = r.Id,
        Date = r.Date,
        StartTime = r.StartTime,
        EndTime = r.EndTime,
        Title = r.Title,
        Description = r.Description,
        Capacity = r.Capacity,
        BookedCount = r.BookedCount,
        Status = r.Status switch
        {
            "full"      => SlotStatus.Full,
            "booked"    => SlotStatus.Booked,
            _           => SlotStatus.Available
        },
        IsBookedByMe = r.IsBookedByMe,
        BookingId = r.BookingId
    };
}
