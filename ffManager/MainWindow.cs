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
	private string game;
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
					this.msgbox(DialogFlags.DestroyWithParent,MessageType.Error,ButtonsType.Close,"ERROR: Wine was not found on your system!\n\t\tPlease install wine and re-run ffManager..");
					this.menubar1.Sensitive = false;
					this.notebook1.Sensitive= false;
				}
				if(!File.Exists(Directory.GetCurrentDirectory() + "/offzip.exe"))
				{
					this.msgbox(DialogFlags.DestroyWithParent,MessageType.Error,ButtonsType.Close,"ERROR: offzip.exe was NOT found in the ffManager folder!\n\t\tPlease re-download and re-run ffManager..");
					this.menubar1.Sensitive = false;
					this.notebook1.Sensitive= false;
				}
				if(!File.Exists( Directory.GetCurrentDirectory() +"/packzip.exe"))
				{
					this.msgbox(DialogFlags.DestroyWithParent,MessageType.Error,ButtonsType.Close,"ERROR: packzip.exe was NOT found in the ffManager folder!\n\t\tPlease re-download and re-run ffManager..");
					this.menubar1.Sensitive = false;
					this.notebook1.Sensitive= false;
				}
		}
		else if(this.platform == "win32")
		{
				if(!File.Exists( Directory.GetCurrentDirectory() + @"\offzip.exe"))
				{
					this.msgbox(DialogFlags.DestroyWithParent,MessageType.Error,ButtonsType.Close,"ERROR: offzip.exe was NOT found in the ffManager folder!\n\t\tPlease re-download and re-run ffManager..");
					this.menubar1.Sensitive = false;
					this.notebook1.Sensitive= false;
				}
				if(!File.Exists( Directory.GetCurrentDirectory() + @"\packzip.exe"))
				{
					this.msgbox(DialogFlags.DestroyWithParent,MessageType.Error,ButtonsType.Close,"ERROR: packzip.exe was NOT found in the ffManager folder!\n\t\tPlease re-download and re-run ffManager..");
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
		//string fastfile_header = fastfile_data.getHeader();
		string fastfile_version= fastfile_data.getVersion();
		/*
		if(fastfile_header != "IWff0100" && fastfile_header != "IWffu100" )
		{
			this.msgbox( DialogFlags.DestroyWithParent,MessageType.Error,ButtonsType.Ok,"Oops! You either gave a corrupt fast file or a file that is NOT a fast file...\n Please open a VALID fastfile!");
			//return;
		}
		else if(fastfile_header == "IWffs100" )
		{
			this.msgbox( DialogFlags.DestroyWithParent,MessageType.Error,ButtonsType.Ok,"Oops! You gave a SIGNED fastfile. ffManager does not support these\n Please open a VALID UN-SIGNED fastfile!");
			//return;
		}
		*/
		if(fastfile_version == "invalid")
		{
			this.msgbox( DialogFlags.DestroyWithParent,MessageType.Error,ButtonsType.Ok,"Oops! You gave a FastFile for a game ffManager does not support...\n Please open a VALID SUPPORTED fastfile!");
			return;
		}
		else
		{
			this.game = fastfile_version;
			Console.WriteLine(this.game);
		}
		string ffdir = ffinfo.Directory.FullName;
			string workdir = "";
			string dumpdir= "";
			string filesdir= "";
		if(this.platform == "win32")
		{
			workdir = ffdir + @"\" + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + @"\";
			dumpdir= workdir +  "dump" + @"\";
			filesdir= workdir + "files"+ @"\";
		}
		else if(this.platform == "unix")
		{
			workdir = ffdir + "/" + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + "/";
			dumpdir= workdir +  "dump" + "/";
			filesdir= workdir + "files"+ "/";
		}
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
			this.msgbox(
			            DialogFlags.Modal,
			            MessageType.Error,
			            ButtonsType.Close,
			            "Please select a Valid FastFile Profile"
			            );
			return;	
		}
		this.btn_decomp.Sensitive = false;
		this.console= this.ffprofile.getConsole();
		if(this.console == "ps3")
		{
			ffManager.Decompress.PS3.Decompressor  ps3_decomp= new ffManager.Decompress.PS3.Decompressor(this.opened_ff_file,this.ffprofile, this);
			ps3_decomp.decompress();
		}
		else if(this.console == "xbox")
		{
			ffManager.Decompress.XBOX.Decompressor  ps3_decomp= new ffManager.Decompress.XBOX.Decompressor(this.opened_ff_file,this.ffprofile, this);
			ps3_decomp.decompress();
		}
		this.btn_comp.Sensitive = true;
	}
	
	protected virtual void ff_profile_open_callback (object sender, System.EventArgs e)
	{
		if(this.ffprofile_chooser.Filename == "" || this.ffprofile_chooser.Filename == null)
			return;
		this.ffprofile.setProfile( this.ffprofile_chooser.Filename, this.opened_ff_file);
		if(this.ffprofile.isValid() == false)
		{
			this.msgbox(
			            DialogFlags.Modal,
			            MessageType.Error,
			            ButtonsType.Close,
			            "Invalid FastFile Profile"
			            );
			return;	
		}

	}
	protected virtual void btn_comp_pressed (object sender, System.EventArgs e)
	{
		
		if(this.ffprofile_chooser.Filename == "" || this.ffprofile_chooser.Filename == null || !this.ffprofile.isValid())
		{
			this.msgbox(
			            DialogFlags.Modal,
			            MessageType.Error,
			            ButtonsType.Close,
			            "Please select a Valid FastFile Profile"
			            );
			return;	
		}
		this.btn_decomp.Sensitive = true;
		this.console= this.ffprofile.getConsole();
		if(this.console == "ps3")
		{
			ffManager.Compress.PS3.Compressor  ps3_comp= new ffManager.Compress.PS3.Compressor(this.opened_ff_file,this.ffprofile, this);
			ps3_comp.compress();
		}
		this.msgbox( DialogFlags.Modal,MessageType.Info,ButtonsType.Ok,"FastFile " + this.opened_ff_file + " Re-Compressed");
		this.btn_comp.Sensitive = true;
	}
	 public void msgbox(DialogFlags flags, MessageType type, ButtonsType btype, string msg)
		{
			MessageDialog msgb = new MessageDialog(this,
		                                      flags,
		                                      type,
		                                      btype,
		                                      msg
		                                      );
			msgb.Run();
			msgb.Destroy();
			msgb.Dispose();
		
		}

	public static string searchForFile(string filename,string fastfile)
	{
		FileInfo ffinfo = new FileInfo(fastfile);
		string ffdir = ffinfo.Directory.FullName;
		string dumpdir="";
		if(MainWindow.getOS() == "win32")
			 dumpdir = ffdir + @"\" + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" + @"\" + "dump" + @"\";
		else if(MainWindow.getOS() == "unix")
			dumpdir = ffdir + "/" + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work" +  "/" + "dump" +  "/";
		string[] list = {"dat","inn","nge","mbs","ase","vvv","neo","img","ttf","dc5","nta","fnc"};
		foreach(string ext in list)
		{
						
			if(File.Exists(dumpdir + filename + "." + ext))
				return filename + "."+ ext;
			
		}
		return "";
	}
}

		
	

