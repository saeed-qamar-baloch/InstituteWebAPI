namespace InstituteWebAPI.Services.Access
{
    /// <summary>Canonical page/module areas used for role-based navigation access.</summary>
    public static class AccessAreas
    {
        public record Area(string Key, string Label);

        public static readonly IReadOnlyList<Area> All = new List<Area>
        {
            new("dashboard",      "Dashboard"),
            new("students",       "Students & Admissions"),
            new("teachers",       "Teachers"),
            new("classes",        "Classes & Timetable"),
            new("attendance",     "Attendance"),
            new("fees",           "Fees"),
            new("expenses",       "Expenses"),
            new("marks",          "Marks & Results"),
            new("test-schedule",  "Test Schedule"),
            new("result-cards",   "Result Cards"),
            new("leave-requests", "Leave Requests"),
            new("reports",        "Reports"),
            new("users",          "User Management"),
            new("audit",          "Audit Log"),
            new("settings",       "Settings"),
        };

        public static readonly HashSet<string> Keys =
            new(All.Select(a => a.Key), StringComparer.OrdinalIgnoreCase);
    }
}
