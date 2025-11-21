using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContractMonthlyClaimSystem.Controllers
{
    // Only HR role should access this controller
    [Authorize(Roles = "HR")]
    [Route("[controller]/[action]")]
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HRController(ApplicationDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        // Show the export form (date range)
        [HttpGet]
        public IActionResult Export()
        {
            return View();
        }

        // POST: generate CSV for approved claims between dates (inclusive)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExportApprovedClaimsCsv(DateTime? from, DateTime? to)
        {
            // default: last month
            var fromDate = from ?? DateTime.UtcNow.AddMonths(-1).Date;
            var toDate = (to ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1); // end of day

            var data = _db.ClaimRecords
                .Where(c => c.Status == ClaimStatus.Approved
                            && c.StatusUpdatedAt != null
                            && c.StatusUpdatedAt >= fromDate
                            && c.StatusUpdatedAt <= toDate)
                .Select(c => new
                {
                    c.Id,
                    LecturerId = c.LecturerId,
                    c.HoursWorked,
                    c.HourlyRate,
                    c.CalculatedAmount,
                    ApprovedDate = c.StatusUpdatedAt,
                    Files = c.Files.Select(f => f.FileName).ToList() // optional
                })
                .ToList();

            var csv = new StringBuilder();
            // header
            csv.AppendLine("Id,LecturerId,HoursWorked,HourlyRate,CalculatedAmount,ApprovedDate,AttachedFiles");

            foreach (var r in data)
            {
                var filesJoined = r.Files != null && r.Files.Any()
                    ? string.Join(" | ", r.Files).Replace(",", " ") // avoid commas in CSV cells
                    : "";
                var line = string.Format("{0},{1},{2},{3},{4},{5},{6}",
                    r.Id,
                    EscapeCsv(r.LecturerId),
                    r.HoursWorked,
                    r.HourlyRate,
                    r.CalculatedAmount,
                    r.ApprovedDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                    EscapeCsv(filesJoined)
                );
                csv.AppendLine(line);
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            var fileName = $"ApprovedClaims_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.csv";

            return File(bytes, "text/csv", fileName);
        }

        // simple CSV escaping for values that may contain commas/newlines
        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            var v = value.Replace("\"", "\"\""); // escape quotes
            if (v.Contains(",") || v.Contains("\n") || v.Contains("\r"))
                return $"\"{v}\"";
            return v;
        }
    }
}
