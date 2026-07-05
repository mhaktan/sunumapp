using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using SunumApp.EntityFrameworkCore;

namespace SunumApp.Web.Host
{
    [DependsOn(typeof(SunumAppApplicationModule), typeof(SunumAppEntityFrameworkCoreModule), typeof(AbpAspNetCoreModule))]
    public class SunumAppWebHostModule : AbpModule
    {
        public override void PreInitialize()
        {
            // Expose all AppServices as dynamic API controllers
            Configuration.Modules.AbpAspNetCore()
                .CreateControllersForAppServices(
                    typeof(SunumAppApplicationModule).GetAssembly(),
                    moduleName: "app",
                    useConventionalHttpVerbs: true
                );
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(SunumAppWebHostModule).GetAssembly());
        }
    }
}
