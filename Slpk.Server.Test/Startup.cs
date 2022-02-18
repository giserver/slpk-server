using Microsoft.Extensions.DependencyInjection;
using Slpk.Server.Services;

namespace Slpk.Server.Test
{
    internal class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ISlpkFileService, SlpkFileService>();
        }
    }
}
