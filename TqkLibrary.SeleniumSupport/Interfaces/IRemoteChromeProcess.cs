using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.SeleniumSupport.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IControlChromeProcess
    {
        /// <summary>
        /// Should be of the form "{hostname|IP address}:port.
        /// </summary>
        Task<string> GetDebuggerAddressAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// 
        /// </summary>
        Task<bool> GetIsOpenChromeAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// 
        /// </summary>
        Task OpenChromeAsync(string? advArgs = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// 
        /// </summary>
        Task CloseChromeAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string?> GetBinaryLocation(CancellationToken cancellationToken = default);
    }
}
