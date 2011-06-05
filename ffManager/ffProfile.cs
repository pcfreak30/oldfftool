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
			return true;
		}
		public string getFormat()
		{
			XmlNodeList format=	this.xml.GetElementsByTagName("ff_format");
			return format[0].InnerXml;
		}
	}
}

