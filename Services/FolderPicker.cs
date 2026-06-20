using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace backend.Services;

/// <summary>
/// Minimal COM-interop wrapper around the Vista-style folder picker
/// (IFileOpenDialog with the FOS_PICKFOLDERS option). Shows the standard
/// Windows "Select Folder" Explorer dialog so the user can browse to a folder
/// instead of relying on a hard-coded path.
///
/// Windows-only — callers must guard with <see cref="OperatingSystem.IsWindows"/>.
/// The COM declarations compile on any platform; they are simply never invoked
/// off Windows (e.g. in the Linux Docker image).
/// </summary>
[SupportedOSPlatform("windows")]
internal static class FolderPicker
{
    /// <summary>
    /// Opens the Windows folder picker and returns the chosen path,
    /// or null if the user cancels the dialog.
    /// </summary>
    public static string? PickFolder(string? title = null)
    {
        // The shell dialog must run on an STA thread; a console app's main thread
        // is MTA, so spin up a dedicated STA thread for the dialog.
        string? result = null;
        Thread thread = new(() => result = PickFolderCore(title));
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        return result;
    }

    private static string? PickFolderCore(string? title)
    {
        IFileOpenDialog dialog = (IFileOpenDialog)new FileOpenDialog();
        try
        {
            dialog.GetOptions(out uint options);
            dialog.SetOptions(options | FOS_PICKFOLDERS | FOS_FORCEFILESYSTEM);

            if (!string.IsNullOrEmpty(title))
                dialog.SetTitle(title);

            // Show returns S_OK (0) when the user confirms a selection. Cancel yields
            // HRESULT_FROM_WIN32(ERROR_CANCELLED), which we treat as "no folder".
            if (dialog.Show(IntPtr.Zero) != 0)
                return null;

            dialog.GetResult(out IShellItem item);
            item.GetDisplayName(SIGDN_FILESYSPATH, out IntPtr pszPath);
            try
            {
                return Marshal.PtrToStringUni(pszPath);
            }
            finally
            {
                Marshal.FreeCoTaskMem(pszPath);
            }
        }
        finally
        {
            Marshal.ReleaseComObject(dialog);
        }
    }

    private const uint FOS_PICKFOLDERS = 0x00000020;
    private const uint FOS_FORCEFILESYSTEM = 0x00000040;
    private const uint SIGDN_FILESYSPATH = 0x80058000;

    [ComImport, Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
    private class FileOpenDialog { }

    // Methods we don't call are declared as no-arg placeholders solely to reserve
    // their vtable slots so the slots we DO call line up at the right offsets.
    [ComImport, Guid("D57C7288-D4AD-4768-BE02-9D969532D960"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFileOpenDialog
    {
        // IModalWindow
        [PreserveSig] int Show(IntPtr parent);
        // IFileDialog
        void SetFileTypes();
        void SetFileTypeIndex();
        void GetFileTypeIndex();
        void Advise();
        void Unadvise();
        void SetOptions(uint fos);
        void GetOptions(out uint pfos);
        void SetDefaultFolder();
        void SetFolder();
        void GetFolder();
        void GetCurrentSelection();
        void SetFileName();
        void GetFileName();
        void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
        void SetOkButtonLabel();
        void SetFileNameLabel();
        void GetResult(out IShellItem ppsi);
        // (remaining IFileDialog / IFileOpenDialog methods omitted — never called)
    }

    [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItem
    {
        void BindToHandler();
        void GetParent();
        void GetDisplayName(uint sigdnName, out IntPtr ppszName);
        void GetAttributes();
        void Compare();
    }
}
