using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Microsoft.AspNetCore.Mvc.ApplicationParts;
 

 
 
using Microsoft.Extensions.FileProviders;
 

namespace WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        List<Assembly> DynamicallyLoadedLibraries = new List<Assembly>();

        /// <summary>
        /// To be used with Razor class libraries already referenced by this project
        /// </summary>
        void ConfigureStaticLibraries(ApplicationPartManager PartManager)
        {
            Assembly Assembly = typeof(StaticRCL.Controllers.LibController).Assembly;  
            ApplicationPart ApplicationPart = new AssemblyPart(Assembly);

            PartManager.ApplicationParts.Add(ApplicationPart);
        }
        /// <summary>
        /// To be used with Razor class libraries loaded dynamically
        /// </summary>
        void LoadDynamicLibraries(ApplicationPartManager PartManager)
        {
            // get the output folder of this application
            string BinFolder = this.GetType().Assembly.ManifestModule.FullyQualifiedName;
            BinFolder = Path.GetDirectoryName(BinFolder);

            // get the full filepath of any dll starting with the rcl_ prefix
            string Prefix = "rcl_";
            string SearchPattern = $"{Prefix}*.dll";   
            string[] LibraryPaths = Directory.GetFiles(BinFolder, SearchPattern);

            if (LibraryPaths != null && LibraryPaths.Length > 0)
            {
                // create the load context
                LibraryLoadContext LoadContext = new LibraryLoadContext(BinFolder);

                Assembly Assembly;
                ApplicationPart ApplicationPart;
                foreach (string LibraryPath in LibraryPaths)
                {
                    // load each assembly using its filepath
                    Assembly = LoadContext.LoadFromAssemblyPath(LibraryPath);

                    // create an application part for that assembly
                    ApplicationPart = LibraryPath.EndsWith(".Views.dll") ? new CompiledRazorAssemblyPart(Assembly) as ApplicationPart : new AssemblyPart(Assembly);

                    // register the application part
                    PartManager.ApplicationParts.Add(ApplicationPart);

                    // if it is NOT the *.Views.dll add it to a list for later use
                    if (!LibraryPath.EndsWith(".Views.dll"))
                        DynamicallyLoadedLibraries.Add(Assembly);
                } 
            }

        } 
        /// <summary>
        /// Registers a <see cref="CompositeFileProvider"/> for each dynamically loaded assembly.
        /// </summary>
        void RegisterDynamicLibariesStaticFiles(IWebHostEnvironment env)
        {
            IFileProvider FileProvider;
            foreach (Assembly A in DynamicallyLoadedLibraries)
            {
                // create a "web root" file provider for the embedded static files found on wwwroot folder
                FileProvider = new ManifestEmbeddedFileProvider(A, "wwwroot");

                // register a new composite provider containing
                // the old web root file provider
                // and the new one we just created
                env.WebRootFileProvider = new CompositeFileProvider(env.WebRootFileProvider, FileProvider); 
            }
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().
                ConfigureApplicationPartManager((PartManager) => {
                    ConfigureStaticLibraries(PartManager);  // static RCLs
                    LoadDynamicLibraries(PartManager);      // dynamic RCLs
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();

            // register file providers for the dynamically loaded libraries
            if (DynamicallyLoadedLibraries.Count > 0)
                RegisterDynamicLibariesStaticFiles(env);


            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
