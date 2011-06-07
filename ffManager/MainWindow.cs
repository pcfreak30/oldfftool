using System;
using Gtk;
using System.IO;
using System.Diagnostics;
using ffManager;
using System.Xml;
public partial class MainWindow : Gtk.Window
{
	private string platform;
	private string opened_ff_file;
	private string fslash;
	private string console;
	private ffProfile ffprofile = new ffProfile();
	public MainWindow () : base(Gtk.WindowType.Toplevel)
	{
		Build ();		
		this.platform = MainWindow.getOS();
		if(this.platform == "unix")
		{
				if(!File.Exists("/usr/bin/wine") && !File.Exists("/usr/local/bin/wine"))
				{
					this.msgbox(this,DialogFlags.DestroyWithParent,MessageType.Error,ButtonsType.Close,"ERROR: Wine was not found on your system!\n\t\tPlease install wine and re-run ffManager..");
					this.menubar1.Sensitive = false;
					this.notebook1.Sensitive= false;
				}
				if(!File.Exists(Directory.GetCurrentDirectory() + "/offzip.exe"))
				{
					this.msgbox(this,DialogFlags.DestroyWithParent,MessageType.Error,ButtonsType.Close,"ERROR: offzip.exe was NOT found in the ffManager folder!\n\t\tPlease re-download and re-run ffManager..");
					this.menubar1.Sensitive = false;
					this.notebook1.Sensitive= false;
				}
				if(!File.Exists( Directory.GetCurrentDirectory() +"/packzip.exe"))
				{
					this.msgbox(this,DialogFlags.DestroyWithParent,MessageType.Error,ButtonsType.Close,"ERROR: packzip.exe was NOT found in the ffManager folder!\n\t\tPlease re-download and re-run ffManager..");
					this.menubar1.Sensitive = false;
					this.notebook1.Sensitive= false;
				}
		}
		else if(this.platform == "win32")
		{
				if(!File.Exists( Directory.GetCurrentDirectory() + @"\offzip.exe"))
				{
					this.msgbox(this,DialogFlags.DestroyWithParent,MessageType.Error,ButtonsType.Close,"ERROR: offzip.exe was NOT found in the ffManager folder!\n\t\tPlease re-download and re-run ffManager..");
					this.menubar1.Sensitive = false;
					this.notebook1.Sensitive= false;
				}
				if(!File.Exists( Directory.GetCurrentDirectory() + @"\packzip.exe"))
				{
					this.msgbox(this,DialogFlags.DestroyWithParent,MessageType.Error,ButtonsType.Close,"ERROR: packzip.exe was NOT found in the ffManager folder!\n\t\tPlease re-download and re-run ffManager..");
					this.menubar1.Sensitive = false;
					this.notebook1.Sensitive= false;
				}
		}

		FileFilter filter = new FileFilter();
		
		filter.Name = "FastFile XML Profile";
		filter.AddMimeType("application/xml");
		filter.AddPattern("*.ffxml");
		this.ffprofile_chooser.AddFilter(filter);
		this.ffprofile_chooser.FileSet+= ff_profile_open_callback;
	}
	public static string getOS()
	{
		OperatingSystem os = Environment.OSVersion;
		PlatformID pid = os.Platform;
		string result = "";
		switch (pid) 
        {
        case PlatformID.Win32NT:
        case PlatformID.Win32S:
        case PlatformID.Win32Windows:
        case PlatformID.WinCE:
            result= "win32";
            break;
        case PlatformID.Unix:
           result= "unix";
			break;
        }
		return result;
	}
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
	protected virtual void ff_open_handler (object sender, System.EventArgs e)
	{
		FileChooserDialog fdiag = new FileChooserDialog("Open FF File",this,FileChooserAction.Open,"Cancel",ResponseType.Cancel, "Open",ResponseType.Accept);
		FileFilter filter = new FileFilter();
		
		filter.Name = "FastFiles";
		filter.AddMimeType("binary/octet-stream");
		filter.AddPattern("*.ff");
		fdiag.AddFilter(filter);
		int response = (int)fdiag.Run();
		if(response == (int) ResponseType.Accept)
		{
			this.ff_open_callback(fdiag.Filename);
		}
		fdiag.Destroy();
		fdiag.Dispose();
	}
	private void ff_open_callback(string fname)
	{
		FileInfo ffinfo = new FileInfo(fname);
		ffInfo fastfile_data = new ffInfo(fname);
		string fastfile_header = fastfile_data.getHeader();
		string fastfile_version= fastfile_data.getVersion();		if(fastfile_header != "IWff0100" && fastfile_header != "IWffu100" )
		{
			this.msgbox(this, DialogFlags.DestroyWithParent,MessageType.Error,ButtonsType.Ok,"Oops! You either gave a corrupt fast file or a file that is NOT a fast file...\n Please open a VALID fastfile!");
			return;
		}
		else if(fastfile_header == "IWffs100" )
		{
			this.msgbox(this, DialogFlags.DestroyWithParent,MessageType.Error,ButtonsType.Ok,"Oops! You gave a SIGNED fastfile. ffManager does not support these\n Please open a VALID UN-SIGNED fastfile!");
			return;
		}
		if(fastfile_version == "invalid")
		{
			this.msgbox(this, DialogFlags.DestroyWithParent,MessageType.Error,ButtonsType.Ok,"Oops! You gave a FastFile for a game ffManager does not support...\n Please open a VALID SUPPORTED fastfile!");
			return;
		}
		else
		{
			this.console = fastfile_version;
		}
		string ffdir = ffinfo.Directory.FullName;
		string workdir = ffdir + this.fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + this.fslash;
		string dumpdir= workdir +  "dump" + this.fslash;
		string filesdir= workdir + "files"+ this.fslash;
		
		if(Directory.Exists(workdir))
		{
			MessageDialog msg = new MessageDialog(this,
			                                      DialogFlags.Modal,
			                                      MessageType.Warning,
			                                      ButtonsType.YesNo,
			                                      "Patch extraction directory exists!\nDelete and continue?"
			                                      );
			ResponseType resp= (ResponseType) msg.Run();
			if(resp == ResponseType.Yes)
			{
				try
				{
					Directory.Delete(workdir,true);
				}
				catch(Exception Excep)
				{
					return;
				}
			}
			else if(resp == ResponseType.No)
			{
				this.ff_open_handler(new object(), new EventArgs());
			}
			msg.Destroy();
			msg.Dispose();
		}
		try
		{
			Directory.CreateDirectory(workdir);
			Directory.CreateDirectory(dumpdir);
			Directory.CreateDirectory(filesdir);
		}
		catch(Exception Excep)
		{
			Console.WriteLine(Excep.Message.ToString());
			return;
		}
		this.btn_comp.Sensitive = false;
		this.btn_decomp.Sensitive = true;
		this.ff_profile_box.Sensitive=true;
		this.opened_ff_file= fname;
		if(this.ffprofile_chooser.Filename != "" || this.ffprofile_chooser.Filename != null)
			this.ffprofile.setProfile(this.ffprofile_chooser.Filename,this.opened_ff_file);
		else
			this.ffprofile.setProfile("",this.opened_ff_file);
		
	}
	protected virtual void btn_decomp_pressed (object sender, System.EventArgs e)
	{
		if(this.ffprofile_chooser.Filename == "" || this.ffprofile_chooser.Filename == null || !this.ffprofile.isValid())
		{
			this.msgbox(this,
			            DialogFlags.Modal,
			            MessageType.Error,
			            ButtonsType.Close,
			            "Please select a Valid FastFile Profile"
			            );
			return;	
		}
		this.btn_decomp.Sensitive = false;

		if(this.console == "ps3")
		{
			ffManager.Decompress.PS3.Decompressor  ps3_decomp= new ffManager.Decompress.PS3.Decompressor(this.opened_ff_file,this.ffprofile);
			ps3_decomp.decompress();
		}
	}
	
