using System;
using System.Reflection;
using System.Runtime.Loader;

namespace WebApp
{

    /// <summary>
    /// A custom assembly load context
    /// <para>SEE: https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support </para>
    /// </summary>
    public class LibraryLoadContext: AssemblyLoadContext
    {
        private AssemblyDependencyResolver fResolver;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="BinFolder">A file system path to be used be the dependency resolver</param>
        public LibraryLoadContext(string BinFolder)
        {
            fResolver = new AssemblyDependencyResolver(BinFolder);
        }

        /// <summary>
        ///     When overridden in a derived class, allows an assembly to be resolved and loaded
        ///     based on its System.Reflection.AssemblyName.
        /// </summary>
        /// <param name="assemblyName">The object that describes the assembly to be loaded.</param>
        /// <returns>The loaded assembly, or null.</returns>
        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = fResolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
        /// <summary>
        /// Allows derived class to load an unmanaged library by name.
        /// </summary>
        /// <param name="unmanagedDllName">Name of the unmanaged library. Typically this is the filename without its path or extensions.</param>
        /// <returns>A handle to the loaded library, or System.IntPtr.Zero.</returns>
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
}
