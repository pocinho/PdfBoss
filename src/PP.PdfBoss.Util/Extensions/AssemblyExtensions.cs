/*  PP.PdfBoss.Util\Extensions\AssemblyExtensions.cs
 *
 *  Copyright 2024 Paulo Pocinho.
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace PP.PdfBoss.Util.Extensions;

public static class AssemblyExtensions
{
    public static void ExtractResource(this Assembly assembly, string name, string targetDir, bool overwriteFiles = true)
    {
        string resourcePath = assembly
            .GetManifestResourceNames()
            .Single(str => str.EndsWith(name));

        using Stream stream = assembly.GetManifestResourceStream(resourcePath)!;
        ZipFile.ExtractToDirectory(stream, targetDir, overwriteFiles: overwriteFiles);
    }
}
