using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Serialization;
using System.IO;

public class SYSTEMTIME
{
    public ushort wYear;
    public ushort wMonth;
    public ushort wDayOfWeek;
    public ushort wDay;
    public ushort wHour;
    public ushort wMinute;
    public ushort wSecond;
    public ushort wMilliseconds;
}

public class LibWrap
{
    [DllImport("Kernel32.dll")]
    public static extern void GetSystemTime([In, Out] SYSTEMTIME st);
}