	protected virtual void ff_profile_open_callback (object sender, System.EventArgs e)
	{
		if(this.ffprofile_chooser.Filename == "" || this.ffprofile_chooser.Filename == null)
			return;
		this.ffprofile.setProfile( this.ffprofile_chooser.Filename, this.opened_ff_file);
		if(this.ffprofile.isValid() == false)
		{
			this.msgbox(this,
			            DialogFlags.Modal,
			            MessageType.Error,
			            ButtonsType.Close,
			            "Invalid FastFile Profile"
			            );
			return;	
		}

	}
		private void processFiles_packed()
		{
			string pack_extract = this.searchForFile(this.ffprofile.getPackedDump());
			Process p = new Process();
			FileInfo ffinfo = new FileInfo(this.opened_ff_file);
			string ffdir = ffinfo.Directory.FullName;
			string dumpdir = ffdir + this.fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + this.fslash + "dump" + this.fslash;
			if(!File.Exists(dumpdir + pack_extract))
			{
				this.msgbox(this, DialogFlags.Modal,MessageType.Error,ButtonsType.Close,"File: " + dumpdir + pack_extract + " does not exist!");
				this.btn_decomp.Sensitive = true;
				return;
			}
		if(this.platform == "win32")
		{
			ProcessStartInfo info = p.StartInfo;
			info.FileName="offzip";
			info.Arguments="-a " + @"""" +  dumpdir  + pack_extract + @"""" + " " + @"""" + dumpdir + @"""" + " 0";
			info.WindowStyle = ProcessWindowStyle.Hidden;
		}
		else if(this.platform == "unix")
		{
			ProcessStartInfo info = p.StartInfo;
			info.FileName="wine";
			info.Arguments="offzip -a " + @"""" + dumpdir +  pack_extract + @"""" + " " + @"""" + dumpdir + @"""" + " 0";
			info.WindowStyle = ProcessWindowStyle.Hidden;
		}
		p.Start();
		p.WaitForExit();
		this.processFiles_parts();
		}
		private void processFiles_parts()
		{
			FileInfo ffinfo = new FileInfo(this.opened_ff_file);
			string ffdir = ffinfo.Directory.FullName;
			string filesdir = ffdir + fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + this.fslash + "files" + fslash;
			string dumpdir = ffdir + fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + this.fslash + "dump" + this.fslash;
			
			XmlNodeList data = this.ffprofile.getFileList();
			foreach(XmlNode file in data)
			{
				string file_name = file.Attributes["name"].Value;
				long tsize = Convert.ToInt64(file.Attributes["size"].Value);
			
				foreach(XmlNode part in file.ChildNodes)
				{
					string part_name = this.searchForFile(part.Attributes["name"].Value);
					string part_start= part.Attributes["startpos"].Value;
					string part_end= part.Attributes["endpos"].Value;
					string file_full_name = filesdir + file_name;
					string part_full_name = dumpdir + part_name;
					if(File.Exists(part_full_name))
						this.write_part(part_full_name, file_full_name,part_start, part_end);
					else this.msgbox(this, DialogFlags.Modal,MessageType.Error,ButtonsType.Close,"File: " + part_full_name + " does not exist!");
				
				}
			}
			this.msgbox(this, DialogFlags.Modal,MessageType.Info,ButtonsType.Ok,"FastFile " + this.opened_ff_file + " Decompressed to:\n" + filesdir);
			this.btn_comp.Sensitive = true;
		}
	private void compressfile_part2(string file)
		{
			Process p = new Process();
			FileInfo ffinfo = new FileInfo(this.opened_ff_file);
			string ffdir = ffinfo.Directory.FullName;
		string comp = "";
		if(this.fslash == "ps3")
		{
			switch(this.ffprofile.getGame())
			{
				case "cod4":
				case "waw":
				comp = "-w -15";
				break;
				case "mw2":
					comp = "";
				break;
			}
		}
		else if(this.ffprofile.getConsole() == "xbox")
		{
			
			switch(this.ffprofile.getGame())
			{
				
				case "waw":
					comp = "-w -15";
				break;
				case "cod4":
				case "mw2":
					comp = "";
				break;
			}
		}
			string dumpdir = ffdir + this.fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + this.fslash + "dump/";
			string hex_offset = dumpdir + this.searchForFile(file);
			string packdump = this.searchForFile(this.ffprofile.getPackedDump());
			if(!File.Exists(dumpdir + file))
			{
				this.msgbox(this, DialogFlags.Modal,MessageType.Error,ButtonsType.Close,"File: " + packdump + file + " does not exist!");
				this.btn_decomp.Sensitive = true;
				return;
			}
		if(this.platform == "win32")
		{
			ProcessStartInfo info = p.StartInfo;
			info.FileName="packzip";
			info.Arguments="-o 0x" + hex_offset + " " + comp +  @"""" +  dumpdir + file + @"""" + " " + @"""" +  packdump + file + @"""";
			info.WindowStyle = ProcessWindowStyle.Normal;
		}
		else if(this.platform == "unix")
		{
			ProcessStartInfo info = p.StartInfo;
			info.FileName="wine";
			info.Arguments="packzip -o 0x" + hex_offset + " " + comp +  @"""" + dumpdir + file + @"""" + " " + @"""" + dumpdir + file + @"""";
			info.WindowStyle = ProcessWindowStyle.Normal;
		}
		p.Start();
		p.WaitForExit();
		this.processFiles_parts();
		}
		
