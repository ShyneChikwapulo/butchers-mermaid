using ButchersMermaid.Data;
using ButchersMermaid.Data.Entities;
using ButchersMermaid.Services;

namespace ButchersMermaid.Tests.Services;

public class EnquiryRepositoryTests
{
    [Fact]
    public async Task AddAsync_PersistsEnquiry_AndSetsSubmittedAt()
    {
        using var factory = new TestDbContextFactory();
        var repo = new EnquiryRepository(factory);
        var before = DateTime.UtcNow;

        var enquiry = new Enquiry
        {
            Type = EnquiryType.Contact,
            Name = "Test User",
            Email = "test@example.com",
            Message = "Hello there",
        };

        var saved = await repo.AddAsync(enquiry);
        var after = DateTime.UtcNow;

        Assert.True(saved.Id > 0);
        Assert.InRange(saved.SubmittedAt, before.AddSeconds(-1), after.AddSeconds(1));

        await using var db = await factory.CreateDbContextAsync();
        var fromDb = await db.Enquiries.FindAsync(saved.Id);
        Assert.NotNull(fromDb);
        Assert.Equal("Test User", fromDb!.Name);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsNewestFirst()
    {
        using var factory = new TestDbContextFactory();
        var repo = new EnquiryRepository(factory);

        await repo.AddAsync(new Enquiry
        {
            Type = EnquiryType.Contact, Name = "A", Email = "a@x.com",
            Message = "first",
            SubmittedAt = DateTime.UtcNow.AddMinutes(-10),
        });
        await repo.AddAsync(new Enquiry
        {
            Type = EnquiryType.Contact, Name = "B", Email = "b@x.com",
            Message = "second",
        });
        await repo.AddAsync(new Enquiry
        {
            Type = EnquiryType.Contact, Name = "C", Email = "c@x.com",
            Message = "third",
        });

        var all = await repo.GetAllAsync();

        Assert.Equal(3, all.Count);
        // The AddAsync method overwrites SubmittedAt with UtcNow, so
        // ordering should be reverse-insertion.
        Assert.Equal("C", all[0].Name);
        Assert.Equal("A", all[^1].Name);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByType_WhenTypeProvided()
    {
        using var factory = new TestDbContextFactory();
        var repo = new EnquiryRepository(factory);

        await repo.AddAsync(new Enquiry
        {
            Type = EnquiryType.Contact, Name = "Contact 1", Email = "c1@x.com", Message = "x",
        });
        await repo.AddAsync(new Enquiry
        {
            Type = EnquiryType.Wholesale, Name = "Wholesale 1", Email = "w1@x.com", Message = "x",
        });
        await repo.AddAsync(new Enquiry
        {
            Type = EnquiryType.Contact, Name = "Contact 2", Email = "c2@x.com", Message = "x",
        });

        var contacts = await repo.GetAllAsync(EnquiryType.Contact);
        var wholesale = await repo.GetAllAsync(EnquiryType.Wholesale);

        Assert.Equal(2, contacts.Count);
        Assert.Single(wholesale);
        Assert.All(contacts, e => Assert.Equal(EnquiryType.Contact, e.Type));
    }

    [Fact]
    public async Task GetUnreadCountAsync_CountsUnreadEnquiries()
    {
        using var factory = new TestDbContextFactory();
        var repo = new EnquiryRepository(factory);

        var e1 = await repo.AddAsync(new Enquiry
        {
            Type = EnquiryType.Contact, Name = "A", Email = "a@x.com", Message = "x",
        });
        await repo.AddAsync(new Enquiry
        {
            Type = EnquiryType.Contact, Name = "B", Email = "b@x.com", Message = "x",
        });
        var e3 = await repo.AddAsync(new Enquiry
        {
            Type = EnquiryType.Contact, Name = "C", Email = "c@x.com", Message = "x",
        });

        await repo.MarkReadAsync(e1.Id, true);
        await repo.MarkReadAsync(e3.Id, true);

        var count = await repo.GetUnreadCountAsync();

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task MarkReadAsync_UpdatesIsReadFlag()
    {
        using var factory = new TestDbContextFactory();
        var repo = new EnquiryRepository(factory);
        var saved = await repo.AddAsync(new Enquiry
        {
            Type = EnquiryType.Contact, Name = "A", Email = "a@x.com", Message = "x",
        });
        Assert.False(saved.IsRead);

        var updated = await repo.MarkReadAsync(saved.Id, true);

        Assert.NotNull(updated);
        Assert.True(updated!.IsRead);

        var again = await repo.MarkReadAsync(saved.Id, false);
        Assert.NotNull(again);
        Assert.False(again!.IsRead);
    }

    [Fact]
    public async Task MarkReadAsync_ReturnsNull_WhenEnquiryDoesNotExist()
    {
        using var factory = new TestDbContextFactory();
        var repo = new EnquiryRepository(factory);

        var result = await repo.MarkReadAsync(9999, true);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEnquiry()
    {
        using var factory = new TestDbContextFactory();
        var repo = new EnquiryRepository(factory);
        var saved = await repo.AddAsync(new Enquiry
        {
            Type = EnquiryType.Contact, Name = "A", Email = "a@x.com", Message = "x",
        });

        var deleted = await repo.DeleteAsync(saved.Id);

        Assert.True(deleted);
        await using var db = await factory.CreateDbContextAsync();
        var fromDb = await db.Enquiries.FindAsync(saved.Id);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenEnquiryDoesNotExist()
    {
        using var factory = new TestDbContextFactory();
        var repo = new EnquiryRepository(factory);

        var deleted = await repo.DeleteAsync(9999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task UpdateEmailStatusAsync_PersistsSuccessAndError()
    {
        using var factory = new TestDbContextFactory();
        var repo = new EnquiryRepository(factory);
        var saved = await repo.AddAsync(new Enquiry
        {
            Type = EnquiryType.Contact, Name = "A", Email = "a@x.com", Message = "x",
        });

        await repo.UpdateEmailStatusAsync(saved.Id, false, "Simulated failure");

        await using var db = await factory.CreateDbContextAsync();
        var fromDb = await db.Enquiries.FindAsync(saved.Id);
        Assert.NotNull(fromDb);
        Assert.False(fromDb!.EmailSent);
        Assert.Equal("Simulated failure", fromDb.EmailError);
    }
}