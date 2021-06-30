using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Text;

namespace TqkLibrary.SeleniumSupport
{
  internal class CustomStaticDataSource : IStaticDataSource, IDisposable
  {
    private readonly MemoryStream memoryStream;

    public CustomStaticDataSource(string content)
    {
      this.memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
    }

    public void Dispose() => memoryStream.Dispose();

    public Stream GetSource() => memoryStream;
  }
}