using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string codeDirPath = Path.GetFullPath(System.Environment.CurrentDirectory + @"\..\..\..\..\..\");
            Console.WriteLine(codeDirPath);
            string appProjFilePath = Directory.GetFiles(codeDirPath, "*.csproj", SearchOption.AllDirectories).SingleOrDefault(x => !Path.GetFileName(x).StartsWith("Msix") && !Path.GetFileName(x).StartsWith("Test"));
            Console.WriteLine(appProjFilePath);
            string packagerProjFile = Directory.GetFiles(codeDirPath, "*.appxmanifest", SearchOption.AllDirectories).FirstOrDefault();
            Console.WriteLine(packagerProjFile);

            if (!File.Exists(appProjFilePath)) throw new Exception("App project not found");
            if (!File.Exists(packagerProjFile)) throw new Exception("Packager project not found");

            XDocument xdocSrc = XDocument.Load(appProjFilePath);
            string version = xdocSrc.Root.Descendants("Version").Single().Value;

            XNamespace xmlns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
            XDocument xdocTrg = XDocument.Load(packagerProjFile);
            XElement test = xdocTrg.Root.Descendants(xmlns + "Identity").Single();
            xdocTrg.Root.Descendants(xmlns + "Identity").Single().Attribute("Version").Value = version + ".0";
            xdocTrg.Save(packagerProjFile);

            Console.WriteLine($"Updated installer package version to: {version}.0");
        }
    }
}
