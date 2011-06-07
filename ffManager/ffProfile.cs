using System;
using System.Xml;
namespace ffManager
{
	public class ffProfile
	{
		private string profile;
		private ffInfo fastfile_info;
		private XmlDocument xml= new XmlDocument();
		public ffProfile ()
		{
		}
		public void setProfile(string file,string fastfile)
		{
			this.profile = file;
			this.xml= new XmlDocument();
			this.fastfile_info= new ffInfo(fastfile);
		}	
		public bool isValid()
		{
			try
			{
				this.xml.Load(this.profile);
			}
			catch(XmlException xmle)
			{
				return false;
			}
			string console = this.getConsole();
			int valid = 0;
			switch(console)
			{
				case "ps3":
				case "xbox":
					valid = 1;
				break;
				default:
					valid = 0;
				break;
			}
			if(valid == 0)
				return false;
			return true;
		}
		public string getConsole()
		{
			XmlNodeList console=	this.xml.GetElementsByTagName("console");
			return console[0].InnerXml;
		}
		public string getGame()
		{
			XmlNodeList game=	this.xml.GetElementsByTagName("game");
			return game[0].InnerXml;
		}
		public string getPackedDump()
		{
			XmlNodeList dump=	this.xml.GetElementsByTagName("extract_file");
			return  dump[0].InnerXml;
		}
		public XmlNodeList getFileList()
		{
			XmlNodeList list = this.xml.GetElementsByTagName("file");
			return list;
		}
	}
}