		private void compress_ff()
		{
			Process p = new Process();
			FileInfo ffinfo = new FileInfo(this.opened_ff_file);
			string ffdir = ffinfo.Directory.FullName;
		string comp = "";
		if(this.ffprofile.getConsole() == "ps3")
		{
			switch(this.ffprofile.getGame())
			{
				case "cod4":
				case "waw":
					return;
				break;
			}
		}
		else if(this.ffprofile.getConsole() == "xbox")
		{
			
			switch(this.ffprofile.getGame())
			{
				
				case "waw":
					return;
				break;
			}
		}
			string dumpdir = ffdir + this.fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + this.fslash + "dump/";
			string hex_offset = dumpdir + this.ffprofile.getPackedDump();
			string packdump = this.searchForFile(this.ffprofile.getPackedDump());
			if(!File.Exists(dumpdir + packdump))
			{
				this.msgbox(this, DialogFlags.Modal,MessageType.Error,ButtonsType.Close,"File: " + dumpdir + packdump + " does not exist!");
				this.btn_decomp.Sensitive = true;
				return;
			}
		if(this.platform == "win32")
		{
			ProcessStartInfo info = p.StartInfo;
			info.FileName="packzip";
			info.Arguments="-o 0x" + hex_offset + " " + comp +  @"""" +  dumpdir + packdump + @"""" + " " + @"""" +  this.opened_ff_file + @"""";
			info.WindowStyle = ProcessWindowStyle.Normal;
		}
		else if(this.platform == "unix")
		{
			ProcessStartInfo info = p.StartInfo;
			info.FileName="wine";
			info.Arguments="packzip -o 0x" + hex_offset + " " + comp +  @"""" + dumpdir + packdump + @"""" + " " + @"""" + this.opened_ff_file + @"""";
			info.WindowStyle = ProcessWindowStyle.Normal;
		}
		p.Start();
		p.WaitForExit();
		}
		private void compressfiles_parts()
		{
			FileInfo ffinfo = new FileInfo(this.opened_ff_file);
			string ffdir = ffinfo.Directory.FullName;
			string filesdir = ffdir + this.fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + this.fslash + "files" + this.fslash;
			string dumpdir = ffdir + this.fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + this.fslash + "dump" + this.fslash;
			
			XmlNodeList data = this.ffprofile.getFileList();
			foreach(XmlNode file in data)
			{
				string file_name = file.Attributes["name"].Value;
				long tsize = Convert.ToInt64(file.Attributes["size"].Value);
			
				foreach(XmlNode part in file.ChildNodes)
				{
					string part_name = part.Attributes["name"].Value;
					string part_start= part.Attributes["startpos"].Value;
					string part_end= part.Attributes["endpos"].Value;
					string file_full_name = filesdir + file_name;
					string part_full_name = dumpdir + part_name;
				Console.WriteLine(part_full_name);
					if(File.Exists(part_full_name))
						this.write_part(part_full_name, file_full_name,part_start, part_end);
						else this.msgbox(this, DialogFlags.Modal,MessageType.Error,ButtonsType.Close,"File: " + part_full_name + " does not exist!");
				
				}
			}
			this.msgbox(this, DialogFlags.Modal,MessageType.Info,ButtonsType.Ok,"FastFile " + this.opened_ff_file + " Decompressed to:\n" + filesdir);
			this.btn_comp.Sensitive = true;
		}


