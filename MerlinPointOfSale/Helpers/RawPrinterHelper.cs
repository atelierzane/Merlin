using System;
using System.IO;
using System.Runtime.InteropServices;
using System.IO.Ports;
using System.Drawing.Printing;

public class RawPrinterHelper
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private class DOCINFOA
    {
        [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
        [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile;
        [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
    }

    [DllImport("winspool.Drv", EntryPoint = "ReadPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool ReadPrinter(IntPtr hPrinter, IntPtr pBuffer, int cbBuffer, out int bytesRead);


    [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

    [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

    [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

    // SendBytesToPrinter()
    // When the function is given a printer name and an unmanaged array of bytes, the function sends those bytes to the print queue.
    // Returns true on success, false on failure.
    public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, int dwCount)
    {
        bool bSuccess = false; // Assume failure unless you specifically succeed.
        IntPtr hPrinter = new IntPtr(0);
        int dwError = 0, dwWritten = 0;
        DOCINFOA di = new DOCINFOA();
        di.pDocName = "RAW Document";
        di.pDataType = "RAW";

        // Open the printer.
        if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
        {
            // Start a document.
            if (StartDocPrinter(hPrinter, 1, di))
            {
                // Start a page.
                if (StartPagePrinter(hPrinter))
                {
                    // Write your bytes.
                    bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                    EndPagePrinter(hPrinter);
                }
                EndDocPrinter(hPrinter);
            }
            ClosePrinter(hPrinter);
        }
        // If you did not succeed, GetLastError may give more information about why not.
        if (bSuccess == false)
        {
            dwError = Marshal.GetLastWin32Error();
        }
        return bSuccess;
    }
    public static byte[] SendBytesToPrinterWithResponse(string printerName, byte[] command)
    {
        IntPtr hPrinter = IntPtr.Zero;
        try
        {
            if (OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
            {
                // Allocate unmanaged memory for the command
                IntPtr pCommand = Marshal.AllocHGlobal(command.Length);
                Marshal.Copy(command, 0, pCommand, command.Length);

                // Write the command to the printer
                WritePrinter(hPrinter, pCommand, command.Length, out _);

                // Allocate buffer for the response
                byte[] buffer = new byte[256];
                IntPtr pBuffer = Marshal.AllocHGlobal(buffer.Length);

                // Read the response from the printer
                ReadPrinter(hPrinter, pBuffer, buffer.Length, out int bytesRead);

                // Copy the response to a managed array
                Marshal.Copy(pBuffer, buffer, 0, bytesRead);

                // Free the allocated memory
                Marshal.FreeHGlobal(pCommand);
                Marshal.FreeHGlobal(pBuffer);

                // Return the actual response data
                return buffer[..bytesRead];
            }
            else
            {
                throw new InvalidOperationException("Failed to open the printer.");
            }
        }
        finally
        {
            if (hPrinter != IntPtr.Zero)
            {
                ClosePrinter(hPrinter);
            }
        }
    }
    public static bool SendBytesToPrinter(string szPrinterName, byte[] bytes)
    {
        bool result;
        int size = bytes.Length;
        IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(size);
        Marshal.Copy(bytes, 0, pUnmanagedBytes, size);
        result = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, size);
        Marshal.FreeCoTaskMem(pUnmanagedBytes);
        return result;
    }
}
