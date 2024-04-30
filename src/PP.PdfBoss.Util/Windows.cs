/*  PP.PdfBoss.Util\Windows.cs
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

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PP.PdfBoss.Util;

public partial class Windows
{
    private const int SW_SHOWNORMAL = 1;

    [LibraryImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    public static void ShowExistingWindow()
    {
        Process currentProcess = Process.GetCurrentProcess();
        Process? targetProcess = Process.GetProcessesByName(currentProcess.ProcessName)
            .FirstOrDefault(wnd => wnd.MainWindowHandle != IntPtr.Zero);

        if (targetProcess != null)
        {
            ShowWindow(targetProcess.MainWindowHandle, SW_SHOWNORMAL);
            SetForegroundWindow(targetProcess.MainWindowHandle);
            return;
        }
    }
}
