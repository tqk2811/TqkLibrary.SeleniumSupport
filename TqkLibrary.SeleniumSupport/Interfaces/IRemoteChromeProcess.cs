using OpenQA.Selenium.Chrome;
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
        Task<ChromeOptions> GetChromeOptionsAsync(CancellationToken cancellationToken = default);
    }
}
