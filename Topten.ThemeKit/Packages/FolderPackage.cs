using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit
{
    /// <summary>
    /// ThemeReader implementation for reading from a plain folder
    /// </summary>
    public class FolderPackage : IResourceProvider
    {
        /// <summary>
        /// Constructs a new ThemeReaderFolder on the specified folder
        /// </summary>
        /// <param name="baseFolder">The base folder containing the theme resources</param>
        public FolderPackage(string baseFolder)
        {
            _baseFolder = System.IO.Path.GetFullPath(baseFolder);
        }

        /// <summary>
        /// The base folder of the theme
        /// </summary>
        string _baseFolder;

        /// <inheritdoc />
        public string Name
        {
            get
            {
                return $"file://{_baseFolder}";
            }
        }

        /// <inheritdoc />
        public Stream TryOpenStream(string name)
        {
            try
            {
                // Build path
                var path = Path.Combine(_baseFolder, name);

                // Check it exists and open
                if (System.IO.File.Exists(path))
                    return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

                // Not found
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc />
        public bool DoesStreamExist(string name)
        {
            try
            {
                // Build path
                var path = Path.Combine(_baseFolder, name);

                // Check it exists and open
                return System.IO.File.Exists(path);
            }
            catch
            {
                return false;
            }
        }
    }
}
