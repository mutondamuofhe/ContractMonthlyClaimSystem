using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class ClaimServiceTests
{
    [Fact]
    public async Task CreateClaim_SavesClaimWithPendingStatus()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()

            .Options;

        using var context = new ApplicationDbContext(options);
        var claim = new ClaimRecord { LecturerId = "user1", HoursWorked = 4m, HourlyRate = 200m, Notes = "Test" };
        context.ClaimRecords.Add(claim);
        await context.SaveChangesAsync();

        var saved = await context.ClaimRecords.FirstOrDefaultAsync(c => c.LecturerId == "user1");

    }
}
