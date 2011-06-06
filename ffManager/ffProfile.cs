using System;
using System.Xml;
namespace ffManager
{
	public class ffProfile
	{
		private string profile;
		private XmlDocument xml= new XmlDocument();
		public ffProfile ()
		{
		}
		public void setProfile(string file)
		{
			this.profile = file;
			this.xml= new XmlDocument();
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
			string game = this.getGame();
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
			switch(game)
			{
				case "cod4":
				case "mw2":
				case "waw":
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

