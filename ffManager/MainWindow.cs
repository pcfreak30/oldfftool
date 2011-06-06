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
	private ffProfile ffprofile = new ffProfile();
	public MainWindow () : base(Gtk.WindowType.Toplevel)
	{
		Build ();
		OperatingSystem os = Environment.OSVersion;
		PlatformID pid = os.Platform;
		switch (pid) 
        {
        case PlatformID.Win32NT:
        case PlatformID.Win32S:
        case PlatformID.Win32Windows:
        case PlatformID.WinCE:
             this.platform = "win32";
            break;
        case PlatformID.Unix:
            this.platform = "unix";
            break;
        }
		FileFilter filter = new FileFilter();
		
		filter.Name = "FastFile XML Profile";
		filter.AddMimeType("application/xml");
		filter.AddPattern("*.ffxml");
		this.ffprofile_chooser.AddFilter(filter);
		this.ffprofile_chooser.FileSet+= ff_profile_open_callback;
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
		string fslash = "";
		if(this.platform == "win32")
			fslash = @"\";
		else if(this.platform == "unix")
			fslash = "/";
		
		string ffdir = ffinfo.Directory.FullName;
		string workdir = ffdir + fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work";
		string dumpdir= ffdir + fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + fslash + "dump";
		string filesdir= ffdir + fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + fslash + "files";
		
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
			if(resp == ResponseType.No)
			{
				return;
			}
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
			this.ffprofile.setProfile(this.ffprofile_chooser.Filename);
		else
			this.ffprofile.setProfile("");
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
		Process p = new Process();
		FileInfo ffinfo = new FileInfo(this.opened_ff_file);
		string ffdir = ffinfo.Directory.FullName;
		string fslash = "";
		if(this.platform == "win32")
			fslash = @"\";
		else if(this.platform == "unix")
			fslash = "/";
		string dumpdir = ffdir + fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + fslash + "dump";
		string console = this.ffprofile.getConsole();

		string game = this.ffprofile.getGame();
		string comp = "";
		string compmode = "";
		if(console == "ps3")
		{
			switch(game)
			{
				case "cod4":
				case "waw":
					comp = "-z -15";
				compmode = "part";
				break;
				case "mw2":
					comp = "";
					compmode = "pack";
				break;
			}
		}
		else if(console == "xbox")
		{
			
			switch(game)
			{
				
				case "waw":
					comp = "-z -15";
					compmode = "part";
				break;
				case "cod4":
					comp = "";
					compmode = "part";
				break;
				case "mw2":
					comp = "";
					compmode = "pack";
				break;
			}
		}
		if(this.platform == "win32")
		{
			ProcessStartInfo info = p.StartInfo;
			info.FileName="offzip";
			info.Arguments="-a " + comp + " " + @"""" + this.opened_ff_file + @"""" + " " + @"""" + dumpdir + @"""" + " 0";
			info.WindowStyle = ProcessWindowStyle.Normal;
		}
		else if(this.platform == "unix")
		{
			ProcessStartInfo info = p.StartInfo;
			info.FileName="wine";
			info.Arguments="offzip -a " + comp + " " + @"""" + this.opened_ff_file + @"""" + " " + @"""" + dumpdir + @"""" + " 0";
			info.WindowStyle = ProcessWindowStyle.Normal;
		}
		p.Start();
		p.WaitForExit();
		if(compmode  == "pack")
			this.processFiles_packed();
		else if(compmode == "part")
			this.processFiles_parts();
	}
	
	protected virtual void ff_profile_open_callback (object sender, System.EventArgs e)
	{
		if(this.ffprofile_chooser.Filename == "" || this.ffprofile_chooser.Filename == null)
			return;
		this.ffprofile.setProfile(this.ffprofile_chooser.Filename);
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
			string pack_extract = this.ffprofile.getPackedDump();
			Process p = new Process();
			FileInfo ffinfo = new FileInfo(this.opened_ff_file);
			string ffdir = ffinfo.Directory.FullName;
			string fslash = "";
			if(this.platform == "win32")
				fslash = @"\";
			else if(this.platform == "unix")
				fslash = "/";
			string dumpdir = ffdir + fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + fslash + "dump/";
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
			info.WindowStyle = ProcessWindowStyle.Normal;
		}
		else if(this.platform == "unix")
		{
			ProcessStartInfo info = p.StartInfo;
			info.FileName="wine";
			info.Arguments="offzip -a " + @"""" + dumpdir +  pack_extract + @"""" + " " + @"""" + dumpdir + @"""" + " 0";
			info.WindowStyle = ProcessWindowStyle.Normal;
		}
		p.Start();
		p.WaitForExit();
		this.processFiles_parts();
		}
		private void processFiles_parts()
		{
			FileInfo ffinfo = new FileInfo(this.opened_ff_file);
			string ffdir = ffinfo.Directory.FullName;
			string fslash = "";
			if(this.platform == "win32")
				fslash = @"\";
			else if(this.platform == "unix")
				fslash = "/";
			string filesdir = ffdir + fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + fslash + "files" + fslash;
			string dumpdir = ffdir + fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + fslash + "dump" + fslash;
			
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
					if(File.Exists(part_full_name))
						this.write_part(part_full_name, file_full_name,part_start, part_end);
					else this.msgbox(this, DialogFlags.Modal,MessageType.Error,ButtonsType.Close,"File: " + part_full_name + " does not exist!");
				
				}
			}
			this.msgbox(this, DialogFlags.Modal,MessageType.Info,ButtonsType.Ok,"FastFile " + this.opened_ff_file + " Decompressed to:\n" + filesdir);
			this.btn_comp.Sensitive = true;
		}
	private void compressfile_packed(string file)
		{
			Process p = new Process();
			FileInfo ffinfo = new FileInfo(this.opened_ff_file);
			string ffdir = ffinfo.Directory.FullName;
			string fslash = "";
			if(this.platform == "win32")
				fslash = @"\";
			else if(this.platform == "unix")
				fslash = "/";
		string console = this.ffprofile.getConsole();

		string game = this.ffprofile.getGame();
		string comp = "";
		if(console == "ps3")
		{
			switch(game)
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
		else if(console == "xbox")
		{
			
			switch(game)
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
			string dumpdir = ffdir + fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + fslash + "dump/";
			string[] file_parts= file.Split('.');
			
			string hex_offset = dumpdir;
			for(int i=0; i < file_parts.Length -1;i++) hex_offset += file_parts[i];
			if(!File.Exists(dumpdir + file))
			{
				this.msgbox(this, DialogFlags.Modal,MessageType.Error,ButtonsType.Close,"File: " + dumpdir + file + " does not exist!");
				this.btn_decomp.Sensitive = true;
				return;
			}
		if(this.platform == "win32")
		{
			ProcessStartInfo info = p.StartInfo;
			info.FileName="packzip";
			info.Arguments="-o 0x" + hex_offset + " " + comp +  @"""" +  dumpdir + file + @"""" + " " + @"""" + this.opened_ff_file + @"""";
			info.WindowStyle = ProcessWindowStyle.Normal;
		}
		else if(this.platform == "unix")
		{
			ProcessStartInfo info = p.StartInfo;
			info.FileName="wine";
			info.Arguments="packzip -o 0x" + hex_offset + " " + comp +  @"""" + dumpdir + file + @"""" + " " + @"""" + this.opened_ff_file + @"""";
			info.WindowStyle = ProcessWindowStyle.Normal;
		}
		p.Start();
		p.WaitForExit();
		this.processFiles_parts();
		}
		private void compressfiles_parts()
		{
			FileInfo ffinfo = new FileInfo(this.opened_ff_file);
			string ffdir = ffinfo.Directory.FullName;
			string fslash = "";
			if(this.platform == "win32")
				fslash = @"\";
			else if(this.platform == "unix")
				fslash = "/";
			string filesdir = ffdir + fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + fslash + "files" + fslash;
			string dumpdir = ffdir + fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + fslash + "dump" + fslash;
			
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
			string fslash = "";
			if(this.platform == "win32")
				fslash = @"\";
			else if(this.platform == "unix")
				fslash = "/";
			try
			{
				File.Copy(this.opened_ff_file,this.opened_ff_file + ".bak",true);
			}
			catch(IOException execp)
			{}
			string filesdir = ffdir + fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + fslash + "files" + fslash;
			string dumpdir = ffdir + fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + fslash + "dump" + fslash;
			XmlNodeList data = this.ffprofile.getFileList();
			foreach(XmlNode file in data)
			{
				string file_name = file.Attributes["name"].Value;
				long tsize = Convert.ToInt64(file.Attributes["size"].Value);
				long pos=0;
				long len=0;
				this.fillPadding(filesdir + file_name,tsize);
				long overflow_size = this.getOverflow(filesdir + file_name,tsize);
				if(overflow_size > 0)
				{
					this.stripPadding(filesdir + file_name);
					this.msgbox(this, DialogFlags.Modal,MessageType.Error,ButtonsType.Close,"File: " + filesdir + file_name + " is over the size limit by " + overflow_size + "..\n\t\tTotal Size Available:" + tsize);
					return;
				}
				else
				{
					foreach(XmlNode part in file.ChildNodes)
					{
						string part_name = part.Attributes["name"].Value;
						string part_start= part.Attributes["startpos"].Value;
						string part_end= part.Attributes["endpos"].Value;
						string file_full_name = filesdir + file_name;
						string part_full_name = dumpdir + part_name;
						len = Convert.ToInt64(part_end) - pos;
						if(File.Exists(part_full_name))
							this.write_part_back(part_full_name, file_full_name,pos,len, part_start);
						else this.msgbox(this, DialogFlags.Modal,MessageType.Error,ButtonsType.Close,"File: " + part_full_name + " does not exist!");
						pos += Convert.ToInt64(part_start);
				}
			}
		}
			
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
			while(input.BaseStream.Position <= posb)
			{
				byte data = input.ReadByte();
				if(data != 0x00)
					output.Write(data);
			}
		}
		catch(EndOfStreamException EOFS)
		{
			
		}
			output.Close();
			input.Close();
		
	}
	private void write_part_back(string infile, string outfile, long src_start, long src_end,string dst_start)
	{
			BinaryReader input = new BinaryReader(File.Open(infile,FileMode.Open,FileAccess.Read));
			BinaryWriter output = new BinaryWriter(File.Open(outfile,FileMode.Open,FileAccess.Write));
			long ldst_start =  Convert.ToInt64(dst_start);
			output.BaseStream.Seek(ldst_start,SeekOrigin.Begin);
			input.BaseStream.Seek(src_start, SeekOrigin.Begin);
			Console.WriteLine("src_start =" + src_start.ToString() + " ldst_start =" + ldst_start.ToString() + " " +  src_end.ToString());
			Console.WriteLine("Writing Data :");
		try{
			while(input.BaseStream.Position <=  src_end)
			{
				byte data = input.ReadByte();
				Console.WriteLine(Convert.ToChar(data));
				output.Write(data);
			}
		}
		catch(EndOfStreamException EOFS)
		{
			
		}
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
		BinaryWriter output = new BinaryWriter(
		                                       File.Open(
		                                                 file,
		                                                 FileMode.Open,
		                                                 FileAccess.Write
		                                                 )
		                                       );
		long count = 0;
		/*
		if(output.BaseStream.Length <= size)
		{
			while(output.BaseStream.Length <= size)
			{
				output.Write(0x00);
				count++;
			}
			return count;
		}
		else
		{
			return 0;
		}
		*/
		return 0;
		output.Close();
	}
	private void stripPadding(string file)
	{
		BinaryReader filer = new BinaryReader(File.Open(file,FileMode.Open,FileAccess.Read));
		BinaryWriter filew = new BinaryWriter(File.Open(file,FileMode.Open,FileAccess.Write));
		long count = filer.BaseStream.Length;
		

			while(filer.BaseStream.Length <= count)
			{
				byte data = filer.ReadByte();
				if(data != 0x00)
					filew.Write(0x00);
			}
		filew.Close();
		filer.Close();
	}
	private long getOverflow(string file, long size)
	{
		BinaryReader file_handle= new BinaryReader(File.Open(file,FileMode.Open,FileAccess.Read));
		long csize = file_handle.BaseStream.Length;
		file_handle.Close();
		return csize - size;
	}
}

		
	

