/*
        MIT License
       
       Copyright (c) 2020-2024 Alastair Lundy
       
       Permission is hereby granted, free of charge, to any person obtaining a copy
       of this software and associated documentation files (the "Software"), to deal
       in the Software without restriction, including without limitation the rights
       to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
       copies of the Software, and to permit persons to whom the Software is
       furnished to do so, subject to the following conditions:
       
       The above copyright notice and this permission notice shall be included in all
       copies or substantial portions of the Software.
       
       THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
       IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
       FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
       AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
       LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
       OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
       SOFTWARE.
   */


using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using AlastairLundy.Extensions.System.Versioning;

using PlatformKit.Core.Identification.Enums;

using PlatformKit.Core.Internal.Exceptions;
using PlatformKit.Core.Internal.Localizations;

using PlatformKit.Core.OperatingSystems;

#if NETSTANDARD2_0
    using OperatingSystem = PlatformKit.Extensions.OperatingSystem.OperatingSystemExtension;
#endif

// ReSharper disable InconsistentNaming

namespace PlatformKit.Core.Identification
{
    /// <summary>
    /// A class to manage RuntimeId detection and programmatic generation.
    /// </summary>
    public class RuntimeIdentification
    {

        /// <summary>
        /// Returns the CPU architecture as a string in the format that a RuntimeID uses.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        protected static string GetArchitectureString()
        {
#if NET5_0_OR_GREATER
            return RuntimeInformation.OSArchitecture switch
            {
                Architecture.X64 => "x64",
                Architecture.X86 => "x86",
                Architecture.Arm => "arm",
                Architecture.Arm64 => "arm64",
#if NET6_0_OR_GREATER
                Architecture.S390x => "s390x",
                Architecture.Wasm => throw new PlatformNotSupportedException(),
#endif
                _ => null
            } ?? throw new InvalidOperationException();
#else
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.Arm:
                    return "arm";
                case Architecture.Arm64:
                    return "arm64";
                case Architecture.X64:
                    return "x64";
                case Architecture.X86:
                    return "x86";
                default:
                    throw new PlatformNotSupportedException();
            }
#endif
        }

        /// <summary>
        /// Returns the OS name as a string in the format that a RuntimeID uses.
        /// </summary>
        /// <param name="identifierType"></param>
        /// <returns></returns>
        protected static string GetOsNameString(RuntimeIdentifierType identifierType)
        {
#if NET5_0_OR_GREATER
            string? osName = null;
#else
            string osName = null;
#endif
            
            if (identifierType == RuntimeIdentifierType.AnyGeneric)
            {
                osName = "any";
            }
            else
            {
                if (OperatingSystem.IsWindows())
                {
                    osName = "win";
                }
                if (OperatingSystem.IsMacOS())
                {
                    osName = "osx";
                }
                if (OperatingSystem.IsFreeBSD())
                {
                    osName = "freebsd";
                }
#if NET6_0_OR_GREATER
                if (OperatingSystem.IsAndroid())
                {
                    osName = "android";
                }
                if (OperatingSystem.IsIOS())
                {
                    osName = "ios";
                }
                if (OperatingSystem.IsTvOS())
                {
                    osName = "tvos";
                }

                if (OperatingSystem.IsWatchOS())
                {
                    osName = "watchos";
                }
#endif
                if (OperatingSystem.IsLinux())
                {
                    if (identifierType == RuntimeIdentifierType.Generic)
                    {
                        osName = "linux";
                    }
                    else if (identifierType == RuntimeIdentifierType.Specific)
                    {
                        osName = LinuxOperatingSystem.GetOsReleasePropertyValue("IDENTIFIER_LIKE=");
                    }
                    else if (identifierType == RuntimeIdentifierType.DistroSpecific || identifierType == RuntimeIdentifierType.VersionLessDistroSpecific)
                    {
                        osName = LinuxOperatingSystem.GetOsReleasePropertyValue("IDENTIFIER=");
                    }
                    else
                    {
                        osName = "linux";
                    }
                }
            }

            if (osName == null)
            {
                throw new PlatformNotSupportedException();
            }

            return osName;
        }
        
