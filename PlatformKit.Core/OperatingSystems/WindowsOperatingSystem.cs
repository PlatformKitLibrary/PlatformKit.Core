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
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

using AlastairLundy.Extensions.System;

#if NETSTANDARD2_0
using OperatingSystem = PlatformKit.Extensions.OperatingSystem.OperatingSystemExtension;
#endif

namespace PlatformKit.Core.OperatingSystems
{

    public class WindowsOperatingSystem
    {
        /// <summary>
        /// Detects Windows Version and returns it as a System.Version
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        // ReSharper disable once MemberCanBePrivate.Global
#if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
        public static Version GetWindowsVersion()
        {
            if (OperatingSystem.IsWindows())
            {
                return Version.Parse(RuntimeInformation.OSDescription
                    .Replace("Microsoft Windows", string.Empty)
                    .Replace(" ", string.Empty));
            }

            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Returns whether a specified version of Windows is Windows 10.
        /// </summary>
        /// <returns>true if a version of Windows is Windows 10</returns>
#if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
        public static bool IsWindows10()
        {
            if (OperatingSystem.IsWindows())
            {
                return IsWindows10(GetWindowsVersion());
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        /// <summary>
        /// Returns whether a specified version of Windows is Windows 10.
        /// </summary>
        /// <returns>true if a version of Windows is Windows 10</returns>
#if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
        public static bool IsWindows10(Version version)
        {
            return version.IsAtLeast(new Version(10, 0, 10240))
                   && version.IsOlderThan(new Version(10, 0, 20349));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
#if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
        public static bool IsWindows11()
        {
            if (OperatingSystem.IsWindows())
            {
                return IsWindows11(GetWindowsVersion());
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        /// <summary>
        /// Returns whether a specified version of Windows is Windows 10.
        /// </summary>
        /// <returns>true if a version of Windows is Windows 10</returns>
#if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
        public static bool IsWindows11(Version version)
        {
            return version.IsAtLeast(new Version(10, 0, 22000))
                   && version.IsOlderThan(new Version(10, 0, 27000));
        }
    }
}