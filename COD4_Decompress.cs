using System;
using System.Collections;
using System.Xml;
using System.Diagnostics;
using System.IO;
namespace ffManager
{
	public class COD4_Decompress
	{
		private ArrayList missing_files =  new ArrayList();
		private string fastfile;
		private string console;
		private string extractDir;
		private string dumpDir;
		private string hashDir;
		private string DS = ffManager.MainClass.getOS() == "win32" ? @"\" : "/";
		private XmlDocument offsets;
		public COD4_Decompress (string file, string console)
		{
			fastfile = file;
			this.console = console;
		
		}
		
		public void decompress(string dir, string xml)
		{
			offsets = new XmlDocument();
			offsets.Load(xml);
			if(Directory.Exists(dir)) Directory.Delete(dir,true);
			extractDir = dir + DS + "scripts";
			dumpDir = dir + DS + "raw";
			hashDir = dir + DS + "hashes";
			Directory.CreateDirectory(dir);
			Directory.CreateDirectory(extractDir);
			Directory.CreateDirectory(dumpDir);
			Directory.CreateDirectory(hashDir);
			Process ps = new Process();
			ps.StartInfo.CreateNoWindow = true;
			ps.StartInfo.WindowStyle= ProcessWindowStyle.Hidden;
			string decomp = "";
			if(ffManager.MainClass.console == "ps3")
			{
				decomp = "-z -15";
			}
			else if(ffManager.MainClass.console == "xbox")
			{
				decomp = "";
			}
			if(ffManager.MainClass.getOS() == "win32")
			{
				ps.StartInfo.FileName = @".\offzip.exe";
				ps.StartInfo.Arguments = "-a " + decomp + @" """ + fastfile + @""" " + @"""" + dumpDir + @""" 0";
			}
			else if(ffManager.MainClass.getOS() == "unix")
			{
				ps.StartInfo.FileName = "wine";
				ps.StartInfo.Arguments = "./offzip.exe -a " + decomp + @" """ + fastfile + @""" " + @"""" + dumpDir + @""" 0";
			}
			Console.WriteLine(ps.StartInfo.FileName + " " + ps.StartInfo.Arguments);
			ps.Start();
			ps.WaitForExit();
			writeScripts();
		}
		
		private void writeScripts()
		{
			XmlNodeList files = offsets.GetElementsByTagName("file");
			foreach(XmlNode file in files)
			{
				Console.WriteLine("Processing " + file.Attributes["name"].Value);
				if(!File.Exists(extractDir + DS + file))
					File.WriteAllText(extractDir + DS +  file.Attributes["name"].Value,"");
				extractData(file);
				File.WriteAllText(extractDir + DS + file.Attributes["name"].Value + ".md5",MainClass.GetMD5HashFromFile(extractDir + DS + file.Attributes["name"].Value));
			}
		}
		private void extractData(XmlNode data)
		{
			XmlNodeList dats = data.ChildNodes;
			foreach(XmlNode dat in dats)
			{
				extractPart(dat, data.Attributes["name"].Value);
			}
		}
		private void extractPart(XmlNode part, string file)
		{
			string source = locateDumpFile(part.Attributes["name"].Value);
			if(source == "")
			{
				ArrayList data = new ArrayList();
				data.Add(part.Attributes["name"].Value);
				data.Add(file);
				missing_files.Add(data);
				return;
			}
			long spos = Convert.ToInt64(part.Attributes["startpos"].Value);
			long epos = Convert.ToInt64(part.Attributes["endpos"].Value);
			BinaryReader source_handle = new BinaryReader( File.OpenRead(dumpDir + DS + source));
			BinaryWriter file_fhandle = new BinaryWriter( File.Open(extractDir + DS + file,FileMode.Append,FileAccess.Write));
			source_handle.BaseStream.Seek(spos, SeekOrigin.Begin);
			long size = epos - spos;
			long len = 0;
			try
			{
				while(len <= size)
				{
					byte data = source_handle.ReadByte();
					if(data != 0x00)
						file_fhandle.BaseStream.WriteByte(data);
					len++;
				}
			}
			catch (EndOfStreamException feof)
			{
				source_handle.Close();
				file_fhandle.Close();
			}
			source_handle.Close();
			file_fhandle.Close();
		}
		private string locateDumpFile(string name)
		{
			DirectoryInfo files = new DirectoryInfo(dumpDir);
			foreach(FileInfo finfo in files.GetFiles())
			{
				if(finfo.Name.Replace(finfo.Extension,"") == name)
					return finfo.Name;
			}
			return "";
		}
		public ArrayList getMissingFiles()
		{
			return missing_files;
		}
	}
}