	protected virtual void btn_comp_pressed (object sender, System.EventArgs e)
	{
		FileInfo ffinfo = new FileInfo(this.opened_ff_file);
			string ffdir = ffinfo.Directory.FullName;
			try
			{
				File.Copy(this.opened_ff_file,this.opened_ff_file + ".bak",true);
			}
			catch(IOException execp)
			{}
			string filesdir = ffdir + this.fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + this.fslash + "files" + this.fslash;
			string dumpdir = ffdir + this.fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + this.fslash + "dump" + this.fslash;
			Console.WriteLine(dumpdir);
			XmlNodeList data = this.ffprofile.getFileList();
			foreach(XmlNode file in data)
			{
				string file_name = file.Attributes["name"].Value;
				long tsize = Convert.ToInt64(file.Attributes["size"].Value);
				long pos=0;
				long len=0;
				long overflow_size = this.getOverflow(filesdir + file_name,tsize);
				if(overflow_size > 0)
				{
					this.stripPadding(filesdir + file_name);
					this.msgbox(this, DialogFlags.Modal,MessageType.Error,ButtonsType.Close,"File: " + filesdir + file_name + " is over the size limit by " + overflow_size + "..\n\t\tTotal Size Available:" + tsize);
					return;
				}
				else
				{
					this.fillPadding(filesdir + file_name,tsize);
				}
					foreach(XmlNode part in file.ChildNodes)
					{
				
						string part_name = this.searchForFile(part.Attributes["name"].Value);
				Console.WriteLine(part_name);
						string part_start= part.Attributes["startpos"].Value;
						string part_end= part.Attributes["endpos"].Value;
						string file_full_name = filesdir + file_name;
						string part_full_name = dumpdir + part_name;
						len = Convert.ToInt64(part_end) - pos;
						if(File.Exists(part_full_name))
							this.write_part_back(file_full_name, part_full_name,pos,len, part_start);
						else this.msgbox(this, DialogFlags.Modal,MessageType.Error,ButtonsType.Close,"File: " + part_full_name + " does not exist!");
						pos += Convert.ToInt64(part_start);
						this.compressfile_part2(part.Attributes["name"].Value);
			}
			
		}
		this.compress_ff();
		this.msgbox(this, DialogFlags.Modal,MessageType.Info,ButtonsType.Ok,"FastFile " + this.opened_ff_file + " Re-Compressed to:\n" + filesdir);
		this.btn_comp.Sensitive = true;
	}
		private void write_part(string infile, string outfile, string start, string end)
		{
			BinaryReader input = new BinaryReader(File.Open(infile,FileMode.Open,FileAccess.Read));
			BinaryWriter output = new BinaryWriter(File.Open(outfile,FileMode.Append,FileAccess.Write));
			long posa =  Convert.ToInt64(start);
			long posb =  Convert.ToInt64(end);
			input.BaseStream.Seek(posa,SeekOrigin.Begin);
		try{
			while(input.BaseStream.Position < posb)
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
	private void write_part_back(string infile, string outfile, long src_start, long src_end,string dst_start)
	{
			BinaryReader input = new BinaryReader(File.Open(infile,FileMode.Open,FileAccess.Read,FileShare.ReadWrite));
			BinaryWriter output = new BinaryWriter(File.Open(outfile,FileMode.Open,FileAccess.Write,FileShare.ReadWrite));
			long ldst_start =  Convert.ToInt64(dst_start);
			output.BaseStream.Seek(ldst_start,SeekOrigin.Begin);
			input.BaseStream.Seek(src_start, SeekOrigin.Begin);
		try{
			while(input.BaseStream.Position <  src_end)
			{
				byte data = input.ReadByte();
				output.Write(data);
			}
		}
		catch(EndOfStreamException EOFS)
		{
			
		}
			output.Flush();
			output.Close();
			input.Close();
		}
			private void msgbox(Window parent_window, DialogFlags flags, MessageType type, ButtonsType btype, string msg)
		{
			MessageDialog msgb = new MessageDialog(parent_window,
		                                      flags,
		                                      type,
		                                      btype,
		                                      msg
		                                      );
			msgb.Run();
			msgb.Destroy();
			msgb.Dispose();
		
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
	private string searchForFile(string filename)
	{
		FileInfo ffinfo = new FileInfo(this.opened_ff_file);
		string ffdir = ffinfo.Directory.FullName;
		string dumpdir = ffdir + this.fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + this.fslash + "dump" + this.fslash;
		string[] list = {"dat","inn","nge","mbs","ase","vvv","neo","img","ttf","dc5","nta","fnc"};
		foreach(string ext in list)
		{
						
			if(File.Exists(dumpdir + filename + "." + ext))
				return filename + "."+ ext;
			
		}
		return "";
	}
}

		
	

