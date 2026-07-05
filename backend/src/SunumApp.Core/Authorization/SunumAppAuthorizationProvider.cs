using Abp.Authorization;
using Abp.Localization;

namespace SunumApp.Authorization
{
    public class SunumAppAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            var pages = context.GetPermissionOrNull("Pages") ?? context.CreatePermission("Pages", L("Pages"));

            // Aircraft
            pages.CreateChildPermission(PermissionNames.Aircraft_Read, L("Aircraft.Read"));
            pages.CreateChildPermission(PermissionNames.Aircraft_Create, L("Aircraft.Create"));
            pages.CreateChildPermission(PermissionNames.Aircraft_Update, L("Aircraft.Update"));
            pages.CreateChildPermission(PermissionNames.Aircraft_Delete, L("Aircraft.Delete"));

            // Personnel
            pages.CreateChildPermission(PermissionNames.Personnel_Read, L("Personnel.Read"));
            pages.CreateChildPermission(PermissionNames.Personnel_Create, L("Personnel.Create"));
            pages.CreateChildPermission(PermissionNames.Personnel_Update, L("Personnel.Update"));
            pages.CreateChildPermission(PermissionNames.Personnel_Delete, L("Personnel.Delete"));

            // SnagReport
            pages.CreateChildPermission(PermissionNames.SnagReport_Read, L("SnagReport.Read"));
            pages.CreateChildPermission(PermissionNames.SnagReport_Create, L("SnagReport.Create"));
            pages.CreateChildPermission(PermissionNames.SnagReport_Update, L("SnagReport.Update"));
            pages.CreateChildPermission(PermissionNames.SnagReport_Delete, L("SnagReport.Delete"));
            pages.CreateChildPermission(PermissionNames.SnagReport_ChangeStatus, L("SnagReport.ChangeStatus"));

            // RBAC
            pages.CreateChildPermission(PermissionNames.AppUser_Read, L("AppUser.Read"));
            pages.CreateChildPermission(PermissionNames.AppRole_Read, L("AppRole.Read"));
            pages.CreateChildPermission(PermissionNames.AppUser_Create, L("AppUser.Create"));
            pages.CreateChildPermission(PermissionNames.AppRole_Create, L("AppRole.Create"));
            pages.CreateChildPermission(PermissionNames.AppUser_Update, L("AppUser.Update"));
            pages.CreateChildPermission(PermissionNames.AppRole_Update, L("AppRole.Update"));
            pages.CreateChildPermission(PermissionNames.AppUser_Delete, L("AppUser.Delete"));
            pages.CreateChildPermission(PermissionNames.AppRole_Delete, L("AppRole.Delete"));
            pages.CreateChildPermission(PermissionNames.AppRole_AssignPermissions, L("AppRole.AssignPermissions"));
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, SunumAppConsts.LocalizationSourceName);
        }
    }
}
