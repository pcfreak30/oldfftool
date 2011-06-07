using System;
using System.IO;
using System.Diagnostics;
using System.Xml;
namespace ffManager
{
	namespace Decompress.PS3
	{
		public class Decompressor
		{
			private string fastfile;
			private string slash;
			private string workdir;
			private string dumpdir;
			private string filesdir;
			private string os;
			private ffProfile profile;
			public Decompressor (string file, ffProfile profile)
			{
				this.fastfile = file;
				this.os = MainWindow.getOS();
				if(this.os == "win32") this.slash = @"\";
				else this.slash = "/";
				FileInfo filei = new FileInfo(this.fastfile);
				this.workdir = filei.DirectoryName + this.slash + filei.Name.Replace(filei.Extension,"") + "_work" + this.slash;
				this.dumpdir = this.workdir + "dump" +this.slash;
				this.filesdir = this.workdir + "files"+ this.slash;
				this.profile = profile;
			}
			public void decompress()
			{
				ffInfo fastfle_info = new ffInfo(this.fastfile);
				if(fastfle_info.getVersion() == "mw2")
				{
					this.decompress_dump();
					return;
				}
				ProcessStartInfo psinfo = new ProcessStartInfo();
				psinfo.UseShellExecute = true;
				psinfo.WindowStyle = ProcessWindowStyle.Normal;
				psinfo.CreateNoWindow = true;
				if(this.os == "unix")
				{
					psinfo.FileName = "wine";
					psinfo.Arguments = "offzip -a -z -15" + @"""" + this.fastfile + @"""" + " " + @"""" + this.dumpdir + @"""" + " 0";
					
				}
				else if(this.os == "win32")
				{
					psinfo.FileName = "offzip";
					psinfo.Arguments = "-a -z -15" + @"""" + this.fastfile + @"""" + " " + @"""" + this.dumpdir + @"""" + " 0";
				}
				Process ps = new Process();
				ps.StartInfo = psinfo;
				ps.Start();
				ps.WaitForExit();
				this.stich_dump();
			}
			private void decompress_dump()
			{
				ProcessStartInfo psinfo = new ProcessStartInfo();
				psinfo.UseShellExecute = true;
				psinfo.WindowStyle = ProcessWindowStyle.Normal;
				psinfo.CreateNoWindow = true;
				if(this.os == "unix")
				{
					psinfo.FileName = "wine";
					psinfo.Arguments = "offzip -a " + @"""" + this.fastfile + @"""" + " " + @"""" + this.dumpdir + @"""" + " 0";
					
				}
				else if(this.os == "win32")
				{
					psinfo.FileName = "offzip";
					psinfo.Arguments = "-a " + @"""" + this.fastfile + @"""" + " " + @"""" + this.dumpdir + @"""" + " 0";
				}
				Process ps = new Process();
				ps.StartInfo = psinfo;
				ps.Start();
				ps.WaitForExit();
				this.extract_dump();	
			}
			private void stich_dump()
			{
				DirectoryInfo dumpinfo = new DirectoryInfo(this.dumpdir);
				FileInfo[] files = dumpinfo.GetFiles();
				try{
					
					foreach(FileInfo dat in files)
					{
						BinaryReader datain = new BinaryReader(File.Open(dat.FullName,FileMode.Open,FileAccess.Read,FileShare.ReadWrite));
						BinaryWriter dataout = new BinaryWriter(File.Open(this.workdir + "extract.dat",FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite));
						dataout.BaseStream.Seek(dataout.BaseStream.Length,SeekOrigin.Begin);
						long length = datain.BaseStream.Length;
						long i=0;
						while(i < length)
						{
							dataout.BaseStream.WriteByte(datain.ReadByte());
							i = dataout.BaseStream.Length;
						}
						dataout.Flush();
						dataout.Close();
						datain.Close();
						dat.Delete();
					}
				}		
				catch(IOException execp)
				{
					Console.WriteLine(execp.Message);
					return;
				}

			}
			
			
			private void extract_dump()
			{
				DirectoryInfo dumpinfo = new DirectoryInfo(this.workdir);
				FileInfo[] files = dumpinfo.GetFiles();
				try{
					
					foreach(FileInfo dat in files)
					{
						if(dat.Extension == "dat" && dat.Name != "extract.dat")
						{
							ProcessStartInfo psinfo = new ProcessStartInfo();
							psinfo.UseShellExecute = true;
							psinfo.WindowStyle = ProcessWindowStyle.Normal;
							psinfo.CreateNoWindow = true;
							if(this.os == "unix")
							{
								psinfo.FileName = "wine";
								psinfo.Arguments = "offzip -a" + @"""" + this.fastfile + @"""" + " " + @"""" + this.dumpdir + @"""" + " 0";
					
							}
							else if(this.os == "win32")
							{
								psinfo.FileName = "offzip";
								psinfo.Arguments = "-a " + @"""" + this.fastfile + @"""" + " " + @"""" + this.dumpdir + @"""" + " 0";
							}
							Process ps = new Process();
							ps.StartInfo = psinfo;
							ps.Start();
							ps.WaitForExit();
							this.extract_scripts();
						}
						
					}
				}		
				catch(IOException execp)
				{
					Console.WriteLine(execp.Message);
					return;
				}

			}
			
			private void extract_scripts()
			{
					XmlNodeList scripts = this.profile.getFileList();
					foreach(XmlNode file in scripts)
					{
						string file_name = file.Attributes["name"].Value;
						long tsize = Convert.ToInt64(file.Attributes["size"].Value);
			
						foreach(XmlNode part in file.ChildNodes)
						{
							Int64 part_start= Convert.ToInt64(part.Attributes["startpos"].Value);
							Int64 part_end= Convert.ToInt64(part.Attributes["endpos"].Value);
							string file_full_name = this.filesdir + file_name;
							BinaryReader input = new BinaryReader(File.Open(this.workdir + "extract.dat",FileMode.Open,FileAccess.Read));
							BinaryWriter output = new BinaryWriter(File.Open(file_full_name,FileMode.Append,FileAccess.Write));
							input.BaseStream.Seek(part_start,SeekOrigin.Begin);
							try
							{
								while(input.BaseStream.Position < part_end)
								{
									byte data = input.ReadByte();
									if(data != 0x00)
									output.Write(data);
								}
							}
							catch(EndOfStreamException EOFS)
							{
								Console.WriteLine(EOFS.Message);
							}
							output.Flush();
							output.Close();
							input.Close();
						}

				}

			}
		}
	}
}

