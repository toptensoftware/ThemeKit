using Topten.ThemeKit.Ast;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit
{
    /// <summary>
    /// Base class for theme readers
    /// </summary>
    public class Theme : IResourceProvider, IResolverSite
    {
        /// <summary>
        /// Constructs a new empty Theme
        /// </summary>
        public Theme()
        {
            Reset();
        }

        /// <summary>
        /// Gets or sets the context to be used for resolving classes
        /// </summary>
        public ResolverContext ResolverContext
        {
            get
            {
                if (_resolverContext == null)
                    _resolverContext = new ResolverContext();
                return _resolverContext;
            }
            set
            {
                _resolverContext = value;
                Reset();
            }
        }

        /// <summary>
        /// A dictionary of implicit argument values
        /// </summary>
        /// <remarks>
        /// The values will be passed to any registered functions
        /// with arguments marked with the [ExplicitArgument] attribute
        /// </remarks>
        public IDictionary<string, object> ImplicitArgumentValues
        {
            get => _implicitArgumentValues;
        }


        /// <summary>
        /// Reset the theme's content to empty
        /// </summary>
        void Reset()
        {
            _packages = new List<IResourceProvider>();
            _packageName = null;
            _resolver = new Resolver(this);
            _loadedClasses = new Dictionary<string, object>();
        }

        /// <summary>
        /// Load them from a string
        /// </summary>
        /// <param name="source">Source code for the theme</param>
        /// <param name="name">Name (used for error messages etc)</param>
        public void LoadString(string source, string name = null)
        {
            // Reset content
            Reset();

            // Package name is name of the file
            _packageName = name;

            // Load the file
            var stringSource = new StringSource(source, name);

            // Create parser
            var parser = new Parser(stringSource);

            // Setup include resolver
            parser.ResolveInclude = (pos, includeName) => throw new InvalidOperationException("Include files not supported for string sources");

            // Parse document
            _resolver.AddDocument(parser.Parse());

            // Fire event
            Loaded?.Invoke();
        }

        /// <summary>
        /// Load a theme file
        /// </summary>
        /// <param name="filename"></param>
        public void LoadFile(string filename)
        {
            // Reset content
            Reset();

            // Create a package for the containing folder
            var fullPath = Path.GetFullPath(filename);
            _packages.Add(new FolderPackage(Path.GetDirectoryName(fullPath)));

            // Package name is name of the file
            _packageName = Path.GetFileNameWithoutExtension(filename);

            // Load the document
            var stringSource = StringSource.FromFile(filename);

            // Create parser
            var parser = new Parser(stringSource);

            // Setup include resolver
            parser.ResolveInclude = (pos, includeName) =>
                StringSource.FromFile(Path.GetDirectoryName(pos.Source.FileName), includeName);

            // Parse document
            _resolver.AddDocument(parser.Parse());

            // Fire event
            Loaded?.Invoke();
        }

        /// <summary>
        /// Gets or sets the package provider for this theme
        /// </summary>
        public PackageProvider PackageProvider
        {
            get
            {
                if (_packageProvider == null)
                    _packageProvider = new PackageProvider();
                return _packageProvider;
            }
            set
            {
                _packageProvider = value;
            }
        }

        /// <summary>
        /// Load a new theme and fire loaded event
        /// </summary>
        /// <param name="packageName">Name of the package to load</param>
        public void LoadPackage(string packageName)
        {
            // Reset
            Reset();

            // Store theme name
            _packageName = packageName;

            // Open the package
            var package = _packageProvider.OpenPackage(packageName);
            _packages.Add(package);

            // Load it
            LoadPackageInternal(package);

            // Fire loaded event
            Loaded?.Invoke();
        }

        /// <summary>
        /// Internal helper to load theme
        /// </summary>
        /// <param name="package">The package to load from</param>
        void LoadPackageInternal(IResourceProvider package)
        {
            // Load the root document
            var stringSource = new StringSource(package.ReadTextStream("main.tkl"), "main.tkl");

            // Create parser
            var parser = new Parser(stringSource);
            parser.ResolveInclude = (pos, includeName) => new StringSource(package.ReadTextStream(includeName), includeName);

            // Parse it
            var document = parser.Parse();
            _resolver.AddDocument(document);

            // Add all imported packages
            foreach (var e in document.Elements.OfType<AstImportDeclaration>())
            {
                // Open the imported package
                var importedPackage = _packageProvider.OpenPackage(e.Name);
                _packages.Add(importedPackage);

                // Load it
                LoadPackageInternal(importedPackage);
            }
        }

        /// <summary>
        /// Fired when the theme is loaded
        /// </summary>
        public event Action Loaded;

        /// <summary>
        /// Gets the loaded package name
        /// </summary>
        public string PackageName
        {
            get => _packageName;
        }

        /// <summary>
        /// Load a theme class
        /// </summary>
        /// <typeparam name="T">The type to be loaded</typeparam>
        /// <param name="className">The class name</param>
        /// <returns>The loaded theme class</returns>
        public T GetClass<T>(string className) where T : class, new()
        {
            // Build key
            var key = $"{typeof(T).FullName}//{className}";

            // Already loaded?
            if (_loadedClasses.TryGetValue(key, out var obj))
                return (T)obj;

            // Resolve and cache it
            T resolved = _resolver.GetClass<T>(className);
            _loadedClasses.Add(key, resolved);

            // Done!
            return resolved;
        }

        string IResourceProvider.Name
        {
            get
            {
                return $"theme://{_packageName}";
            }
        }

        /// <inheritdoc />
        public Stream TryOpenStream(string name)
        {
            return _packages?.Select(x=>x.TryOpenStream(name)).FirstOrDefault(x => x != null);
        }

        /// <inheritdoc />
        public bool DoesStreamExist(string name)
        {
            return _packages?.Any(x => x.DoesStreamExist(name)) ?? false;
        }


        /// <summary>
        /// Get the provider's name (used for resource caching)
        /// </summary>
        public string ProviderName => $"Theme:{_packageName}";

        string _packageName;
        List<IResourceProvider> _packages;
        PackageProvider _packageProvider;
        Resolver _resolver;
        ResolverContext _resolverContext;
        Dictionary<string, object> _loadedClasses;
        Dictionary<string, object> _implicitArgumentValues = new Dictionary<string, object>();

        ResolverContext IResolverSite.GetResolverContext() => ResolverContext;
        IDictionary<string, object> IResolverSite.GetImplicitArgValues() => _implicitArgumentValues;
    }
}
