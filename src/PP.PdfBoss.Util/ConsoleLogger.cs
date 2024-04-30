/*  PP.PdfBoss.Util\ConsoleLogger.cs
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

using System.Text;

namespace PP.PdfBoss.Util;

public class ConsoleLogger : LinkedList<string>
{
    private readonly int _maxLines;

    public ConsoleLogger(int maxLines)
    {
        _maxLines = maxLines;
    }

    public string Push(string? line)
    {
        if (!string.IsNullOrEmpty(line))
            AddLast(line);

        if (Count > _maxLines)
            RemoveFirst();

        return Log();
    }

    public string Log()
    {
        StringBuilder concatenation = new();

        foreach (string? element in this)
            concatenation.AppendLine(element);

        return concatenation.ToString();
    }
}
