using System;
using System.Globalization;

namespace banality.robot
{
	public class ParsedSubject
	{
		public ParsedSubject(string subject)
		{
			if (string.IsNullOrWhiteSpace(subject))
				return;
			string trimmedSubject = subject.Trim();
			int delimiterIndex = trimmedSubject.IndexOf(' ');
			if (delimiterIndex < 0) return;
			string cmd = trimmedSubject.Substring(0, delimiterIndex).ToLowerInvariant();
			string rightPart = trimmedSubject.Substring(delimiterIndex + 1);
			if (cmd.ToLower(CultureInfo.InvariantCulture) == "reg")
				RegInfo = new PlayerRegistrationInfo {Name = rightPart};
			else
				Answers = PlayerAnswers.Parse(trimmedSubject.Replace("¸", "å").ToLowerInvariant());
		}

		public PlayerRegistrationInfo RegInfo { get; set; }
		public PlayerAnswers Answers { get; set; }
	}
}