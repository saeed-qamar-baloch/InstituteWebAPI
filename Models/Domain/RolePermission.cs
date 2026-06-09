using System.ComponentModel.DataAnnotations;

namespace InstituteWebApp.Models.Domain
{
    /// <summary>
    /// One allowed access area for a role (page/module-level RBAC).
    /// e.g. RoleName = "Accountant", Area = "fees".
    /// </summary>
    public class RolePermission
    {
        [Key]
        public Guid RolePermissionID { get; set; }
        public string RoleName { get; set; } = "";
        public string Area { get; set; } = "";
    }
}
