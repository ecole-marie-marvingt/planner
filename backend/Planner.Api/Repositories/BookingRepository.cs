using Dapper;
using Npgsql;
using Planner.Api.Models;

namespace Planner.Api.Repositories;

public sealed class BookingRepository(NpgsqlDataSource dataSource) : IBookingRepository
{
    public async Task<Booking?> GetBookingAsync(Guid bookingId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id AS bookingid, slot_id AS slotid, user_name AS username,
                   email, booked_at AS bookedat
            FROM bookings
            WHERE id = @BookingId
            """;

        await using var conn = await dataSource.OpenConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<Booking>(sql, new { BookingId = bookingId });
    }

    public async Task<Booking> CreateBookingAsync(
        Guid slotId,
        string userName,
        string email,
        CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO bookings (id, slot_id, user_name, email, booked_at)
            VALUES (@Id, @SlotId, @UserName, @Email, @BookedAt)
            RETURNING id AS bookingid, slot_id AS slotid, user_name AS username,
                      email, booked_at AS bookedat
            """;

        var param = new
        {
            Id = Guid.NewGuid(),
            SlotId = slotId,
            UserName = userName,
            Email = email,
            BookedAt = DateTimeOffset.UtcNow
        };

        await using var conn = await dataSource.OpenConnectionAsync(ct);
        return await conn.QuerySingleAsync<Booking>(sql, param);
    }

    public async Task DeleteBookingAsync(Guid bookingId, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM bookings WHERE id = @BookingId";
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync(sql, new { BookingId = bookingId });
    }

    public async Task<bool> ExistsAsync(Guid slotId, string email, CancellationToken ct = default)
    {
        const string sql = """
            SELECT EXISTS(
                SELECT 1 FROM bookings WHERE slot_id = @SlotId AND email = @Email
            )
            """;
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        return await conn.ExecuteScalarAsync<bool>(sql, new { SlotId = slotId, Email = email });
    }
}
