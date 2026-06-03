using System;
using System.Diagnostics;
using TqkLibrary.WinApi.Helpers;

namespace TqkLibrary.SeleniumSupport.Windows
{
    /// <summary>
    /// Bản <see cref="BaseChromeProfile"/> dành cho Windows: đóng Chrome "mềm" bằng cách gửi
    /// WM_CLOSE tới từng cửa sổ của tiến trình (qua TqkLibrary.WinApi) thay vì kill thẳng.
    /// </summary>
    public abstract class WindowsChromeProfile : BaseChromeProfile
    {
        const uint WM_CLOSE = 0x0010;

        /// <inheritdoc/>
        protected override void RequestCloseProcessWindows(Process process)
        {
            ProcessHelper processHelper = new ProcessHelper(process.Id);
            foreach (var window in processHelper.WindowsTree)
            {
                window.SendMessage(WM_CLOSE, UIntPtr.Zero, IntPtr.Zero);
            }
        }
    }
}
