using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;

namespace RockLib.Configuration.AspNetCore
{
    /// <summary>
    /// Extension methods for ASP.NET Core.
    /// </summary>
    public static class AspNetExtensions
    {
        /// <summary>
        /// Sets the value of the <see cref="Config.Root"/> property to the <see cref="IConfiguration"/>
        /// containing the merged configuration of the application and the <see cref="IWebHost"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebHostBuilder"/> to configure.</param>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        public static IWebHostBuilder SetConfigRoot(this IWebHostBuilder builder) =>
            builder.ConfigureServices((context, services) =>
            {
                if (!Config.IsLocked && Config.IsDefault)
                    Config.SetRoot(context.Configuration);
            });
    }
}
