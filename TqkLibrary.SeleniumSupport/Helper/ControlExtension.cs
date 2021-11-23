using ICSharpCode.SharpZipLib.Zip;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using TqkLibrary.SeleniumSupport.DataClass;

namespace TqkLibrary.SeleniumSupport.Helper
{
    public static class ControlExtension
    {
        public static void GenExtension(string path)
        {
            string background_ = Resource.Control_Ext_background;


            if (File.Exists(path)) try { File.Delete(path); } catch { }

            using ZipFile zipFile = ZipFile.Create(path);
            zipFile.BeginUpdate();
            using CustomStaticDataSource manifest = new CustomStaticDataSource(Resource.Control_Ext_manifest);
            zipFile.Add(manifest, "manifest.json");
            using CustomStaticDataSource background = new CustomStaticDataSource(background_);
            zipFile.Add(background, "background.js");
            zipFile.CommitUpdate();
            zipFile.Close();
        }


        public static ChromeDriver SetProxy(this ChromeDriver chromeDriver, Proxy proxy)
        {
            chromeDriver.ExecuteScript("window.postMessage({ type: 'SetProxy', data: arguments[0]},'*');", proxy);
            return chromeDriver;
        }
        public static ChromeDriver ChangeUA(this ChromeDriver chromeDriver, string UA)
        {
            chromeDriver.ExecuteScript("window.postMessage({ type: 'ChangeUA', data: arguments[0]},'*');", UA);
            return chromeDriver;
        }
    }
}
