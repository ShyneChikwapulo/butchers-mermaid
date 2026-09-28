using ButchersMermaid.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ButchersMermaid.Tests;

/// <summary>
/// Creates AppDbContext instances against an in-memory SQLite database
/// that persists for the lifetime of this factory. Callers must dispose
/// the factory when finished — the connection is kept open to prevent
/// SQLite from discarding the in-memory store between contexts.
/// </summary>
public sealed class TestDbContextFactory : IDbContextFactory<AppDbContext>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public TestDbContextFactory()
    {
        // A shared in-memory connection: the same database is available to
        // every context created by this factory. Disposal of the factory
        // disposes the connection and the database goes away.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Create the schema once. EnsureCreated is appropriate here — we
        // don't need migrations in tests.
        using var context = new AppDbContext(_options);
        context.Database.EnsureCreated();
    }

    public AppDbContext CreateDbContext() => new(_options);

    public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(CreateDbContext());

    public void Dispose()
    {
        _connection.Dispose();
    }
}