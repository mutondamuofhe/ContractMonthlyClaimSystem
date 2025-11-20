using System;
using System.Linq;
using ContractMonthlyClaimSystem.Models;   // adjust namespace if needed
using ContractMonthlyClaimSystem.Data;     // adjust if your DbContext is elsewhere

namespace ContractMonthlyClaimSystem.Services
{
    public enum ClaimProcessorResult
    {
        None,
        AutoApproved,
        Escalated,
        NeedsDocs
    }

    public static class ClaimProcessor
    {
        public static ClaimProcessorResult ProcessNewClaim(Claim claim, ApplicationDbContext ctx)
        {
            // Basic business rule example — adjust to your needs

            if (!claim.DocumentsUploaded)
            {
                claim.Status = "Pending";   // keeps pending for manual review
                ctx.Update(claim);
                ctx.SaveChanges();
                return ClaimProcessorResult.NeedsDocs;
            }

            if (claim.HoursWorked <= 10 && claim.CalculatedAmount <= 5000)
            {
                claim.Status = "Approved";
                claim.StatusUpdatedDate = DateTime.UtcNow;
                claim.LastProcessedBy = "System";
                ctx.Update(claim);
                ctx.SaveChanges();
                return ClaimProcessorResult.AutoApproved;
            }

            if (claim.CalculatedAmount > 20000)
            {
                claim.Status = "Escalated";  // manager review needed
                claim.StatusUpdatedDate = DateTime.UtcNow;
                claim.LastProcessedBy = "System";
                ctx.Update(claim);
                ctx.SaveChanges();
                return ClaimProcessorResult.Escalated;
            }

            claim.Status = "Pending";
            ctx.Update(claim);
            ctx.SaveChanges();
            return ClaimProcessorResult.None;
        }
    }
}
