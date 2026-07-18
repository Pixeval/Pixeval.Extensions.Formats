using System;
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
    private static readonly Lock _InitializationLock = new();
    private static bool _IsInitialized;

    public static void Configure()
    {
        lock (_InitializationLock)
        {
            if (!_IsInitialized)
            {
                NativeLibrary.SetDllImportResolver(typeof(QuestPDF.Settings).Assembly, ResolveNativeLibrary);
                _IsInitialized = true;
            }
        }

        QuestPDF.Settings.License = LicenseType.Community;

        var extensionDirectory = ExtensionsHostBase.ExtensionDirectory;
        if (!string.IsNullOrWhiteSpace(extensionDirectory))
        {
            AddFontDiscoveryPath(extensionDirectory);
            AddFontDiscoveryPath(Path.Combine(extensionDirectory, "LatoFont"));
        }
    }

    private static IntPtr ResolveNativeLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        var extensionDirectory = ExtensionsHostBase.ExtensionDirectory;
        if (string.IsNullOrWhiteSpace(extensionDirectory))
            return 0;

        var fileName = GetMappedLibraryName(libraryName);
        var candidates = new[]
        {
            Path.Combine(extensionDirectory, fileName),
            Path.Combine(extensionDirectory, "runtimes", GetRuntimeIdentifier(), "native", fileName)
        };

        foreach (var candidate in candidates)
            if (NativeLibrary.TryLoad(candidate, out var handle))
                return handle;

        return 0;
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
}
