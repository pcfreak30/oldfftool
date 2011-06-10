using System;
using Gtk;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Collections;
namespace ffManager
{
	namespace Compress.PS3
	{
		public class Compressor
		{
			private string fastfile;
			private string workdir;
			private string dumpdir;
			private string filesdir;
			private string os;
			private MainWindow parent;
			private ffProfile profile;
			public Compressor (string file, ffProfile profile, MainWindow parent)
			{
				this.fastfile = file;
				this.os = MainWindow.getOS();
				this.parent = parent;
				FileInfo filei = new FileInfo(this.fastfile);
				if(this.os == "win32")
				{
					this.workdir = filei.DirectoryName + @"\" + filei.Name.Replace(filei.Extension,"") + "_work" + @"\";
					this.dumpdir = this.workdir + "dump" +@"\" ;
					this.filesdir = this.workdir + "files"+ @"\" ;
					this.profile = profile;
				}
				else if(this.os == "unix")
				{
					this.workdir = filei.DirectoryName + "/" + filei.Name.Replace(filei.Extension,"") + "_work" + "/";
					this.dumpdir = this.workdir + "dump" + "/";
					this.filesdir = this.workdir + "files"+ "/";
					this.profile = profile;
				}
			}
			public void compress()
			{
					XmlNodeList scripts = this.profile.getFileList();
					foreach(XmlNode file in scripts)
					{
					
						string file_name = file.Attributes["name"].Value;
						long tsize = Convert.ToInt64(file.Attributes["size"].Value);
						long pos=0;
						long len=0;
						long overflow_size = this.getOverflow(this.filesdir + file_name,tsize);
						if(overflow_size > 0)
						{
							this.stripPadding(filesdir + file_name);
							this.parent.msgbox( DialogFlags.Modal,MessageType.Error,ButtonsType.Close,"File: " + filesdir + file_name + " is over the size limit by " + overflow_size + "..\n\t\tTotal Size Available:" + tsize + "\t\t**Skipping File in Compression**");
						}
						else
						{
						this.fillPadding(this.filesdir + file_name,tsize);
						
						foreach(XmlNode part in file.ChildNodes)
						{
							string part_name = MainWindow.searchForFile(part.Attributes["name"].Value,this.fastfile);
							string part_full_name = dumpdir + part_name;
							File.Create(file_name);
							if(!File.Exists(part_full_name))
								this.parent.msgbox(DialogFlags.Modal,MessageType.Error,ButtonsType.Close,"File: "+ file_name + " could not be extracted fully as invalid files were referenced in the FFXML..");
							else
							{
								Int64 part_start= Convert.ToInt64(part.Attributes["startpos"].Value);
								Int64 part_end= Convert.ToInt64(part.Attributes["endpos"].Value);
								len= part_end -pos;
								string file_full_name = this.filesdir + file_name;
								BinaryReader input = new BinaryReader(File.Open(part_full_name,FileMode.Open,FileAccess.Read,FileShare.ReadWrite));
								BinaryWriter output = new BinaryWriter(File.Open(file_full_name,FileMode.Append,FileAccess.Write,FileShare.ReadWrite));
								input.BaseStream.Seek(part_start,SeekOrigin.Begin);
								try
								{
									while(input.BaseStream.Position < part_end)
									{
										byte data = input.ReadByte();
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
								pos += part_start;
							}
						}
						
					}
					
				}
					ffInfo ffinfo = new ffInfo(this.fastfile);
					if(ffinfo.getVersion() == "mw2")
					{
						this.compress_dump();
					}
					else
					{
						this.compress_ff();
					}
			}
			/*
			private void write_zlib_part(string infile,string outfile,long offsetout)
			{
				try
				{
					File.Delete(this.workdir + "compressed.dat");
					File.Delete(this.workdir + "compressed.zlib");
				}
				catch(IOException execp)
				{
					Console.WriteLine(execp.Message);
					return;
				}
				try
				{
					BinaryReader datain = new BinaryReader(File.Open(infile,FileMode.Open,FileAccess.Read,FileShare.ReadWrite));
					BinaryWriter dataout = new BinaryWriter(File.Open(this.workdir + "compressed.dat",FileMode.Create,FileAccess.Write,FileShare.ReadWrite));
					while(dataout.BaseStream.Length < datain.BaseStream.Length)
					{
						dataout.BaseStream.WriteByte(datain.ReadByte());
					}
					dataout.Flush();
					dataout.Close();
					datain.Close();
				}
				catch(IOException execp)
				{
					Console.WriteLine(execp.Message);
					return;
				}
				
				ProcessStartInfo psinfo = new ProcessStartInfo();
				psinfo.UseShellExecute = true;
				psinfo.WindowStyle = ProcessWindowStyle.Normal;
				psinfo.CreateNoWindow = true;
				ffInfo fastfle_info = new ffInfo(this.fastfile);
				string arg = "";
				if(fastfle_info.getVersion() != "mw2")
				{
					arg = "";
				}
				else arg = " -w -15 ";
				if(this.os == "unix")
				{
					psinfo.FileName = "wine";
					psinfo.Arguments = "offzip -a " + arg + @"""" + this.workdir + "compressed.dat" + @"""" + " " + @"""" + this.workdir + "compressed.zlib" + @"""" + " 0";
					
				}
				else if(this.os == "win32")
				{
					psinfo.FileName = "offzip";
					psinfo.Arguments = "-a "+ arg +  @"""" + this.workdir + "compressed.dat" + @"""" + " " + @"""" + this.workdir + "compressed.zlib" +@"""" + " 0";
				}
				Process ps = new Process();
				ps.StartInfo = psinfo;
				ps.Start();
				ps.WaitForExit();
				try
				{
					BinaryReader datain = new BinaryReader(File.Open(this.workdir + "compressed.zlib",FileMode.Open,FileAccess.Read,FileShare.ReadWrite));
					BinaryWriter dataout = new BinaryWriter(File.Open(outfile,FileMode.Create,FileAccess.Write,FileShare.ReadWrite));
					dataout.BaseStream.Seek(offsetoWut, SeekOrigin.Begin);
					long start = 0;
					while(curr < datain.BaseStream.Length)
					{
						dataout.BaseStream.WriteByte(datain.ReadByte());
						curr++;
					}
					dataout.Flush();
					dataout.Close();
					datain.Close();
				}
				catch(IOException execp)
				{
					Console.WriteLine(execp.Message);
					return;
				}
			}
			*/
			
			
			private void compress_dump()
			{
					XmlNodeList scripts = this.profile.getFileList();
					ArrayList processed_parts = new ArrayList();
						DirectoryInfo dumpinfo = new DirectoryInfo(this.workdir);
						FileInfo[] files = dumpinfo.GetFiles();
						string dump_file = "";
						foreach(FileInfo dat in files)
						{
							if(dat.Extension == "dat")
							{
								dump_file = dat.FullName;
							}
						}
					foreach(XmlNode file in scripts)
					{
						foreach(XmlNode part in file)
						{
							string part_name = MainWindow.searchForFile(part.Attributes["name"].Value, this.fastfile);
							string part_full_name = this.dumpdir + part_name;
							if(!processed_parts.Contains(part_name))
							   {
								ProcessStartInfo psinfo = new ProcessStartInfo();
								psinfo.UseShellExecute = true;
								psinfo.WindowStyle = ProcessWindowStyle.Normal;
								psinfo.CreateNoWindow = true;
								if(this.os == "unix")
								{
									psinfo.FileName = "wine";
									psinfo.Arguments = "packzip -o 0x" + @"""" + part.Attributes["name"].Value + @"""" + " " + @"""" + part_full_name + @"""" + " " + @"""" + dump_file + @"""";
						
								}
								else if(this.os == "win32")
								{
									psinfo.FileName = "packzip";
									psinfo.Arguments = "-o 0x" + @"""" + part.Attributes["name"].Value + @"""" + " " + @"""" + part_full_name + @"""" + " " + @"""" + dump_file + @"""" ;
								}
								Process ps = new Process();
								ps.StartInfo = psinfo;
								ps.Start();
								ps.WaitForExit();
								processed_parts.Add(part_name);
							}
						}
					}
					this.compress_dump_round2();
			}
			
			private void compress_dump_round2()
			{
					XmlNodeList scripts = this.profile.getFileList();
						DirectoryInfo dumpinfo = new DirectoryInfo(this.workdir);
						FileInfo[] files = dumpinfo.GetFiles();
						string dump_file = "";
						foreach(FileInfo dat in files)
						{
							if(dat.Extension == "dat")
							{
								dump_file = dat.FullName;
							}
						}
						ProcessStartInfo psinfo = new ProcessStartInfo();
						psinfo.UseShellExecute = true;
						psinfo.WindowStyle = ProcessWindowStyle.Normal;
						psinfo.CreateNoWindow = true;
						if(this.os == "unix")
						{
							psinfo.FileName = "wine";
							psinfo.Arguments = "packzip -o 0x" + @"""" + dump_file.Replace(".dat","")+ @"""" + " " + @"""" + this.workdir + dump_file + @"""" + " " + @"""" + this.fastfile + @"""";
													}
						else if(this.os == "win32")
						{
							psinfo.FileName = "packzip";
							psinfo.Arguments = "-o 0x" + @"""" + dump_file.Replace(".dat","") + @"""" + " " + @"""" + this.workdir + dump_file + @"""" + " " + @"""" + this.fastfile + @"""" ;
						}
						Process ps = new Process();
						ps.StartInfo = psinfo;
						ps.Start();
						ps.WaitForExit();
			}
			private void compress_ff()
			{
					XmlNodeList scripts = this.profile.getFileList();
					ArrayList processed_parts = new ArrayList();
						DirectoryInfo dumpinfo = new DirectoryInfo(this.workdir);
						FileInfo[] files = dumpinfo.GetFiles();
						string dump_file = "";
						foreach(FileInfo dat in files)
						{
							if(dat.Extension == "dat")
							{
								dump_file = dat.FullName;
							}
						}
					foreach(XmlNode file in scripts)
					{
						foreach(XmlNode part in file)
						{
							string part_name = MainWindow.searchForFile(part.Attributes["name"].Value, this.fastfile);
							string part_full_name = this.dumpdir + part_name;
							if(!processed_parts.Contains(part_name))
							   {
								ProcessStartInfo psinfo = new ProcessStartInfo();
								psinfo.UseShellExecute = true;
								psinfo.WindowStyle = ProcessWindowStyle.Normal;
								psinfo.CreateNoWindow = true;
								if(this.os == "unix")
								{
									psinfo.FileName = "wine";
									psinfo.Arguments = "packzip -o 0x" + @"""" + part.Attributes["name"].Value + @"""" + " " + @"""" + part_full_name + @"""" + " " + @"""" + this.fastfile + @"""";
						
								}
								else if(this.os == "win32")
								{
									psinfo.FileName = "packzip";
									psinfo.Arguments = "-o 0x" + @"""" + part.Attributes["name"].Value + @"""" + " " + @"""" + part_full_name + @"""" + " " + @"""" + this.fastfile + @"""" ;
								}
								Process ps = new Process();
								ps.StartInfo = psinfo;
								ps.Start();
								ps.WaitForExit();
								processed_parts.Add(part_name);
							}
						}
					}
			}
				
		private long fillPadding(string file, long size)
		{
			long count = 0;
	
			BinaryWriter output = new BinaryWriter(File.Open(file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite) );
			long num = size - output.BaseStream.Length;
			try
			{
					for(int i=0; i < num; i++)
					{
						output.BaseStream.WriteByte(0);
					}
					count++;
			}
			catch(IOException EXECP)
			{
			}
			output.Close();
			return count;

			}
			private void stripPadding(string file)
			{
		BinaryReader filer = new BinaryReader(File.Open(file,FileMode.Open,FileAccess.Read,FileShare.ReadWrite ));
		BinaryWriter filew = new BinaryWriter(File.Open(file,FileMode.Open,FileAccess.Write,FileShare.ReadWrite));
		long count = filer.BaseStream.Length;
		
		try
		{
			while(filer.BaseStream.Length <= count)
			{
				byte data = filer.ReadByte();
				if(data != 0x00)
				{
					Console.Write(Convert.ToChar(data));
					filew.Write(data);
				}
			}
		}
		catch(EndOfStreamException execp)
		{
			Console.WriteLine(execp.Message);
		}
		filew.Flush();
		filew.Close();
		filer.Close();
	}
	private long getOverflow(string file, long size)
	{
		
			BinaryReader file_handle= new BinaryReader(File.Open(file,FileMode.Open,FileAccess.Read,FileShare.ReadWrite));
			long csize = file_handle.BaseStream.Length;
			file_handle.Close();
			return csize - size;
	}
		}
	}
}