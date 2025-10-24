using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<IActionResult> Create(ClaimCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User);
            var claim = new ClaimRecord
            {
                LecturerId = userId,
                HoursWorked = model.HoursWorked,
                HourlyRate = model.HourlyRate,
                Notes = model.Notes,
                SubmittedAt = System.DateTime.UtcNow
            };

            _db.ClaimRecords.Add(claim);
           

            if (model.Files != null && model.Files.Count > 0)
            {
                var allowed = new[] { ".pdf", ".docx", ".xlsx" };
                long maxBytes = 5 * 1024 * 1024;
                var uploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
                Directory.CreateDirectory(uploads);

                foreach (var file in model.Files)
                {
                    if (file.Length == 0) continue;
                    if (file.Length > maxBytes) continue;
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowed.Contains(ext)) continue;
                    var stored = System.Guid.NewGuid() + ext;
                    var path = Path.Combine(uploads, stored);
                    using var fs = new FileStream(path, FileMode.Create);
                    await file.CopyToAsync(fs);

                    _db.ClaimFiles.Add(new ClaimFile
                    {
                        ClaimRecordId = claim.Id,
                        FileName = file.FileName,
                        StoredFilePath = Path.Combine("uploads", stored),
                        Size = file.Length
                    });
                }
                
            }

            return RedirectToAction("MyClaims");
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
