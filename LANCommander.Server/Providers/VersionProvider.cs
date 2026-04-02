using System.Diagnostics;
using System.Reflection;
using LANCommander.Server.Services;
using LANCommander.Server.Services.Abstractions;
using LANCommander.Server.Services.Models;
using LANCommander.Server.Settings.Enums;
using Semver;

namespace LANCommander.Server.Providers;

public class VersionProvider : IVersionProvider
{
    public SemVersion GetCurrentVersion()
    {
        var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

        // The .NET SDK appends the git SourceRevisionId as semver build metadata
        // (e.g. "2.0.2-lukas-experimental+abc123"). Strip it so callers that do
        // not call WithoutMetadata() still get a clean, readable version string.
        var versionWithoutMetadata = version?.Split('+')[0] ?? "0.0.0";

        return SemVersion.Parse(versionWithoutMetadata, SemVersionStyles.Any);
    }

    public ReleaseChannel GetReleaseChannel(SemVersion version)
    {
        if (version.IsRelease)
            return ReleaseChannel.Stable;
        
        if (version.IsPrerelease && version.PrereleaseIdentifiers.Any(pi => pi.Value == "nightly"))
            return ReleaseChannel.Nightly;
        
        if (version.IsPrerelease)
            return ReleaseChannel.Prerelease;
        
        throw new ArgumentException("Could not parse version number", nameof(version));
    }
}