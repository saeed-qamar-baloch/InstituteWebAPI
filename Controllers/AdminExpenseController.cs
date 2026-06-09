using InstituteWebAPI.Data;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminExpenseController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;
        private readonly InstituteWebAPI.Services.Audit.IAuditService audit;
        public AdminExpenseController(RozhnInstituteDbContext dbContext, InstituteWebAPI.Services.Audit.IAuditService audit)
        { this.dbContext = dbContext; this.audit = audit; }

        // ── DTOs ──────────────────────────────────────────────────────────────
        public class ExpenseDto
        {
            public Guid ExpenseID { get; set; }
            public Guid ExpenseCategoryID { get; set; }
            public string? CategoryName { get; set; }
            public string Title { get; set; } = "";
            public decimal Amount { get; set; }
            public DateTime ExpenseDate { get; set; }
            public string? PaymentMethod { get; set; }
            public string? Notes { get; set; }
        }

        public class SaveExpenseDto
        {
            [Required] public Guid ExpenseCategoryID { get; set; }
            [Required] public string Title { get; set; } = "";
            [Range(0, double.MaxValue)] public decimal Amount { get; set; }
            [Required] public DateTime ExpenseDate { get; set; }
            public string? PaymentMethod { get; set; }
            public string? Notes { get; set; }
        }

        // ── GET list (filters) ────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to,
            [FromQuery] Guid? categoryId, [FromQuery] string? search)
        {
            var q = dbContext.Expenses.AsNoTracking().Include(e => e.ExpenseCategory).AsQueryable();
            if (from.HasValue)       q = q.Where(e => e.ExpenseDate >= from.Value.Date);
            if (to.HasValue)         q = q.Where(e => e.ExpenseDate <= to.Value.Date);
            if (categoryId.HasValue) q = q.Where(e => e.ExpenseCategoryID == categoryId.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(e => e.Title.Contains(s) || (e.Notes != null && e.Notes.Contains(s)));
            }

            var rows = await q
                .OrderByDescending(e => e.ExpenseDate).ThenByDescending(e => e.CreatedOn)
                .Select(e => new ExpenseDto
                {
                    ExpenseID = e.ExpenseID,
                    ExpenseCategoryID = e.ExpenseCategoryID,
                    CategoryName = e.ExpenseCategory != null ? e.ExpenseCategory.Name : null,
                    Title = e.Title,
                    Amount = e.Amount,
                    ExpenseDate = e.ExpenseDate,
                    PaymentMethod = e.PaymentMethod,
                    Notes = e.Notes,
                })
                .ToListAsync();
            return Ok(rows);
        }

        // ── CRUD ──────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveExpenseDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = new Expense
            {
                ExpenseID = Guid.NewGuid(),
                ExpenseCategoryID = dto.ExpenseCategoryID,
                Title = dto.Title.Trim(),
                Amount = dto.Amount,
                ExpenseDate = dto.ExpenseDate.Date,
                PaymentMethod = string.IsNullOrWhiteSpace(dto.PaymentMethod) ? null : dto.PaymentMethod.Trim(),
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                CreatedOn = DateTime.UtcNow,
            };
            dbContext.Expenses.Add(entity);
            await dbContext.SaveChangesAsync();
            await audit.LogAsync("Expense", "Expense Added",
                $"{entity.Title} — Rs. {entity.Amount} on {entity.ExpenseDate:dd/MM/yyyy}", "Expense", entity.ExpenseID.ToString());
            return Ok(new { entity.ExpenseID });
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SaveExpenseDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var e = await dbContext.Expenses.FirstOrDefaultAsync(x => x.ExpenseID == id);
            if (e == null) return NotFound();
            e.ExpenseCategoryID = dto.ExpenseCategoryID;
            e.Title = dto.Title.Trim();
            e.Amount = dto.Amount;
            e.ExpenseDate = dto.ExpenseDate.Date;
            e.PaymentMethod = string.IsNullOrWhiteSpace(dto.PaymentMethod) ? null : dto.PaymentMethod.Trim();
            e.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
            await dbContext.SaveChangesAsync();
            return Ok(new { e.ExpenseID });
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var e = await dbContext.Expenses.FirstOrDefaultAsync(x => x.ExpenseID == id);
            if (e == null) return NotFound();
            dbContext.Expenses.Remove(e);
            await dbContext.SaveChangesAsync();
            await audit.LogAsync("Expense", "Expense Deleted",
                $"{e.Title} — Rs. {e.Amount}", "Expense", e.ExpenseID.ToString());
            return Ok(new { id });
        }

        // ── Dashboard ─────────────────────────────────────────────────────────
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var yearStart = new DateTime(today.Year, 1, 1);

            var thisMonth = await dbContext.Expenses.Where(e => e.ExpenseDate >= monthStart && e.ExpenseDate <= today).SumAsync(e => (decimal?)e.Amount) ?? 0;
            var thisYear  = await dbContext.Expenses.Where(e => e.ExpenseDate >= yearStart && e.ExpenseDate <= today).SumAsync(e => (decimal?)e.Amount) ?? 0;
            var todayTot  = await dbContext.Expenses.Where(e => e.ExpenseDate == today).SumAsync(e => (decimal?)e.Amount) ?? 0;

            // Category-wise (this year)
            var byCategory = await dbContext.Expenses
                .Where(e => e.ExpenseDate >= yearStart && e.ExpenseDate <= today)
                .GroupBy(e => e.ExpenseCategory.Name)
                .Select(g => new { Name = g.Key, Amount = g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Amount)
                .ToListAsync();

            // Monthly trend (this year, 12 months)
            var monthlyRaw = await dbContext.Expenses
                .Where(e => e.ExpenseDate >= yearStart && e.ExpenseDate <= today)
                .GroupBy(e => e.ExpenseDate.Month)
                .Select(g => new { Month = g.Key, Amount = g.Sum(x => x.Amount) })
                .ToListAsync();
            var trend = Enumerable.Range(1, 12).Select(m => new
            {
                Month = m,
                Label = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(m),
                Amount = monthlyRaw.FirstOrDefault(x => x.Month == m)?.Amount ?? 0
            }).ToList();

            return Ok(new
            {
                ThisMonth = thisMonth,
                ThisYear = thisYear,
                Today = todayTot,
                ByCategory = byCategory,
                HighestCategory = byCategory.FirstOrDefault(),
                MonthlyTrend = trend,
            });
        }

        // ── Summary (reports + income vs expense + profit/loss) ───────────────
        [HttpGet("summary")]
        public async Task<IActionResult> Summary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var today = DateTime.Today;
            var f = (from ?? new DateTime(today.Year, 1, 1)).Date;
            var t = (to ?? today).Date;

            var expenses = await dbContext.Expenses
                .Where(e => e.ExpenseDate >= f && e.ExpenseDate <= t)
                .Select(e => new { e.Amount, e.ExpenseDate, Category = e.ExpenseCategory.Name })
                .ToListAsync();

            // Income = fee payments collected in period
            var payments = await dbContext.Payments
                .Where(p => p.PaymentDate >= f && p.PaymentDate <= t.AddDays(1))
                .Select(p => new { p.TotalAmount, p.PaymentDate })
                .ToListAsync();

            var totalExpense = expenses.Sum(e => e.Amount);
            var totalIncome  = payments.Sum(p => p.TotalAmount);

            var byCategory = expenses
                .GroupBy(e => e.Category)
                .Select(g => new { Name = g.Key, Amount = g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Amount)
                .ToList();

            // by month income vs expense
            var months = new List<object>();
            var cursor = new DateTime(f.Year, f.Month, 1);
            var end = new DateTime(t.Year, t.Month, 1);
            while (cursor <= end)
            {
                var mStart = cursor;
                var mEnd = cursor.AddMonths(1);
                var inc = payments.Where(p => p.PaymentDate >= mStart && p.PaymentDate < mEnd).Sum(p => p.TotalAmount);
                var exp = expenses.Where(e => e.ExpenseDate >= mStart && e.ExpenseDate < mEnd).Sum(e => e.Amount);
                months.Add(new
                {
                    Year = cursor.Year,
                    Month = cursor.Month,
                    Label = cursor.ToString("MMM yyyy", CultureInfo.CurrentCulture),
                    Income = inc,
                    Expense = exp,
                    Net = inc - exp,
                });
                cursor = cursor.AddMonths(1);
            }

            return Ok(new
            {
                From = f,
                To = t,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Net = totalIncome - totalExpense,
                ByCategory = byCategory,
                ByMonth = months,
            });
        }
    }
}
