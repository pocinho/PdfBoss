/*  PP.PdfBoss.Util\Checksum.cs
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

using PP.PdfBoss.Util.Extensions;

namespace PP.PdfBoss.Util;

public class Checksum
{
    public static async Task<string> HashFolder(string folderPath)
    {
        DirectoryInfo dir = new(folderPath);
        if (!dir.Exists)
        {
            return string.Empty;
        }

        using MD5 algorithm = MD5.Create();
        byte[] hash = await algorithm.ComputeHashAsync(dir);

        return BitConverter.ToString(hash).Replace("-", "");
    }
}
