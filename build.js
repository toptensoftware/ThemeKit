var bt = require('./buildtools/buildTools.js')

if (bt.options.official)
{
    // Check everything committed
    bt.git_check();

    // Clock version
    bt.clock_version();

    // Clean build directory
    bt.run("rm -rf ./Build");

    // Run tests
    bt.run("dotnet test Topten.ThemeKit.Test -c Release");
}

// Build
bt.run("dotnet build Topten.ThemeKit\\Topten.ThemeKit.csproj -c Release")

if (bt.options.official)
{
    // Tag and commit
    bt.git_tag();

    // Push nuget package
    /* NOT TILL READY!
    bt.run(`dotnet nuget push`,
           `./Build/Release/*.${bt.options.version.build}.nupkg`,
           `--source nuget.org`);
    */
}
