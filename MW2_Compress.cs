using System;
using System.Collections;
using System.Xml;
using System.Diagnostics;
using System.IO;
namespace ffManager
{
    public class MW2_Compress
    {
        private ArrayList missing_files =  new ArrayList();
        private ArrayList overflow_files =  new ArrayList();
        private string fastfile;
        private string console;
        private string extractDir;
        private string dumpDir;
		private string hashDir;
        private ArrayList process_files = new ArrayList();
        private string DS = ffManager.MainClass.getOS() == "win32" ? @"\" : "/";
        private XmlDocument offsets;
        public MW2_Compress (string file, string console)
        {
            fastfile = file;
            this.console = console;
        }
        public void compress(string dir, string xml)
        {
            offsets = new XmlDocument();
            offsets.Load(xml);
            extractDir = dir + DS + "scripts";
            dumpDir = dir + DS + "raw";
			hashDir = dir + DS + "hashes";
            packData();
            ArrayList process_files = new ArrayList();
            Console.WriteLine("Compressing " + fastfile);
            XmlNodeList files = offsets.GetElementsByTagName("file");
            foreach(XmlNode file in files)
            {
                Console.WriteLine("Processing " + file.Attributes["name"].Value);
                foreach(XmlNode part in file.ChildNodes)
                {
                    if(!process_files.Contains(part.Attributes["name"].Value))
                    {
                        string source = locateDumpFile(part.Attributes["name"].Value);
                        if(source == "")
                        continue;
						string[] dump = getDumpName(dir);
                        Process ps = new Process();
                        ps.StartInfo.CreateNoWindow = true;
                        ps.StartInfo.WindowStyle= ProcessWindowStyle.Hidden;
                        string comp = "";
                        if(ffManager.MainClass.console == "ps3")
                        {
                            comp = "-w -15";
                        }
                        else if(ffManager.MainClass.console == "xbox")
                        {
                            comp = "";
                        }
                        if(ffManager.MainClass.getOS() == "win32")
                        {
                            ps.StartInfo.FileName =MainClass.cwd + @"\packzip.exe";
                            ps.StartInfo.Arguments = "-o 0x" + part.Attributes["name"].Value + " " + comp + @" """ + dumpDir + DS + source + @""" " + @"""" + dir + DS + dump[0] + @"""";
                        }
                        else if(ffManager.MainClass.getOS() == "unix")
                        {
                            ps.StartInfo.FileName = "wine";
                            ps.StartInfo.Arguments = @"""" + MainClass.cwd + @"/packzip.exe"" -o 0x" + part.Attributes["name"].Value + " " + comp + @" """ + dumpDir + DS + source + @""" " + @"""" + dir + DS + dump[0] + @"""";
                        }
						Console.WriteLine(ps.StartInfo.FileName + " " + ps.StartInfo.Arguments);
                        ps.Start();
                        ps.WaitForExit();
                    }
                }
            }
			compressDump(dir);
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
        public ArrayList getOverflow()
        {
            return overflow_files;
        }
        private void packData()
        {
            XmlNodeList doc = offsets.GetElementsByTagName("file");
            foreach(XmlNode file in doc)
            {
                long size = checkSize(file.Attributes["name"].Value,Convert.ToInt64(file.Attributes["size"].Value));
                if(size != -1 && hasChanged(file.Attributes["name"].Value))
                {
                    long pos = 0;
                    if(size >
                    0)
                    fillPadding(file.Attributes["name"].Value,size);
                    foreach(XmlNode part in file.ChildNodes)
                    {
                        packPart(part,file.Attributes["name"].Value, pos);
                        pos += Convert.ToInt64(part.Attributes["endpos"].Value) - Convert.ToInt64(part.Attributes["startpos"].Value);
                    }
                }
            }
        }
        private void packPart(XmlNode part, string file, long foffset)
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
	 BinaryReader file_fhandle = new BinaryReader(File.OpenRead(extractDir + DS + file));
            BinaryWriter source_fhandle = new BinaryWriter(File.OpenWrite(dumpDir + DS + source));
            long size = epos -  spos;
            long len = 0;
		try
            {
		source_fhandle.BaseStream.Seek(spos,SeekOrigin.Begin);
		file_fhandle.BaseStream.Seek(foffset,SeekOrigin.Begin);
                while(len <= size)
                {
                    source_fhandle.BaseStream.WriteByte(file_fhandle.ReadByte());
                    len++;
                }
            }
            catch(IOException ioex)
            {
                source_fhandle.Close();
                file_fhandle.Close();
            }
            source_fhandle.Close();
            file_fhandle.Close();
        }
        private long checkSize(string file, long size)
        {
            FileInfo finfo = new FileInfo(extractDir + DS + file);
            if(finfo.Length > size)
            {
                ArrayList data = new ArrayList();
                data.Add(file);
                data.Add(size);
                data.Add(finfo.Length - size);
                overflow_files.Add(data);
                return -1;
            }
            else
            return size - finfo.Length;
        }
        private bool hasChanged(string file)
        {
			string md5hash = MainClass.GetMD5HashFromFile(extractDir + DS + file);
            if(md5hash != File.ReadAllText(hashDir + DS + file + ".md5").Trim())
            {
                Console.WriteLine("File " + file + " has changed..");
                File.WriteAllText(hashDir + DS + file + ".md5",md5hash);
                return true;
            }
            else
            {
                Console.WriteLine("File " + file + " has NOT changed -- Skipping...");
                return false;
            }
        }
        private void fillPadding(string file, long num)
        {
            BinaryWriter fhandle= new BinaryWriter(File.Open(extractDir + DS + file,FileMode.Append,FileAccess.Write));
            try
            {
                for(long i=0; i < num; i++)
                {
                    fhandle.BaseStream.WriteByte(0x00);
                }
            }
            catch(IOException ioex)
            {
                fhandle.Close();
            }
            fhandle.Close();
        }
		private string[] getDumpName(string dir)
		{
			DirectoryInfo dirinfo = new DirectoryInfo(dir);
			string cdumpfile = "";
			string cdumpfileext = "";
			long cdumpfilesize = 0;
			foreach(FileInfo file in dirinfo.GetFiles())
			{
				if(file.Length > cdumpfilesize)
				{
					cdumpfile = file.Name;
					cdumpfilesize = file.Length;
					cdumpfileext = file.Extension;
				}
			}
			return new string[] {cdumpfile, cdumpfile.Replace(cdumpfileext,"")};
		}
		private void compressDump(string dir)
		{
			string[] dump = getDumpName(dir);
			string name = dump[1];
			string filename = dump[0];
			Process ps = new Process();
			ps.StartInfo.CreateNoWindow = true;
			ps.StartInfo.WindowStyle= ProcessWindowStyle.Hidden;
			string comp = "";
			if(ffManager.MainClass.console == "ps3")
			{
				comp = "-w -15";
			}
			else if(ffManager.MainClass.console == "xbox")
				comp = "";
			if(ffManager.MainClass.getOS() == "win32")
			{
				ps.StartInfo.FileName = MainClass.cwd  + @"\packzip.exe";
				ps.StartInfo.Arguments = "-o 0x" + name + " " + comp + @" """ + dir + DS + filename + @""" " + @"""" + fastfile + @"""";
			}
			else if(ffManager.MainClass.getOS() == "unix")
			{
				ps.StartInfo.FileName = "wine";
				ps.StartInfo.Arguments = @"""" + MainClass.cwd + @"./packzip.exe"" -o 0x" + name + " " + comp + @" """ + dir + DS + filename + @""" " + @"""" + fastfile + @"""";
			}
			Console.WriteLine(ps.StartInfo.FileName + " " + ps.StartInfo.Arguments);
			ps.Start();
			ps.WaitForExit();
		}
    }
}