using System;
using System.Globalization;
using System.IO;

namespace Kiccc.Ing.PcPos.Logger
{
	internal static class Logger
	{
		public static void Log(Kiccc.Ing.PcPos.Logger.Logger.Type type, string message, string logPath)
		{
			if (!Directory.Exists(logPath))
			{
				Directory.CreateDirectory(logPath);
			}
			PersianCalendar persiancalendar = new PersianCalendar();
			using (StreamWriter f = new StreamWriter(string.Format("{0}\\{1}{2}{3}.txt", new object[] { logPath, persiancalendar.GetYear(DateTime.Now), persiancalendar.GetMonth(DateTime.Now), persiancalendar.GetDayOfMonth(DateTime.Now) }), true))
			{
				object[] year = new object[] { persiancalendar.GetYear(DateTime.Now), persiancalendar.GetMonth(DateTime.Now), persiancalendar.GetDayOfMonth(DateTime.Now), null, null, null };
				DateTime now = DateTime.Now;
				year[3] = now.Hour;
				now = DateTime.Now;
				year[4] = now.Minute;
				now = DateTime.Now;
				year[5] = now.Second;
				string log = string.Format("{0} | {1} : {2} \r\n", string.Format("{0}/{1}/{2} {3}:{4}:{5}", year), type, message);
				f.Write(log);
				f.Dispose();
			}
		}

		internal enum Type
		{
			Exception,
			Description,
			Response,
			Request
		}
	}
}