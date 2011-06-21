using System;
using System.Xml;
using System.IO;
using System.Threading;
using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System.Web;
using System.Net;
namespace ffManager
{
	class MainClass
	{
		public static string ffversion = "";
		public static string fastfile = "";
		public static string profile = "";
		public static string console = "";
		private static string DS = ffManager.MainClass.getOS() == "win32" ? @"\" : "/";
		public static string cwd = "";
		public static void Main (string[] args)
		{
				string cwd_exe = System.Reflection.Assembly.GetExecutingAssembly().Location.ToString();
				FileInfo exeinfo = new FileInfo(cwd_exe);
				MainClass.cwd = exeinfo.Directory.FullName;
				if(!File.Exists(MainClass.cwd + MainClass.DS + "offzip.exe") || !File.Exists(MainClass.cwd + MainClass.DS + "packzip.exe"))
				{
					Console.WriteLine("Opps, seems you are missing required files!");
					Console.WriteLine("Please Re-Download ffManager from SimplyHacks.com...");
					Console.WriteLine("Press Enter to Continue");
					Console.Read();
					return;
				}
			    Console.WriteLine("ffManager Tool\n");
    			Console.WriteLine("Version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + "\n");
				Console.WriteLine("------------------------------\n");
    			Console.WriteLine("Created by Derrick Hammer, A.K.A. PCFreak30\n");
    			Console.WriteLine("Released under GPLv3 License, http://www.gnu.org/licenses/gpl.html\n\n");
    			Console.WriteLine("Visit PCFreak30.com for News and Updates\n\n");
    			Console.WriteLine("Visit SimplyHacks.com for Releases and a fun, leech-free community..\n");
    			Console.WriteLine("------------------------------\n");
				Thread.Sleep(3000);
				Console.Clear();
				if(args.Length == 1)
				{
					args[0] = args[0].Trim().Replace("'","").Replace(@"""","");
					isValidFastFile(args[0], false);
					if(isValidProfile(args[0]))
						MainClass.profile = args[0];
				}
				if(args.Length == 2)
				{
					args[0] = args[0].Trim().Replace("'","").Replace(@"""","");
					args[1] = args[1].Trim().Replace("'","").Replace(@"""","");
					isValidFastFile(args[0], false);
					isValidFastFile(args[1], false);
					if(isValidProfile(args[0]))
						MainClass.profile = args[0];
					if(isValidProfile(args[1]))
						MainClass.profile = args[1];
				}
				if(MainClass.checkForUpdate())
				{
					Console.WriteLine("Update Complete! Please re-run ffManager to use the new version");
					Console.WriteLine("Thank You");
					Console.WriteLine("Press ENTER to continue");
					Console.Read();
				}
				else
				{
					MainClass.showOptions();
				}
		}
		private static void showOptions()
		{
			Console.Clear();
			Console.WriteLine("\t\t\tPlease select an action AND press ENTER:");
			Console.WriteLine("");
			if(MainClass.fastfile.Length > 0)
				Console.WriteLine("1: Set the FastFile to use\n\t\tCurrent:" + MainClass.fastfile + "\n\t\tVersion: " + MainClass.ffversion);
			else
				Console.WriteLine("1: Set the FastFile to use");
			if(MainClass.profile.Length > 0)
				Console.WriteLine("2: Set the FastFile FFXML profile to use\n\t\tCurrent:" + MainClass.profile);
			else
				Console.WriteLine("2: Set the FastFile FFXML profile to use");
			if(MainClass.console.Length > 0)
				Console.WriteLine("3: Set the Console\n\t\tCurrent:  " + MainClass.console);
			else
			Console.WriteLine("3: Set the Console");
			Console.WriteLine("4: De-Compress the FastFile");
			Console.WriteLine("5: Compress the FastFile");
			Console.WriteLine("6: Quit");
			Console.WriteLine("");
			Console.Write("Choice:");
			string choice = Console.ReadLine().Trim();
			switch(choice)
			{
				case "1":
						MainClass.promptFastFile();
						break;
				case "2":
						MainClass.promptProfile();
						break;
				case "3":
						MainClass.promptConsole();
						break;
				case "4":
						MainClass.doDecompress();
						break;
				case "5":
						MainClass.doCompress();
						break;
				case "6":
						return;
						break;
					default:
						MainClass.showOptions();
						break;
			}
		}
		private static void promptFastFile()
		{
			Console.Clear();
			Console.WriteLine("Drag and drop a FastFile into this window AND press ENTER: ");
			string file = Console.ReadLine().Trim().Replace("'","").Replace(@"""","");
			if(file == "")
				MainClass.showOptions();
			if(MainClass.isValidFastFile(file))
			{
				switch(MainClass.ffversion)
				{
					case "cod4":
							Console.WriteLine("Call of Duty 4 FastFile Detected");
							break;
					case "cod5":
							Console.WriteLine("Call of Duty World at War FastFile Detected");
							break;
					case "mw2":
							Console.WriteLine("Modern Warfare 2 FastFile Detected");
							break;
				}
				Thread.Sleep(1500);
				MainClass.showOptions();
			}
			else
				MainClass.promptFastFile();
		}
		private static bool isValidFastFile(string file, bool showError = true)
		{
			if(!File.Exists(file))
				return false;
			
			BinaryReader binread = new BinaryReader( File.OpenRead(file));
			string header =new string(binread.ReadChars(8)).Trim().ToLower();
			binread.BaseStream.Seek(10,SeekOrigin.Begin);
			byte[] verb = binread.ReadBytes(2);
			binread.Close();
			string verba = "";
			for(int i=0; i < verb.Length; i++)
			{
				verba = verba + int.Parse(verb[i].ToString()).ToString();
			}
			int version = int.Parse(verba);
			if(header != "iwff0100" && header != "iwffu100")
			{
				if(showError)
				{
					Console.WriteLine("This is NOT a valid FastFile");
					Thread.Sleep(2000);
				}
				return false;
			}
			else if(header == "iWffs100")
			{
				if(showError)
				{
					Console.WriteLine("ffManager does NOT support signed fastfiles");
					Thread.Sleep(2000);
				}
				return false;
			}
			
			switch(version)
			{
			case 1:
				MainClass.ffversion = "cod4";
				MainClass.fastfile= file;
				return true;
				break;
				case 113:
				MainClass.ffversion = "mw2";
				MainClass.fastfile= file;
				return true;
				break;
				case 1131:
				MainClass.ffversion = "cod5";
				MainClass.fastfile= file;
				return true;
				break;
				default:
				Console.WriteLine("ffManager does not suport FastFiles of that version...");
				Thread.Sleep(2000);
				return false;
				break;
			}
		}
		
		private static void promptProfile()
		{
			Console.Clear();
			Console.WriteLine("Drag and drop a FastFile FFXML Profile into this window AND press ENTER: ");
			string file = Console.ReadLine().Trim().Replace("'","").Replace(@"""","");
			if(file == "")
				MainClass.showOptions();
			if(MainClass.isValidProfile(file))
			{
				MainClass.profile = file;
				Console.WriteLine("Profile "+ file +" set");
				Thread.Sleep(1500);
				MainClass.showOptions();
			}
			else
				MainClass.promptProfile();
		}
		private static bool isValidProfile(string file)
		{
			if(!File.Exists(file))
				return false;
			XmlDocument xml = new XmlDocument();
			try
			{
				xml.Load(file);
				return true;
			}
			catch (XmlException xmlex)
			{
				return false;
			}
		}
		private static void promptConsole()
		{
			Console.Clear();
			Console.WriteLine("Please choose the console format for the FastFile: ");
			Console.WriteLine("Valid Options are:\n\t--> PS3\n\t-->XBOX\n(Not case sensitive)\n\nChoice:");
			string console = Console.ReadLine().Trim();
			if(console == "")
				MainClass.showOptions();
			if(console == "ps3" || console == "xbox")
			{
				MainClass.console = console;
				MainClass.showOptions();
			}
			else
				MainClass.promptConsole();
		}
		
		private static void doDecompress()
		{
			if(MainClass.fastfile.Length == 0)
			{
				Console.WriteLine("ERROR: FastFile not set. Going to FastFile selection...");
				Thread.Sleep(2000);
				MainClass.promptFastFile();
			}
			else if(MainClass.profile.Length == 0)
			{
				Console.WriteLine("ERROR: FastFile FFXML Profile not set. Going to profile selection...");
				Thread.Sleep(2000);
				MainClass.promptProfile();
			}
			else if(MainClass.console.Length == 0)
			{
				Console.WriteLine("ERROR: Console not set. Going to console selection...");
				Thread.Sleep(2000);
				MainClass.promptConsole();
			}
			else
			{
				FileInfo ffinfo = new FileInfo(MainClass.fastfile);
				string dir = ffinfo.DirectoryName + MainClass.DS + ffinfo.Name.Replace(ffinfo.Extension,"_work");
				if(MainClass.ffversion == "cod4")
				{
					COD4_Decompress cod = new COD4_Decompress(MainClass.fastfile,MainClass.console);
					cod.decompress(dir, MainClass.profile);
					ArrayList missing = cod.getMissingFiles();
					if(missing.Count > 0)
					{
						foreach(ArrayList element in missing)
						{
							Console.WriteLine("Missing Data " + element[0] + " for file "+ element[1]);
						}
					}
				}
				else if(MainClass.ffversion == "cod5")
				{	
					COD5_Decompress cod = new COD5_Decompress(MainClass.fastfile,MainClass.console);
					cod.decompress(dir, MainClass.profile);
					ArrayList missing = cod.getMissingFiles();
					if(missing.Count > 0)
					{
						foreach(ArrayList element in missing)
						{
							Console.WriteLine("Missing Data " + element[0] + " for file "+ element[1]);
						}
					}
				}
				else if(MainClass.ffversion == "mw2")
				{	
					MW2_Decompress cod = new MW2_Decompress(MainClass.fastfile,MainClass.console);
					cod.decompress(dir, MainClass.profile);
					ArrayList missing = cod.getMissingFiles();
					if(missing.Count > 0)
					{
						foreach(ArrayList element in missing)
						{
							Console.WriteLine("Missing Data " + element[0] + " for file "+ element[1]);
						}
					}
				}
				Console.WriteLine("Press ENTER to continue back to program options..");
				Console.ReadLine();
				MainClass.showOptions();
			}
		}
		
		private static void doCompress()
		{
			FileInfo ffinfo = new FileInfo(MainClass.fastfile);
			string dir = ffinfo.DirectoryName + MainClass.DS + ffinfo.Name.Replace(ffinfo.Extension,"_work");
			if(!Directory.Exists(dir) || !Directory.Exists(dir + DS + "raw") || !Directory.Exists(dir + DS + "scripts"))
			{
				Console.WriteLine("ERROR: You have not De-Compressed the currently set FastFile!");
				Thread.Sleep(2000);
				MainClass.promptFastFile();
			}
			else
			{
				if(MainClass.ffversion == "cod4")
				{
					COD4_Compress cod = new COD4_Compress(MainClass.fastfile,MainClass.console);
					cod.compress(dir, MainClass.profile);
					ArrayList missing = cod.getMissingFiles();
					ArrayList overflow = cod.getOverflow();
					if(missing.Count > 0)
					{
						foreach(ArrayList element in missing)
						{
							Console.WriteLine("Missing Data " + element[0] + " for file "+ element[1]);
						}
					}
					if(overflow.Count > 0)
					{
						foreach(ArrayList element in  overflow )
						{
							Console.WriteLine("Overflow in " + element[0] + "! Max size is: "+ element[1] + " -- Overflow is: " + element[2]);
						}
					}
				}
				else if(MainClass.ffversion == "cod5")
				{	
					COD5_Compress cod = new COD5_Compress(MainClass.fastfile,MainClass.console);
					cod.compress(dir, MainClass.profile);
					ArrayList missing = cod.getMissingFiles();
					ArrayList overflow = cod.getOverflow();
					if(missing.Count > 0)
					{
						foreach(ArrayList element in missing)
						{
							Console.WriteLine("Missing Data " + element[0] + " for file "+ element[1]);
						}
					}
					if(overflow.Count > 0)
					{
						foreach(ArrayList element in  overflow )
						{
							Console.WriteLine("Overflow in " + element[0] + "! Max size is: "+ element[1] + " -- Overflow is: " + element[2]);
						}
					}
				}
				else if(MainClass.ffversion == "mw2")
				{	
					MW2_Compress cod = new MW2_Compress(MainClass.fastfile,MainClass.console);
					cod.compress(dir, MainClass.profile);
					ArrayList missing = cod.getMissingFiles();
					ArrayList overflow = cod.getOverflow();
					if(missing.Count > 0)
					{
						foreach(ArrayList element in missing)
						{
							Console.WriteLine("Missing Data " + element[0] + " for file "+ element[1]);
						}
					}
					if(overflow.Count > 0)
					{
						foreach(ArrayList element in  overflow )
						{
							Console.WriteLine("Overflow in " + element[0] + "! Max size is: "+ element[1] + " -- Overflow is: " + element[2]);
						}
					}
				}
				Console.WriteLine("Press ENTER to continue back to program options..");
				Console.ReadLine();
				MainClass.showOptions();
			}
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
		public static string GetMD5HashFromFile(string fileName)
		{
			FileStream file = new FileStream(fileName, FileMode.Open);
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] retVal = md5.ComputeHash(file);
			file.Close();
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < retVal.Length; i++)
			{
				sb.Append(retVal[i].ToString("x2"));
			}
			return sb.ToString();
		}
		private static bool checkForUpdate()
		{
			string version = "";
			try
			{
			XmlTextReader reader = new XmlTextReader("http://www.simplyhacks.com/ffManager-version.xml");
			reader.Read();
			reader.MoveToContent();
			while (!reader.EOF && reader.Name != "version")
			{
				reader.Read();
			}
			version = reader.ReadString();
			}
			catch(Exception execp)
			{
				return false;
			}
			if(version == System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString())
				return false;
			if(MainClass.promptUpdate(version))
			{
				try
				{
				MainClass.doUpdate();
				}
				catch(Exception execp)
				{
					return false;
				}
				return true;
			}
			return false;			
		}
		
		private static bool promptUpdate(string version)
		{
			Console.Clear();
			Console.WriteLine("**UPDATE NOTICE**");
			Console.WriteLine("The latest version of ffManager is " + version + "..You have version "+ System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\n Would you like to update? YES or NO: ");
			string answer = Console.ReadLine().Trim().ToLower();
			if(answer == "yes")
			  	return true;
			else if(answer == "no")
				return false;
			else
				return MainClass.promptUpdate(version);
		}
		
		private static void doUpdate()
		{
			Console.Clear();
			string updateFile = "." + MainClass.DS + "ffManager-updated.exe";
			string origFile = "." + MainClass.DS + "ffManager.exe";
			if(File.Exists(updateFile))
			   File.Delete(updateFile);
			WebClient req = new WebClient();
			req.DownloadFile("http://www.simplyhacks.com/ffManager.exe",updateFile);
			if(File.Exists(origFile))
			   File.Delete(origFile);
			File.Move(updateFile,origFile);
		}
	}
}

