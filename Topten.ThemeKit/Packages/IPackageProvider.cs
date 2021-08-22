using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit
{
    /// <summary>
    /// Provides packages of given name
    /// </summary>
    public interface IPackageProvider
    {
        /// <summary>
        /// Provide a pacakage with given name
        /// </summary>
        /// <param name="name">The name of the package</param>
        /// <returns>An IThemePackage instnace</returns>
        IResourceProvider OpenPackage(string name);
    }
}
