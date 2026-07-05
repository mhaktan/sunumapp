using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;

namespace SunumApp
{
    [DependsOn(typeof(SunumAppCoreModule), typeof(AbpAutoMapperModule))]
    public class SunumAppApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Modules.AbpAutoMapper().Configurators.Add(cfg =>
            {
                cfg.AddMaps(typeof(SunumAppApplicationModule).GetAssembly());
            });
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(SunumAppApplicationModule).GetAssembly());
        }
    }
}
