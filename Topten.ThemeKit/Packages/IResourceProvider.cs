using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit
{
    /// <summary>
    /// Base class for theme package readers
    /// </summary>
    public interface IResourceProvider
    {
        /// <summary>
        /// The name of this package
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Try to open a stream in this package
        /// </summary>
        /// <param name="name">The name of the stream to open</param>
        /// <returns>The opened stream or null if not found</returns>
        Stream TryOpenStream(string name);

        /// <summary>
        /// Check if a stream with the specified name exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool DoesStreamExist(string name);
    }

    /// <summary>
    /// Helper extensions for working with IResourceProvider
    /// </summary>
    public static class IResourceProviderExtensions
    {
        /// <summary>
        /// Opens a resource stream, throwing an exception if not found
        /// </summary>
        /// <param name="provider">The resource provider</param>
        /// <param name="name">The name of the stream to open</param>
        /// <returns>A Stream reference</returns>
        public static Stream OpenStream(this IResourceProvider provider, string name)
        {
            var stm = provider.TryOpenStream(name);
            if (stm == null)
                throw new IOException($"Stream '{name}' not found in '{provider.Name}'");
            return stm;
        }

        /// <summary>
        /// Reads a text file from a resource provider
        /// </summary>
        /// <param name="provider">The resource provider</param>
        /// <param name="name">The name of the stream to open</param>
        /// <returns>The text content of the file</returns>
        public static string ReadTextStream(this IResourceProvider provider, string name)
        {
            using (var stm = provider.OpenStream(name))
            {
                using (var sr = new StreamReader(stm, Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
