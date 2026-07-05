namespace SunumApp.Authorization
{
    public static class PermissionNames
    {
        public const string Pages = "Pages";

        // Aircraft
        public const string Aircraft_Read = "Aircraft.Read";
        public const string Aircraft_Create = "Aircraft.Create";
        public const string Aircraft_Update = "Aircraft.Update";
        public const string Aircraft_Delete = "Aircraft.Delete";

        // Personnel
        public const string Personnel_Read = "Personnel.Read";
        public const string Personnel_Create = "Personnel.Create";
        public const string Personnel_Update = "Personnel.Update";
        public const string Personnel_Delete = "Personnel.Delete";

        // SnagReport
        public const string SnagReport_Read = "SnagReport.Read";
        public const string SnagReport_Create = "SnagReport.Create";
        public const string SnagReport_Update = "SnagReport.Update";
        public const string SnagReport_Delete = "SnagReport.Delete";
        public const string SnagReport_ChangeStatus = "SnagReport.ChangeStatus";

        // RBAC management
        public const string AppUser_Read = "AppUser.Read";
        public const string AppRole_Read = "AppRole.Read";
        public const string AppUser_Create = "AppUser.Create";
        public const string AppRole_Create = "AppRole.Create";
        public const string AppUser_Update = "AppUser.Update";
        public const string AppRole_Update = "AppRole.Update";
        public const string AppUser_Delete = "AppUser.Delete";
        public const string AppRole_Delete = "AppRole.Delete";
        public const string AppRole_AssignPermissions = "AppRole.AssignPermissions";

    }
}
