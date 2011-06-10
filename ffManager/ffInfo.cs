using System;
using System.IO;
using System.Text;
namespace ffManager
{
	public class ffInfo
	{
		private string fastfile; 
		public ffInfo (string file)
		{
			this.fastfile = file;
		}
		public string getHeader()
		{
			BinaryReader datain = new BinaryReader(
			                                       File.OpenRead(this.fastfile)
			                                       );
			string datout = "";
			for(int i=0; i < 10; i++)
			{
				datout += Convert.ChangeType(datain.ReadByte(),TypeCode.String);
			}
			return datout;
		}
		public string getVersion()
		{
			BinaryReader datain = new BinaryReader(
			                                       File.OpenRead(this.fastfile)
			                                       );
			datain.BaseStream.Seek(10,SeekOrigin.Begin);
			
			byte[] header;
			string data = "";
			int c;
			header = datain.ReadBytes(2);
			foreach(byte piece in header)
			{
				c= Convert.ToInt32(piece);
				data += c.ToString();
			}
			Int32 version = Convert.ToInt32(data);
			switch(version)
			{
				case 113:
					return "mw2";
				break;
				case 1131:
					return "waw";
				break;
				case 1:
					return "cod4";
				default:
					return "invalid";
			}
		}
	}
	
}

