using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

[Authorize(Roles = "ProgrammeCoordinator,AcademicManager")]
[Route("[controller]/[action]")]
public class CoordinatorController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CoordinatorController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db; _userManager = userManager;
    }

    public async Task<IActionResult> Pending()
    {
        var claims = await _db.ClaimRecords
            .Include(c => c.Files)
            .Where(c => c.Status == ClaimStatus.Pending)
            .OrderBy(c => c.SubmittedAt)
            .ToListAsync();
        return View(claims);
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int id)
    {
        var claim = await _db.ClaimRecords.FirstOrDefaultAsync(c => c.Id == id);
        if (claim == null) return NotFound();
        claim.Status = ClaimStatus.Approved;
        claim.StatusUpdatedAt = System.DateTime.UtcNow;
        claim.StatusUpdatedBy = _userManager.GetUserId(User);
        await _db.SaveChangesAsync();
        return RedirectToAction("Pending");
    }

    [HttpPost]
    public async Task<IActionResult> Reject(int id)
    {
        var claim = await _db.ClaimRecords.FirstOrDefaultAsync(c => c.Id == id);
        if (claim == null) return NotFound();
        claim.Status = ClaimStatus.Rejected;
        claim.StatusUpdatedAt = System.DateTime.UtcNow;
        claim.StatusUpdatedBy = _userManager.GetUserId(User);
        await _db.SaveChangesAsync();
        return RedirectToAction("Pending");
    }
}
