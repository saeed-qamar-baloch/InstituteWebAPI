using System.ComponentModel.DataAnnotations;

namespace InstituteWebApp.Models.Domain
{
    public class ExpenseCategory
    {
        [Key]
        public Guid ExpenseCategoryID { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public List<Expense> Expenses { get; set; } = new();
    }
}
