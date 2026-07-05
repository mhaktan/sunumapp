using System.Collections.Generic;
using Abp.Dependency;

namespace SunumApp.Authorization
{
    /// <summary>Single permission descriptor — name, group (entity), description.</summary>
    public class PermissionInfo
    {
        public string Name { get; }
        public string Group { get; }
        public string Description { get; }
        public bool IsRbac { get; }

        public PermissionInfo(string name, string group, string description, bool isRbac)
        {
            Name = name; Group = group; Description = description; IsRbac = isRbac;
        }
    }

    public interface IPermissionRegistry
    {
        IReadOnlyList<PermissionInfo> All { get; }
    }

    public class PermissionRegistry : IPermissionRegistry, ISingletonDependency
    {
        public IReadOnlyList<PermissionInfo> All { get; } = new List<PermissionInfo>
        {
            new PermissionInfo("Aircraft.Read", "Aircraft", "Read Aircraft", false),
            new PermissionInfo("Aircraft.Create", "Aircraft", "Create Aircraft", false),
            new PermissionInfo("Aircraft.Update", "Aircraft", "Update Aircraft", false),
            new PermissionInfo("Aircraft.Delete", "Aircraft", "Delete Aircraft", false),
            new PermissionInfo("Personnel.Read", "Personnel", "Read Personnel", false),
            new PermissionInfo("Personnel.Create", "Personnel", "Create Personnel", false),
            new PermissionInfo("Personnel.Update", "Personnel", "Update Personnel", false),
            new PermissionInfo("Personnel.Delete", "Personnel", "Delete Personnel", false),
            new PermissionInfo("SnagReport.Read", "SnagReport", "Read SnagReport", false),
            new PermissionInfo("SnagReport.Create", "SnagReport", "Create SnagReport", false),
            new PermissionInfo("SnagReport.Update", "SnagReport", "Update SnagReport", false),
            new PermissionInfo("SnagReport.Delete", "SnagReport", "Delete SnagReport", false),
            new PermissionInfo("SnagReport.ChangeStatus", "SnagReport", "Change SnagReport status", false),
            new PermissionInfo("AppUser.Read", "AppUser", "Read users", true),
            new PermissionInfo("AppRole.Read", "AppRole", "Read roles", true),
            new PermissionInfo("AppUser.Create", "AppUser", "Create users", true),
            new PermissionInfo("AppRole.Create", "AppRole", "Create roles", true),
            new PermissionInfo("AppUser.Update", "AppUser", "Update users", true),
            new PermissionInfo("AppRole.Update", "AppRole", "Update roles", true),
            new PermissionInfo("AppUser.Delete", "AppUser", "Delete users", true),
            new PermissionInfo("AppRole.Delete", "AppRole", "Delete roles", true),
            new PermissionInfo("AppRole.AssignPermissions", "AppRole", "Assign permissions to roles", true),
        };
    }
}