        /// <summary>
        /// Returns the OS version as a string in the format that a RuntimeID uses.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        internal static string GetOsVersionString()
        {
#if NET5_0_OR_GREATER
            string? osVersion = null;
#else
            string osVersion = null;
#endif
            if (OperatingSystem.IsWindows())
            {
                bool isWindows10 = WindowsOperatingSystem.IsWindows10();
                bool isWindows11 = WindowsOperatingSystem.IsWindows11();
                
                if (isWindows10)
                {
                    osVersion = "10";
                }
                else if (isWindows11)
                {
                    osVersion = "11";
                }
                else if (!isWindows10 && !isWindows11)
                {
#if NET5_0_OR_GREATER
                    osVersion = WindowsOperatingSystem.GetWindowsVersion().Build switch
                    {
                        < 9200 => throw new PlatformNotSupportedException(),
                        9200 => "8",
                        9600 => "81",
                        _ => throw new PlatformNotSupportedException(Resources.Exceptions_PlatformNotSupported_WindowsOnly)
                    };
#else
                    switch (WindowsOperatingSystem.GetWindowsVersion().Build)
                    {
                        case 9200:
                            osVersion = "8";
                            break;
                        case 9600:
                            osVersion = "81";
                            break;
                        default:
                            throw new PlatformNotSupportedException(Resources
                                .Exceptions_PlatformNotSupported_WindowsOnly);
                    }
#endif
                }
            }
            if (OperatingSystem.IsLinux())
            {
                osVersion = LinuxOperatingSystem.GetLinuxDistributionVersion().ToString();
            }
            if (OperatingSystem.IsFreeBSD())
            {
                osVersion = FreeBsdOperatingSystem.GetFreeBSDVersion().ToString();
                
                switch (osVersion.CountDotsInString())
                {
                    case 3:
                        osVersion = osVersion.Remove(osVersion.Length - 4, 4);
                        break;
                    case 2:
                        osVersion = osVersion.Remove(osVersion.Length - 2, 2);
                        break;
                    case 1:
                        break;
                    case 0:
                        osVersion = $"{osVersion}.0";
                        break;
                    }
            }
            if (OperatingSystem.IsMacOS())
            {
                bool isAtLeastHighSierra = OperatingSystem.IsMacOSVersionAtLeast(10, 13);

                Version version = MacOperatingSystem.GetMacOsVersion();

                if (isAtLeastHighSierra)
                {
                    
                    if (OperatingSystem.IsMacOSVersionAtLeast(11, 0))
                    {
                        osVersion = $"{version.Major}";
                    }
                    else
                    {
                        osVersion = $"{version.Major}.{version.Major}";
                    }
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }
            }

            return osVersion;
        }

        /// <summary>
        /// Programmatically generates a .NET Runtime Identifier.
        /// Note: Microsoft advises against programmatically creating Runtime IDs but this may be necessary in some instances.
        /// For More Information Visit: https://learn.microsoft.com/en-us/dotnet/core/rid-catalog
        /// </summary>
        /// <param name="identifierType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string GenerateRuntimeIdentifier(RuntimeIdentifierType identifierType)
        {
            if (identifierType == RuntimeIdentifierType.AnyGeneric)
            {
                return GenerateRuntimeIdentifier(identifierType, false, false);
            }

            if (identifierType == RuntimeIdentifierType.Generic ||
                identifierType == RuntimeIdentifierType.Specific && (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD()) ||
                identifierType == RuntimeIdentifierType.VersionLessDistroSpecific)
            {
                return GenerateRuntimeIdentifier(identifierType, true, false);
            }

            if (identifierType == RuntimeIdentifierType.Specific && (!OperatingSystem.IsLinux() && !OperatingSystem.IsFreeBSD())
                || identifierType == RuntimeIdentifierType.DistroSpecific)
            {
                return GenerateRuntimeIdentifier(identifierType, true, true);
            }

            throw new ArgumentException();
        }
        
