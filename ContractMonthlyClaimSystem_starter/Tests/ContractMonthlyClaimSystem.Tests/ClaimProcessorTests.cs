using System;
using System.Linq;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContractMonthlyClaimSystem.Tests
{
    public class ClaimProcessorTests : IDisposable
    {
        private readonly ApplicationDbContext _db;

        public object Assert { get; private set; }

        public ClaimProcessorTests()
        {
            // Create a new in-memory database per test class
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _db.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public void ProcessNewClaim_WithDocumentsAndSmallAmount_IsAutoApproved()
        {
            // Arrange
            var claim = new ClaimRecord
            {
                HoursWorked = 5,
                HourlyRate = 500m,
                CalculatedAmount = 5 * 500m,
                DocumentsUploaded = true,
                SubmittedAt = DateTime.UtcNow,
                Status = ClaimStatus.Pending
            };

            _db.ClaimRecords.Add(claim);
            _db.SaveChanges();

            // Act
            var result = ClaimProcessor.ProcessNewClaim(claim, _db);

            // Reload from DB to see updated values
            var updated = _db.ClaimRecords.FirstOrDefault(c => c.Id == claim.Id);

            // Assert
            Assert.Equal(ClaimProcessorResult.AutoApproved, result);
            Assert.Equal(ClaimStatus.Approved, updated.Status);
        }

        [Fact]
        public void ProcessNewClaim_WithoutDocuments_ReturnsNeedsDocsAndRemainsPending()
        {
            // Arrange
            var claim = new ClaimRecord
            {
                HoursWorked = 8,
                HourlyRate = 300m,
                CalculatedAmount = 8 * 300m,
                DocumentsUploaded = false,
                SubmittedAt = DateTime.UtcNow,
                Status = ClaimStatus.Pending
            };

            _db.ClaimRecords.Add(claim);
            _db.SaveChanges();

            // Act
            var result = ClaimProcessor.ProcessNewClaim(claim, _db);
            var updated = _db.ClaimRecords.FirstOrDefault(c => c.Id == claim.Id);

            // Assert
            Assert.Equal(ClaimProcessorResult.NeedsDocs, result);
            Assert.Equal(ClaimStatus.Pending, updated.Status);
        }

        [Fact]
        public void ProcessNewClaim_LargeAmount_IsEscalated()
        {
            // Arrange
            var claim = new ClaimRecord
            {
                HoursWorked = 200,
                HourlyRate = 200m,
                CalculatedAmount = 200 * 200m, // 40 000
                DocumentsUploaded = true,
                SubmittedAt = DateTime.UtcNow,
                Status = ClaimStatus.Pending
            };

            _db.ClaimRecords.Add(claim);
            _db.SaveChanges();

            // Act
            var result = ClaimProcessor.ProcessNewClaim(claim, _db);
            var updated = _db.ClaimRecords.FirstOrDefault(c => c.Id == claim.Id);

            // Assert
            Assert.Equal(ClaimProcessorResult.Escalated, result);
            Assert.Equal(ClaimStatus.Escalated, updated.Status);
        }
    }
}
