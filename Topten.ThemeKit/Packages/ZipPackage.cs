using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit
{
    /// <summary>
    /// ThemeReader implementation for reading from zip files
    /// </summary>
    public class ZipPackage : IResourceProvider
    {
        /// <summary>
        /// Constructs a new ZipPackage for a stream
        /// </summary>
        /// <param name="name">A name describing this zip package</param>
        /// <param name="zipStream">A stream containing the zip archive</param>
        public ZipPackage(string name, Stream zipStream)
        {
            _name = name;
            _zipArchive = new ZipArchive(zipStream);
        }

        /// <summary>
        /// Constructs a new ZipPackage for a file
        /// </summary>
        /// <param name="filename">A stream containing the zip archive</param>
        public ZipPackage(string filename)
        {
            _name = System.IO.Path.GetFullPath(filename);
            _zipArchive = ZipFile.OpenRead(filename);
        }

        /// <summary>
        /// The ZipArchive instance
        /// </summary>
        ZipArchive _zipArchive;
        string _name;

        /// <inheritdoc />
        public string Name
        {
            get
            {
                return $"zip://{_name}";
            }
        }

        /// <inheritdoc />
        public Stream TryOpenStream(string name)
        {
            // Find entry
            var ze = _zipArchive.GetEntry(name);
            if (ze == null)
                return null;

            // Open it
            return ze.Open();
        }

        /// <inheritdoc />
        public bool DoesStreamExist(string name)
        {
            try
            {
                return _zipArchive.GetEntry(name) != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
