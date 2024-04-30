/*  PP.PdfBoss.Util\Extensions\HashAlgorithmExtensions.cs
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
using System.Security.Cryptography;
using System.Text;

namespace PP.PdfBoss.Util.Extensions;

public static class HashAlgorithmExtensions
{
    public static async Task<byte[]> ComputeHashAsync(this HashAlgorithm alg, DirectoryInfo dir, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
    {
        IEnumerable<FileInfo> orderedFiles = dir
            .EnumerateFiles(searchPattern, searchOption)
            .OrderBy(f => f.FullName);

        using (CryptoStream cs = new(Stream.Null, alg, CryptoStreamMode.Write))
        {
            foreach (FileInfo file in orderedFiles)
            {
                byte[] pathBytes = Encoding.UTF8.GetBytes(Path.GetRelativePath(dir.FullName, file.FullName));
                cs.Write(pathBytes, 0, pathBytes.Length);

                using FileStream fs = file.OpenRead();
                await fs.CopyToAsync(cs);
            }

            cs.FlushFinalBlock();
        }

        return alg.Hash!;
    }
}
