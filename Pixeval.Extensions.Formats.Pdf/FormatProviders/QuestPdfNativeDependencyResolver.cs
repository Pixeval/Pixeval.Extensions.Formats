using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Pixeval.Extensions.SDK;
using QuestPDF.Infrastructure;

namespace Pixeval.Extensions.Formats.Pdf.FormatProviders;

internal static class QuestPdfNativeDependencyResolver
{
    private static readonly Lock InitializationLock = new();
    private static readonly ConcurrentBag<IntPtr> NativeLibraryHandles = [];
    private static bool _isInitialized;

    public static void Configure()
    {
        Initialize();
        QuestPDF.Settings.License = LicenseType.Community;

        var extensionDirectory = ExtensionsHostBase.ExtensionDirectory;
        if (!string.IsNullOrWhiteSpace(extensionDirectory))
        {
            AddFontDiscoveryPath(extensionDirectory);
            AddFontDiscoveryPath(Path.Combine(extensionDirectory, "LatoFont"));
        }
    }

    private static void Initialize()
    {
        if (_isInitialized)
            return;

        lock (InitializationLock)
        {
            if (_isInitialized)
                return;

            NativeLibrary.SetDllImportResolver(typeof(QuestPDF.Settings).Assembly, ResolveNativeLibrary);
            _isInitialized = true;
        }
    }

    private static IntPtr ResolveNativeLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        var mappedLibraryName = GetMappedLibraryName(libraryName);

        foreach (var directory in GetSearchDirectories())
        {
            if (!Directory.Exists(directory))
                continue;

            PreloadWindowsSidecarDependencies(directory);

            var candidatePath = Path.Combine(directory, mappedLibraryName);
            if (NativeLibrary.TryLoad(candidatePath, out var handle))
                return handle;
        }

        return IntPtr.Zero;
    }

    private static IEnumerable<string> GetSearchDirectories()
    {
        foreach (var directory in GetBaseDirectories())
        {
            yield return directory;
            yield return Path.Combine(directory, "runtimes", GetRuntimeIdentifier(), "native");
        }
    }

    private static IEnumerable<string> GetBaseDirectories()
    {
        if (!string.IsNullOrWhiteSpace(ExtensionsHostBase.ExtensionDirectory))
            yield return ExtensionsHostBase.ExtensionDirectory;

        if (!string.IsNullOrWhiteSpace(AppContext.BaseDirectory))
            yield return AppContext.BaseDirectory;

        if (!string.IsNullOrWhiteSpace(Environment.CurrentDirectory))
            yield return Environment.CurrentDirectory;

        var currentDirectory = Directory.GetCurrentDirectory();
        if (!string.IsNullOrWhiteSpace(currentDirectory))
            yield return currentDirectory;
    }

    private static void PreloadWindowsSidecarDependencies(string directory)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        foreach (var dependencyName in WindowsSidecarDependencies)
        {
            var dependencyPath = Path.Combine(directory, dependencyName);
            if (!File.Exists(dependencyPath))
                continue;

            if (NativeLibrary.TryLoad(dependencyPath, out var handle))
                NativeLibraryHandles.Add(handle);
        }
    }

    private static void AddFontDiscoveryPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return;

        if (QuestPDF.Settings.FontDiscoveryPaths.Any(existingPath => string.Equals(existingPath, path, StringComparison.OrdinalIgnoreCase)))
            return;

        QuestPDF.Settings.FontDiscoveryPaths.Add(path);
    }

    private static string GetMappedLibraryName(string libraryName)
    {
        if (Path.HasExtension(libraryName))
            return libraryName;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return libraryName + ".dll";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "lib" + libraryName + ".dylib";

        return "lib" + libraryName + ".so";
    }

    private static string GetRuntimeIdentifier()
    {
        var os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "win"
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? "osx"
                : "linux";

        var architecture = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86 => "x86",
            Architecture.X64 => "x64",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            _ => RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()
        };

        return os + "-" + architecture;
    }

    private static readonly string[] WindowsSidecarDependencies =
    [
        "libwinpthread-1.dll",
        "libgcc_s_seh-1.dll",
        "libgcc_s_dw2-1.dll",
        "libstdc++-6.dll",
        "zlib1.dll"
    ];
}
