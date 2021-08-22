using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit
{
    /// <summary>
    /// Standard implementation of IPackageProvider that
    /// resolves package names to folders or zip
    /// archives in one or more specified folders.
    /// </summary>
    public class PackageProvider : IPackageProvider
    {
        /// <summary>
        /// Constructs a new ThemeLoader
        /// </summary>
        public PackageProvider()
        {
        }

        /// <summary>
        /// Adds a search folder to the theme loader
        /// </summary>
        /// <param name="folder">The folder to be searched for theme files</param>
        public void AddFolder(string folder)
        {
            _locations.Add(folder);
        }

        /// <summary>
        /// Adds an assembly to the theme loader
        /// </summary>
        /// <param name="assembly">The assembly to be checked for theme files</param>
        public void AddAssembly(Assembly assembly, string prefix)
        {
            _locations.Add(new AssemblyInfo()
            {
                Assembly = assembly,
                Prefix = prefix ?? "",
            });
        }


        /// <summary>
        /// Sets a default extension to be added to all package loads
        /// </summary>
        /// <remarks>
        /// The specified extension should include the period
        /// </remarks>
        public string DefaultExtension
        {
            get;
            set;
        }



        /// <inheritdoc />
        public IResourceProvider OpenPackage(string name)
        {
            string filename = name;
            if (DefaultExtension != null)
            {
                filename += DefaultExtension;
            }

            for (int i=_locations.Count - 1; i >= 0; i--)
            {
                var location = _locations[i];

                if (location is string f)
                {
                    var fullName = System.IO.Path.Combine(f, filename);
                    if (System.IO.File.Exists(fullName))
                        return new ZipPackage(fullName);
                    if (System.IO.Directory.Exists(fullName))
                        return new FolderPackage(fullName);
                }
                if (location is AssemblyInfo a)
                {
                    var stream = a.Assembly.GetManifestResourceStream(a.Prefix + filename);
                    if (stream != null)
                        return new ZipPackage(name, stream);
                }
            }


            throw new InvalidOperationException($"Couldn't find package '{name}'");
        }

        class AssemblyInfo
        {
            public Assembly Assembly;
            public string Prefix;
        }

        List<object> _locations= new List<object>();
    }
}
