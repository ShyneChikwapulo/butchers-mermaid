using ButchersMermaid.Data;
using ButchersMermaid.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ButchersMermaid.Services;

public interface IEnquiryRepository
{
    Task<Enquiry> AddAsync(Enquiry enquiry);
    Task<List<Enquiry>> GetAllAsync(EnquiryType? type = null);
    Task<int> GetUnreadCountAsync();
    Task<Enquiry?> MarkReadAsync(int id, bool isRead);
    Task<bool> DeleteAsync(int id);
    Task UpdateEmailStatusAsync(int id, bool sent, string? error);
}

public class EnquiryRepository : IEnquiryRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public EnquiryRepository(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<Enquiry> AddAsync(Enquiry enquiry)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        enquiry.SubmittedAt = DateTime.UtcNow;
        db.Enquiries.Add(enquiry);
        await db.SaveChangesAsync();
        return enquiry;
    }

    public async Task<List<Enquiry>> GetAllAsync(EnquiryType? type = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.Enquiries.AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(e => e.Type == type.Value);
        }

        return await query
            .OrderByDescending(e => e.SubmittedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Enquiries.CountAsync(e => !e.IsRead);
    }

    public async Task<Enquiry?> MarkReadAsync(int id, bool isRead)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var enquiry = await db.Enquiries.FindAsync(id);
        if (enquiry is null) return null;

        enquiry.IsRead = isRead;
        await db.SaveChangesAsync();
        return enquiry;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var enquiry = await db.Enquiries.FindAsync(id);
        if (enquiry is null) return false;

        db.Enquiries.Remove(enquiry);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task UpdateEmailStatusAsync(int id, bool sent, string? error)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var enquiry = await db.Enquiries.FindAsync(id);
        if (enquiry is null) return;

        enquiry.EmailSent = sent;
        enquiry.EmailError = error;
        await db.SaveChangesAsync();
    }
}