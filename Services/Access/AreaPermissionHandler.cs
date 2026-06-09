using InstituteWebAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InstituteWebAPI.Services.Access
{
    /// <summary>
    /// Lets a user pass a role-based authorization check when one of their roles
    /// has been granted the access-area that the requested controller belongs to.
    /// Built-in role checks still apply (Admin/Teacher), so this only *adds* access
    /// for custom roles — it never removes any.
    /// </summary>
    public class AreaPermissionHandler : AuthorizationHandler<RolesAuthorizationRequirement>
    {
        private readonly IHttpContextAccessor http;
        private readonly RozhnInstituteDbContext db;

        public AreaPermissionHandler(IHttpContextAccessor http, RozhnInstituteDbContext db)
        {
            this.http = http;
            this.db = db;
        }

        // Maps the first route segment after /api/ (the controller name) to an access area.
        private static readonly Dictionary<string, string> RouteArea = new(StringComparer.OrdinalIgnoreCase)
        {
            // Students & admissions
            ["AdminStudents"] = "students", ["AdminAdmissions"] = "students", ["AdminAdmitCard"] = "students",
            ["AdminCardRequest"] = "students", ["StudentTimeline"] = "students", ["StudentFeeHistory"] = "students",
            ["students"] = "students", ["AdminGuardians"] = "students", ["AdminScholarships"] = "students",
            // Teachers
            ["AdminTeachers"] = "teachers", ["AdminTeacherSalary"] = "teachers", ["AdminTeacherCourses"] = "teachers",
            ["TeacherDailyAttendance"] = "teachers", ["TeacherDashboard"] = "teachers",
            // Classes & timetable
            ["AdminClasses"] = "classes", ["AdminCurrentClass"] = "classes", ["AdminClassStudents"] = "classes",
            ["AdminTimetable"] = "classes", ["StudentPromotion"] = "classes", ["AdminSection"] = "classes",
            ["AdminSlots"] = "classes", ["AdminSessions"] = "classes",
            // Attendance
            ["TeacherAttendance"] = "attendance",
            // Fees
            ["fee-management"] = "fees", ["FeeManagement"] = "fees", ["AdminFeeTypes"] = "fees",
            // Expenses
            ["AdminExpense"] = "expenses", ["AdminExpenseCategory"] = "expenses",
            // Marks & results
            ["TeacherStudentMarks"] = "marks", ["TeacherPassingMarks"] = "marks", ["TeacherTests"] = "marks",
            ["MarkEditRequests"] = "marks",
            // Test schedule
            ["AdminTestSchedule"] = "test-schedule",
            // Result cards
            ["StudentResultCard"] = "result-cards",
            // Leave requests
            ["StudentLeaveRequests"] = "leave-requests",
            // Reports
            ["Reports"] = "reports",
            // User management
            ["AdminUsers"] = "users", ["AdminRoles"] = "users",
            // Audit
            ["AdminAudit"] = "audit",
            // Settings (catalogs & config)
            ["AdminTerm"] = "settings", ["AdminTermMonths"] = "settings", ["AdminCourses"] = "settings",
            ["AdminVillage"] = "settings", ["TestTypes"] = "settings", ["AdminGradeCriteria"] = "settings",
            ["AdminInstituteSetting"] = "settings", ["AdminBackup"] = "settings", ["AdminFeeSettings"] = "settings",
            // Dashboard
            ["Dashboard"] = "dashboard",
        };

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesAuthorizationRequirement requirement)
        {
            var ctx = http.HttpContext;
            if (ctx == null) return;
            if (!(context.User.Identity?.IsAuthenticated ?? false)) return;

            // Resolve the area from the request path: /api/{controller}/...
            var segments = ctx.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            if (segments.Length < 2 || !segments[0].Equals("api", StringComparison.OrdinalIgnoreCase)) return;
            if (!RouteArea.TryGetValue(segments[1], out var area)) return;

            var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            if (roles.Count == 0) return;

            var allowed = await db.RolePermissions.AsNoTracking()
                .AnyAsync(p => roles.Contains(p.RoleName) && p.Area == area);

            if (allowed) context.Succeed(requirement);
        }
    }
}
