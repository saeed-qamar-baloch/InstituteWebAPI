using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class Expense
    {
        [Key]
        public Guid ExpenseID { get; set; }

        public Guid ExpenseCategoryID { get; set; }
        [ForeignKey(nameof(ExpenseCategoryID))]
        public ExpenseCategory ExpenseCategory { get; set; }

        public string Title { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>Date the expense was incurred (drives month/year reporting).</summary>
        public DateTime ExpenseDate { get; set; }

        public string? PaymentMethod { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