        /// <summary>
        /// Programmatically generates a .NET Runtime Identifier based on the system calling the method.
        /// Note: Microsoft advises against programmatically creating Runtime IDs but this may be necessary in some instances.
        /// For More Information Visit: https://learn.microsoft.com/en-us/dotnet/core/rid-catalog
        /// </summary>
        /// <returns></returns>
        public static string GenerateRuntimeIdentifier(RuntimeIdentifierType identifierType, bool includeOperatingSystemName, bool includeOperatingSystemVersion)
        {
            string osName = GetOsNameString(identifierType);
            string cpuArch = GetArchitectureString();
            
            if (identifierType == RuntimeIdentifierType.AnyGeneric ||
                identifierType == RuntimeIdentifierType.Generic && includeOperatingSystemName == false)
            {
                return $"any-{GetArchitectureString()}";
            }

            if (identifierType == RuntimeIdentifierType.Generic && includeOperatingSystemName)
            {
                return $"{osName}-{cpuArch}";
            }

            if (identifierType == RuntimeIdentifierType.Specific ||
                OperatingSystem.IsLinux() && identifierType == RuntimeIdentifierType.DistroSpecific ||
                OperatingSystem.IsLinux() && identifierType == RuntimeIdentifierType.VersionLessDistroSpecific)
            {
                string osVersion = GetOsVersionString();

                if (OperatingSystem.IsWindows())
                {
                    if (includeOperatingSystemVersion)
                    {
                        return $"{osName}{osVersion}-{cpuArch}";
                    }

                    return $"{osName}-{cpuArch}";
                }

                if (OperatingSystem.IsMacOS())
                {
                    if (includeOperatingSystemVersion)
                    {
                        return $"{osName}.{osVersion}-{cpuArch}";
                    }

                    return $"{osName}-{cpuArch}";
                }

                if (((OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD()) && 
                        (identifierType == RuntimeIdentifierType.DistroSpecific) || identifierType == RuntimeIdentifierType.VersionLessDistroSpecific))
                {
                    if (includeOperatingSystemVersion)
                    {
                        return $"{osName}.{osVersion}-{cpuArch}";
                    }

                    return $"{osName}-{cpuArch}";
                }
                if (((OperatingSystem.IsLinux() && identifierType == RuntimeIdentifierType.Specific) && includeOperatingSystemVersion == false) ||
                    includeOperatingSystemVersion == false)
                {
                    return $"{osName}-{cpuArch}";
                }
            }
            else if((!OperatingSystem.IsLinux() && !OperatingSystem.IsFreeBSD()) && (identifierType == RuntimeIdentifierType.DistroSpecific || identifierType == RuntimeIdentifierType.VersionLessDistroSpecific))
            {
                Console.WriteLine(Resources.RuntimeInformation_NonLinuxSpecific_Warning);
                return GenerateRuntimeIdentifier(RuntimeIdentifierType.Specific);
            }

            throw new RuntimeIdentifierGenerationException();
        }

        /// <summary>
        /// Detects the RuntimeID if running on .NET 5 or later and generates the generic RuntimeID if running on .NET Standard 2.0 or later.
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public static string GetRuntimeIdentifier()
        {
#if NET5_0_OR_GREATER
            return RuntimeInformation.RuntimeIdentifier;
#else
            if (OperatingSystem.IsLinux())
            {
                return GenerateRuntimeIdentifier(RuntimeIdentifierType.DistroSpecific);
            }

            return GenerateRuntimeIdentifier(RuntimeIdentifierType.Specific);
#endif
        }

        /// <summary>
        /// Detects possible Runtime Identifiers that could be applicable to the system calling the method.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<RuntimeIdentifierType, string> GetPossibleRuntimeIdentifierCandidates()
        {
            Dictionary<RuntimeIdentifierType, string> possibilities = new Dictionary<RuntimeIdentifierType, string>
            {
                { RuntimeIdentifierType.AnyGeneric, GenerateRuntimeIdentifier(RuntimeIdentifierType.AnyGeneric) },
                { RuntimeIdentifierType.Generic, GenerateRuntimeIdentifier(RuntimeIdentifierType.Generic) },
                { RuntimeIdentifierType.Specific, GenerateRuntimeIdentifier(RuntimeIdentifierType.Specific) }
            };

            if (OperatingSystem.IsLinux())
            {
                possibilities.Add(RuntimeIdentifierType.VersionLessDistroSpecific,GenerateRuntimeIdentifier(RuntimeIdentifierType.DistroSpecific, true, false));
                possibilities.Add(RuntimeIdentifierType.DistroSpecific,GenerateRuntimeIdentifier(RuntimeIdentifierType.DistroSpecific));
            }

            return possibilities;
        }
    }
}