using System;
using Gtk;
using System.IO;
using System.Diagnostics;
using ffManager;
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
					Console.WriteLine(Excep.Message.ToString());
					return;
				}
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
	}
	protected virtual void btn_decomp_pressed (object sender, System.EventArgs e)
	{
		if(this.ffprofile_chooser.Filename == "" || this.ffprofile_chooser.Filename == null || !this.ffprofile.isValid())
		{
			MessageDialog msg = new MessageDialog(this,
			                                      DialogFlags.Modal,
			                                      MessageType.Error,
			                                      ButtonsType.Close,
			                                      "Please select a Valid FastFile Profile"
			                                      );
			msg.Run();
			msg.Destroy();
			msg.Dispose();
			return;	
		}
		Process p = new Process();
		FileInfo ffinfo = new FileInfo(this.opened_ff_file);
		string ffdir = ffinfo.Directory.FullName;
		string fslash = "";
		if(this.platform == "win32")
			fslash = @"\";
		else if(this.platform == "unix")
			fslash = "/";
		string workdir = ffdir + fslash + ffinfo.Name.Replace(ffinfo.Extension,"") + "_work";
		string ffformat = this.ffprofile.getFormat();
		if(this.platform == "win32")
		{
			ProcessStartInfo info = p.StartInfo;
			info.FileName="offzip";
			if(ffformat == "xbox")
				info.Arguments="-a " + @"""" + this.opened_ff_file + @"""" + " " + @"""" + workdir + @"""" + " 0";
			else if(ffformat == "ps3")
				info.Arguments="-a -z -15 " + @"""" + this.opened_ff_file + @"""" + " " + @"""" + workdir + @"""" + " 0";
			info.WindowStyle = ProcessWindowStyle.Normal;
		}
		else if(this.platform == "unix")
		{
			ProcessStartInfo info = p.StartInfo;
			info.FileName="wine";
			if(ffformat == "xbox")
				info.Arguments=" offzip -a " + @"""" + this.opened_ff_file + @"""" + " " + @"""" + workdir + @"""" + " 0";
			else if(ffformat == "ps3")
				info.Arguments="offzip -a -z -15 " + @"""" + this.opened_ff_file + @"""" + " " + @"""" + workdir + @"""" + " 0";
			info.WindowStyle = ProcessWindowStyle.Normal;
		}
		p.Start();
		p.WaitForExit();
		if(ffformat == "xbox")
			this.processFiles_XBOX();
		else if(ffformat == "ps3")
			this.processFiles_PS3();
	}
	
	protected virtual void ff_profile_open_callback (object sender, System.EventArgs e)
	{
		if(this.ffprofile_chooser.Filename == "" || this.ffprofile_chooser.Filename == null)
			return;
		this.ffprofile.setProfile(this.ffprofile_chooser.Filename);
		if(!this.ffprofile.isValid())
		{
			MessageDialog msg = new MessageDialog(this,
			                                      DialogFlags.Modal,
			                                      MessageType.Error,
			                                      ButtonsType.Close,
			                                      "Invalid FastFile Profile"
			                                      );
			msg.Run();
			msg.Destroy();
			msg.Dispose();
			return;	
		}

	}
		private void processFiles_XBOX()
		{
			
		}
		private void processFiles_PS3()
		{
			
		}
}

		
	

