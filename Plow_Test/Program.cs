using PlowTruck;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Plow_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            PlowTruckCore test = new PlowTruckCore(Environment.CurrentDirectory + "\\TestPlow.xml");

            //test.ScanDirectory = @"C:\Users\Daedalus\Downloads";
            test.ScanDirectory = @"E:\Russell\Downloads";
            test.Scan();

            Console.WriteLine(String.Format("{0,0}{1,15}{2,15}{3,25}", "Folder", "Extension", "Action", "File"));
            //Console.WriteLine("File\tExtension\tFolder\tAction");

            XmlElement scan_root = test.ScanResults.DocumentElement;
            XmlNodeList scan_nodes = scan_root.SelectNodes("Result");
            foreach (XmlNode childNode in scan_nodes)
            {
                if (!(childNode.Attributes["action"].Value == "unmatched"))
                {
                    //<Result action="1" foldername="Text Documents" extension="txt">C:\Downloads\File.txt</Result>
                    Console.WriteLine(String.Format("{0,0}{1,15}{2,15}\t{3,25}", childNode.Attributes["foldername"].Value, childNode.Attributes["extension"].Value,
                                                            childNode.Attributes["action"].Value, childNode.InnerText));
                }
            }
            
            string linebreak = new String('=', 32);
            Console.WriteLine("{0}Unmatched Results{1}", linebreak, linebreak);
 
            foreach (XmlNode childNode in scan_nodes)
            {
                if (childNode.Attributes["action"].Value == "unmatched")
                {
                    //<Result action="1" foldername="Text Documents" extension="txt">C:\Downloads\File.txt</Result>
                    Console.WriteLine(String.Format("{0,0}{1,15}{2,15}\t{3,25}", childNode.Attributes["foldername"].Value, childNode.Attributes["extension"].Value,
                                                            childNode.Attributes["action"].Value, childNode.InnerText));
                }
            }

            Console.ReadLine();
            test.PlowDirectory = test.ScanDirectory;
            test.Plow();
            //test.ArchiveFile(@"E:\Test\Test.lcl", @"E:\Test\Plowed.zip");

            Console.ReadLine();
        }
    }
}
