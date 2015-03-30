#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Globalization;

#endregion

namespace LinuxProcess
{
	#region Linux Process

	public class LinuxProcess
	{
		#region Constant

		private const string PsCommand = "ps";
		private const string PsCommandArguments = "-eo pid,lstart,cmd";
		private const char SpaceChar = ' ';
		private const int DateTimeStartIndex = 1;
		private const int DateTimeLength = 24;
		private const int CommandStartIndex = 26;
		private const string DateTimeFormat = "ddd MMM d HH:mm:ss yyyy";
		private const string KillCommand = "kill";
		private const int StringFirstIndex = 0;

		#endregion

		#region Member

		public ulong Id { get; set; }
		public DateTime Start { get; set; }
		public string Command { get; set; }

		#endregion

		#region Static Variables

		private static string[] ProcessListSeparator = new string[] { Environment.NewLine };

		private static ProcessStartInfo PsProcessStartInfo = new ProcessStartInfo()
		{
			FileName = LinuxProcess.PsCommand,
			Arguments = LinuxProcess.PsCommandArguments,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			UseShellExecute = false,
		};

		#endregion

		#region Static Methods

		#region Get Process List

		public static List<LinuxProcess> GetProcessList(Func<LinuxProcess, bool> match = null)
		{
			var result = new List<LinuxProcess>();

			foreach (var line in Process.Start(LinuxProcess.PsProcessStartInfo).StandardOutput.ReadToEnd().Split(LinuxProcess.ProcessListSeparator, StringSplitOptions.None).Skip(1).Select(item => item.TrimStart()))
			{
				var firstSpaceIndex = line.IndexOf(LinuxProcess.SpaceChar);
				if (firstSpaceIndex < LinuxProcess.StringFirstIndex)
				{
					continue;
				}

				ulong id;
				if (!ulong.TryParse(line.Substring(LinuxProcess.StringFirstIndex, firstSpaceIndex), out id))
				{
					continue;
				}

				DateTime start;
				if (!DateTime.TryParseExact(line.Substring(firstSpaceIndex + LinuxProcess.DateTimeStartIndex, LinuxProcess.DateTimeLength), LinuxProcess.DateTimeFormat, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowWhiteSpaces, out start))
				{
					continue;
				}

				var linuxProcess = new LinuxProcess()
				{
					Id = id,
					Start = start,
					Command = line.Substring(firstSpaceIndex + LinuxProcess.CommandStartIndex)
				};

				if (match != null)
				{
					if (!match(linuxProcess))
					{
						continue;
					}
				}

				result.Add(linuxProcess);
			}

			return result;
		}

		#endregion

		#region Kill

		public static void Kill(ulong id)
		{
			Process.Start(new ProcessStartInfo()
			{
				FileName = LinuxProcess.KillCommand,
				Arguments = id.ToString(),
				CreateNoWindow = true,
				UseShellExecute = false,
			});
		}

		#endregion

		#endregion
	}

	#endregion
}