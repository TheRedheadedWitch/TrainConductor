using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVTrainConductor;

internal static class Clipboard
{
    private const uint CF_UNICODETEXT = 13;
    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)] private static extern bool OpenClipboard(IntPtr hWndNewOwner);
    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)] private static extern bool CloseClipboard();
    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)] private static extern IntPtr GetClipboardData(uint uFormat);
    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)] private static extern bool IsClipboardFormatAvailable(uint format);
    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr GlobalLock(IntPtr hMem);
    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)] private static extern bool GlobalUnlock(IntPtr hMem);

    internal static string? GetClipboardText()
    {
        if (!IsClipboardFormatAvailable(CF_UNICODETEXT)) return null;
        if (!OpenClipboard(IntPtr.Zero)) return null;
        IntPtr handle = IntPtr.Zero, pointer = IntPtr.Zero;
        string? result = null;
        try
        {
            handle = GetClipboardData(CF_UNICODETEXT);
            if (handle == IntPtr.Zero) return null;
            pointer = GlobalLock(handle);
            if (pointer == IntPtr.Zero) return null;
            result = System.Runtime.InteropServices.Marshal.PtrToStringUni(pointer);
        }
        finally
        {
            if (pointer != IntPtr.Zero) GlobalUnlock(handle);
            CloseClipboard();
        }
        return result;
    }
}