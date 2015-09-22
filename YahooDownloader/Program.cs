using System;
using System.Net;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace YahooDownloader
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var urlPrototype = @"http://ichart.finance.yahoo.com/table.csv?s={0}&a={1}&b={2}&c={3}&d={4}&e={5}&f={6}&g={7}&ignore=.csv";
			if (args.Length != 2) {
				Console.WriteLine ("Usage: YahooDownloader SYMBOL RESOLUTION");
				Console.WriteLine ("SYMBOL = eg SPY");
				Console.WriteLine ("RESOLUTION = d for daily data, w for weekly data");
				Environment.Exit (1);
			}
			var symbol = args [0];
			var resolution = args [1];
			var startMonth = 01;
			var startDay = 01;
			var startYear = 1990;
			var finishMonth = DateTime.Today.Month;
			//we subtract one day to make sure we have data from yahoo
			var finishDay = DateTime.Today.Subtract (TimeSpan.FromDays (1)).Day;
			var finishYear = DateTime.Today.Year;
			// The Yahoo Finance URL for each parameter
			var url = string.Format(urlPrototype, symbol, startMonth, startDay, startYear, finishMonth, finishDay, finishYear, resolution);
			try
			{
				WebClient cl = new WebClient();
				var lines = cl.DownloadString(url);
				var lines_ = lines.Split('\n');
				string temppath = Path.GetTempPath();
				string file = symbol + ".csv";
				string fullpath = Path.Combine(temppath,file);
				File.Delete(fullpath);
				using(StreamWriter swfile = new System.IO.StreamWriter(fullpath))
				{
					for (var i=lines_.Length - 1;i >= 1;i--)
					{
						var str_ = lines_[i].Split(',');
						if (str_.Length < 6)
							continue;
						var ymd = str_[0].Split('-');
						var year = ymd[0];
						var month = ymd[1];
						var day = ymd[2];
						var open = 0.0m;
						var high = 0.0m;
						var low = 0.0m;
						var close = 0.0m;
						int volume = 0;
						Decimal.TryParse(str_[1],out open);
						Decimal.TryParse(str_[2],out high);
						Decimal.TryParse(str_[3],out low);
						Decimal.TryParse(str_[4],out close);
						Int32.TryParse(str_[5],out volume);
						int _open, _high, _low, _close;
						_open = Decimal.ToInt32(10000*open);
						_high = Decimal.ToInt32(10000*high);
						_low = Decimal.ToInt32(10000*low);
						_close = Decimal.ToInt32(10000*close);
						var s = "{0}{1}{2} 00:00,{3},{4},{5},{6},{7}";
						var sf = string.Format(s,year,month,day,_open,_high,_low,_close,volume);
						swfile.WriteLine(sf);
					}
				}
				string zipfile = symbol + ".zip";
				string curdir = Directory.GetCurrentDirectory();
				string zipfullpath = Path.Combine(curdir,zipfile);
				File.Delete(zipfullpath);
				using(ZipArchive _zip = ZipFile.Open(zipfullpath,ZipArchiveMode.Create))
				{
					_zip.CreateEntryFromFile(fullpath,file);
				}
			}
			catch(Exception ex)
			{
				Console.Write("Error: " + ex.Message);
				Environment.Exit(1);
			}

		}
	}
}
