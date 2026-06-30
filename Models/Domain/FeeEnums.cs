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
        Waived = 4,

        /// <summary>
        /// "Not Registered" — a placeholder due for the admission month when the
        /// student registered on/after the 25th. No amount is owed; the next
        /// month's fee is generated normally instead. One-time, at admission only.
        /// </summary>
        NR = 5
    }

    public enum PaymentMethod
    {
        Cash = 1,
        Bank = 2,
        Online = 3
    }
}
