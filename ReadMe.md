


## Introduction

Asp.Net Core, starting from version **3.0**, provides a way to split an application into modules by using [Application Parts](https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-3.1).

A solution may comprised of a Web Application and any number of Assembly libraries that may contain controllers, views, pages, static files such as javascript and css files, and more. Those libraries are called [Razor Class Libraries](https://docs.microsoft.com/en-us/aspnet/core/razor-pages/ui-class?view=aspnetcore-3.1&tabs=visual-studio) or RCL.

There is a number of reasons as to why someone wants to use Razor libraries in a solution.

But the most valuable case is when a library is loaded dynamically, as a **plugin**. Imagine an e-Commerce solution providing a number of Tax or Shipping Charges calculation **plugins** or Payment **plugins**, for the administrator to choose from.

There are some difficulties though. For sure the documentation fails to provide fully descriptive examples and samples. 

But the most frustrating is that it seems that [Application Parts](https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-3.1) and [RCLs](https://docs.microsoft.com/en-us/aspnet/core/razor-pages/ui-class?view=aspnetcore-3.1&tabs=visual-studio) are not created to be used with libraries that loaded dynamically, that is **plugins**.

Especially when it comes to static files, i.e. javasript and css files, the dynamically loaded RCLs is a failure.

## The content of this exercise

In this text we'll examine both use cases:

- an RCL statically referenced by the main application
- an RCL that is loaded dynamically by the main application.

Both RCLs include static files, i.e. javasript and css files.

We will use an Asp.Net Core MVC Web application and two RCLs. 

For start create an Asp.Net Core MVC Web application and name it `WebApp`.

## A referenced RCL

Create an RCL following the instructions provided by the [documentation](https://docs.microsoft.com/en-us/aspnet/core/razor-pages/ui-class?view=aspnetcore-3.1&tabs=visual-studio).

Name the RCL `StaticRCL`. We'll see later why the name matters.

Delete all files and folders from the project and add three new folders, `Controllers`, `Views` and `wwwroot`.

Create a controller class inside the `Controllers` folder.

```
    public class LibController : Controller
    {
        [Route("/static")]
        public IActionResult Index()
        {
            return View();
        }
    }
```

Create a `Lib` folder inside the `Views` folder. Add an `Index.cshtml` view file.

```
    <script src="~/_content/StaticRCL/js/script.js"></script>

    <div>
        <strong>STATICALLY</strong> referenced Razor Class Library
    </div>

    <div>
        <button onclick="StaticRCL_ShowMessage();">Click Me!</button>
    </div>
```

Create a `js` folder insided the `wwwroot` folder. Add a `script.js` file.

```
    function StaticRCL_ShowMessage() {
        alert('Hi from Statically refernced Razor Class Library javascript');
    }
```

## A dynamically loadable RCL

Create another RCL with similar structure and files as above. Name it `DynamicRCL`.

Controller.

```
    public class LibDynamicController : Controller
    {
        [Route("/dynamic")]
        public IActionResult Index()
        {
            return View();
        }
    }
```

View.

```
<script src="js/script.js"></script>

<div>
    <strong>DYNAMICALLY</strong> loaded Razor Class Library
</div>

<div>
    <button onclick="DynamicRCL_ShowMessage();">Click Me!</button>
</div>
```

Javascript file.

```
function DynamicRCL_ShowMessage() {
    alert('Hi from Dynamically loaded Razor Class Library javascript');
}
```

We also need to do the following.

- Put a `rcl_` prefix in the Assembly Name of the project, i.e. `<AssemblyName>rcl_DynamicRCL</AssemblyName>`
- Add a `GenerateEmbeddedFilesManifest`, i.e. `<GenerateEmbeddedFilesManifest>true</GenerateEmbeddedFilesManifest>`
- Add the `Microsoft.Extensions.FileProviders.Embedded` NuGet package, i.e. `<PackageReference Include="Microsoft.Extensions.FileProviders.Embedded" Version="3.1.0" />`
- Set the output path to the `bin` folder of the main Web Application, i.e. `<OutputPath>..\WebApp\bin\Debug\</OutputPath>`
- Instruct the project to use all files in `wwwroot` folder as embedded resources, i.e. `<EmbeddedResource Include="wwwroot\**\*" />`

Here is the whole project source file.

```
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>netcoreapp3.1</TargetFramework>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
    <AssemblyName>rcl_DynamicRCL</AssemblyName>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>     
    <GenerateEmbeddedFilesManifest>true</GenerateEmbeddedFilesManifest>
  </PropertyGroup>

  <PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Debug|AnyCPU'">
    <OutputPath>..\WebApp\bin\Debug\</OutputPath>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
    <PackageReference Include="Microsoft.Extensions.FileProviders.Embedded" Version="3.1.0" />
  </ItemGroup> 
    
   <ItemGroup>
      <EmbeddedResource Include="wwwroot\**\*" />
   </ItemGroup>

</Project>
```

## Handing a referenced RCL

The `WebApp` Web Application should have a `project reference` to the first RCL, the `StaticRCL`.

The `Index.cshtml` of the `HomeController` is as the following.

```
@{
    ViewData["Title"] = "Home Page";
}

<div class="text-center">
    <h1 class="display-4">Welcome</h1>
    <div><a href="/static">Statically referenced Razor Class Library View</a></div>
    <div><a href="/dynamic">Dynamically loaded Razor Class Library View</a></div>
</div>
```

As you can see there are two anchor elements calling a corresponding RCL route.

The `ConfigureServices()` method of the `Startup` class handles both, the statically referenced and the dynamically loaded libraries.

```
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews().
            ConfigureApplicationPartManager((PartManager) => {
                ConfigureStaticLibraries(PartManager);  // static RCLs
                LoadDynamicLibraries(PartManager);      // dynamic RCLs
            });
    }
```

The [`ApplicationPartManager`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.applicationparts.applicationpartmanager?view=aspnetcore-3.1) class manages the parts and features of an Asp.Net Core MVC or Razor Pages application.

The logic is to get a reference to an `Assembly` already referenced by the Web Application, create an [`AssemblyPart`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.applicationparts.assemblypart?view=aspnetcore-3.1) for that `Assembly` and then register that `AssemblyPart` calling the `ApplicationPartManager`.

```
    void ConfigureStaticLibraries(ApplicationPartManager PartManager)
    {
        Assembly Assembly = typeof(StaticRCL.Controllers.LibController).Assembly;  
        ApplicationPart ApplicationPart = new AssemblyPart(Assembly);

        PartManager.ApplicationParts.Add(ApplicationPart);
    }
```

The above works fine with routing to Razor Views (and Razor Pages). With a **twist** when comes to static files, such as javascript and css files.

Here is what the documentation says

> The files included in the `wwwroot` folder of the RCL are exposed to either the RCL or the consuming app under the prefix `_content/{LIBRARY NAME}/`. For example, a library named `Razor.Class.Lib` results in a path to static content at `_content/Razor.Class.Lib/`.

Here is what the `Index.cshtml` of the `StaticRCL` project does, conforming to the above.

```
    <script src="~/_content/StaticRCL/js/script.js"></script>
```

## Handling a dynamically loaded RCL

A descendant of the [AssemblyLoadContext](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.loader.assemblyloadcontext?view=netcore-3.1) class is required in order to load the plugin libraries, according to the [relevent documentation](https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support).

Here it is.

```
    public class LibraryLoadContext: AssemblyLoadContext
    {
        private AssemblyDependencyResolver fResolver;

        public LibraryLoadContext(string BinFolder)
        {
            fResolver = new AssemblyDependencyResolver(BinFolder);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = fResolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string FilePath = fResolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (FilePath != null)
            {
                return LoadUnmanagedDllFromPath(FilePath);
            }

            return IntPtr.Zero;
        }
    }
```

We use that `LibraryLoadContext` in loading plugin libraries. 

The following `LoadDynamicLibraries()` called from `ConfigureServices()` loads the libraries, i.e. plugin assemblies, based on a prefix, in this case `rcl_`. That's why we've changed the assembly name of the `DynamicRCL` project to `rcl_DynamicRCL`, above.

I hope the code is easily understandable.

```
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
```

Now the tricky part.

We have configured so javascript, css and other `static` resources to be **embedded** resources in the `DynamicRCL`. Furthermore we asked that library to create a **manifest** for those embedded files.

Now we have to read that **manifest**, create an [`IFileProvider`](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/file-providers?view=aspnetcore-3.1) on that `DynamicRCL` Assembly and its `wwwroot` folder, and then register that file provider with the system.

```
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
```

The above method is called by the `Configure()` method of the `Startup` class just before the `app.UseStaticFiles()` call.

```
    app.UseHttpsRedirection();

    // register file providers for the dynamically loaded libraries
    if (DynamicallyLoadedLibraries.Count > 0)
        RegisterDynamicLibariesStaticFiles(env);

    app.UseStaticFiles();
```

Here is what the `Index.cshtml` of the `DynamicRCL` project does in order to use a javascript file.

```
    <script src="js/script.js"></script>
```

There is no use of the `_content/{LIBRARY NAME}/` scheme here. We just use the `js` folder since we have registered the `wwwroot` folder of the `DynamicRCL` assembly as the **web root** folder.


That's all.


**Tested on:**
- Windows 10
- Asp.Net Core 3.1
- Microsoft Visual Studio 2019 Preview, Version 16.9.0 Preview 5.0