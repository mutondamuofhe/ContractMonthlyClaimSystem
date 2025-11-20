using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ContractMonthlyClaimSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContractMonthlyClaimSystem_starter.Controllers
{
    public class ClaimCreateViewModel
    {
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public string Notes { get; set; }
        public IFormFileCollection Files { get; set; }
    }

    [Authorize(Roles = "Lecturer")]
    [Route("[controller]/[action]")]
    public class ClaimsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private object _context;

        public ClaimsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Claim model, IFormFile uploadedFile)
        {
            if (!ModelState.IsValid) return View(model);

            // Recalculate server-side (security)
            model.CalculatedAmount = model.HoursWorked * model.HourlyRate;
            model.SubmittedDate = DateTime.UtcNow;
            model.Status = "Pending";
            model.DocumentsUploaded = uploadedFile != null && uploadedFile.Length > 0;

            // (optional) Save file to wwwroot/uploads with unique name; link to claim record
            if (model.DocumentsUploaded)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(uploadedFile.FileName)}";
                using (var stream = System.IO.File.Create(Path.Combine(uploads, fileName)))
                    await uploadedFile.CopyToAsync(stream);

                // store filename(s) in a related table or a column (not shown)
            }

            _context.Add(model);
            await _context.SaveChangesAsync();

            // After saving, run the automated verification processor (see next section)
            var autoResult = ClaimProcessor.ProcessNewClaim(model, _context);
            if (autoResult == ClaimProcessorResult.AutoApproved)
            {
                // optionally show message
                TempData["Message"] = "Claim auto-approved by system rules.";
            }

            return RedirectToAction("Index"); // or lecturer dashboard
        }


        [HttpGet]
        public async Task<IActionResult> MyClaims()
        {
            var userId = _userManager.GetUserId(User);
            var claims = await _db.ClaimRecords
                .Include(c => c.Files)
                .Where(c => c.LecturerId == userId)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();
            return View(claims);
        }
    }
}
