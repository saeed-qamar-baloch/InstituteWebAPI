namespace InstituteWebApp.Models.Domain
{
    public enum FeeDueType
    {
        Admission = 1,
        Monthly = 2,
        Card = 3
    }

    public enum FeeDueStatus
    {
        Unpaid = 1,
        Paid = 2,
        Partial = 3,
        Waived = 4
    }

    public enum PaymentMethod
    {
        Cash = 1,
        Bank = 2,
        Online = 3
    }
}
