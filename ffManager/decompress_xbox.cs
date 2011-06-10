using System;
using Gtk;
using System.IO;
using System.Diagnostics;
using System.Xml;
namespace ffManager
{
	namespace Decompress.XBOX
	{
		public class Decompressor
		{
			private string fastfile;
			private string workdir;
			private string dumpdir;
			private string filesdir;
			private string os;
			private MainWindow parent;
			private ffProfile profile;
			public Decompressor (string file, ffProfile profile, MainWindow parent)
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
			public void decompress()
			{
				ffInfo fastfle_info = new ffInfo(this.fastfile);
				if(fastfle_info.getVersion() == "cod4")
				{
					this.decompress_cod4();
					return;
				}
				ProcessStartInfo psinfo = new ProcessStartInfo();
				psinfo.UseShellExecute = true;
				psinfo.WindowStyle = ProcessWindowStyle.Normal;
				psinfo.CreateNoWindow = true;
				if(this.os == "unix")
				{
					psinfo.FileName = "wine";
					psinfo.Arguments = "offzip -a " + @"""" + this.fastfile + @"""" + " " + @"""" + this.workdir + @"""" + " 0";
					
				}
				else if(this.os == "win32")
				{
					psinfo.FileName = "offzip";
					psinfo.Arguments = "-a " + @"""" + this.fastfile + @"""" + " " + @"""" + this.workdir + @"""" + " 0";
				}
				Process ps = new Process();
				ps.StartInfo = psinfo;
				ps.Start();
				ps.WaitForExit();

				this.extract_dump();	
				this.extract_scripts();
			}
			private void decompress_cod4()
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
				ffInfo fastfle_info = new ffInfo(this.fastfile);
				this.extract_scripts();
			}
			private void extract_dump()
			{
				DirectoryInfo dumpinfo = new DirectoryInfo(this.workdir);
				FileInfo[] files = dumpinfo.GetFiles();
				try{
					
					foreach(FileInfo dat in files)
					{
						if(dat.Extension == "dat")
						{
							ProcessStartInfo psinfo = new ProcessStartInfo();
							psinfo.UseShellExecute = true;
							psinfo.WindowStyle = ProcessWindowStyle.Normal;
							psinfo.CreateNoWindow = true;
							if(this.os == "unix")
							{
								psinfo.FileName = "wine";
								psinfo.Arguments = "offzip -a" + @"""" + dat.FullName + @"""" + " " + @"""" + this.dumpdir + @"""" + " 0";
					
							}
							else if(this.os == "win32")
							{
								psinfo.FileName = "offzip";
								psinfo.Arguments = "-a " + @"""" + dat.FullName + @"""" + " " + @"""" + this.dumpdir + @"""" + " 0";
							}
							Process ps = new Process();
							ps.StartInfo = psinfo;
							Console.WriteLine(psinfo.Arguments);
							ps.Start();
							ps.WaitForExit();
							break;
						}
						this.extract_scripts();
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
					Console.WriteLine("Extracting");
					foreach(XmlNode file in scripts)
					{
						string file_name = file.Attributes["name"].Value;
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
								string file_full_name = this.filesdir + file_name;
								BinaryReader input = new BinaryReader(File.Open(part_full_name,FileMode.Open,FileAccess.Read,FileShare.ReadWrite));
								BinaryWriter output = new BinaryWriter(File.Open(file_full_name,FileMode.Append,FileAccess.Write,FileShare.ReadWrite));
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

}