using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using Kontur.Mime;
using LumiSoft.Net.MIME;
using NUnit.Framework;
using OpenPop.Mime;
using log4net;
using log4net.Config;
using MailMessage = System.Web.Mail.MailMessage;

namespace banality.robot
{

	[TestFixture]
	public class Program_Test
	{
		[Test]
		public void Test()
		{
			var file = @"c:\work\banality.robot\banality.robot\bin\Debug\1337769437680.mime";
			var data = File.ReadAllBytes(file);
			var message = new Message(data);
			var subject = message.Headers.Subject;
			var mimeMessage = MIME_Message.ParseFromFile(file);
			var encoded = mimeMessage.Header["Subject"][0].ValueToString();
			var decoded = MIME_Encoding_EncodedWord.DecodeS(encoded);
			Console.WriteLine(encoded);
			Console.WriteLine(decoded);
			Console.WriteLine(subject);
			Console.WriteLine(message.ToMailMessage().Subject);
		}
	}
	class Program
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (Program));
		static void Main(string[] args)
		{
			XmlConfigurator.Configure(new FileInfo("log.config.xml"));
			log.Info("Started");
			var client = new FetchAllUnseenPop3Client("seen", "pop3.bk.ru", 110, "banality", ",4y4kmyjcnb");
			var replier = new Replier();
			var firstTime = true;
			var lastRegenerateTime = DateTime.MinValue;
			while (true)
			{
				log.Info("session start");
				try
				{
					using(var playersRepo = new PlayersRepo("players"))
					using(var answersRepo = new AnswersRepo("answers"))
					using(var tagsRepo = new TagsRepo("tags"))
					{
						var processing = new EmailProcessing(client, playersRepo, answersRepo, tagsRepo, replier);
						processing.ProcessInbox();
						if (processing.Changed || firstTime || DateTime.Now - lastRegenerateTime > TimeSpan.FromMinutes(1))
						{
							log.Info("Regenerating html files");
							firstTime = false;
							var gameResults = new GameLogic().Calculate(playersRepo, answersRepo, tagsRepo);
							new HtmlGenerator(gameResults).Regenerate("htmls");
							//SendToFtp("htmls");
							lastRegenerateTime = DateTime.Now;
						}
					}
					log.Info("session end");
				}
				catch (Exception e)
				{
					log.Error(e);
					log.Info("waiting 20 sec after crash...");
					Thread.Sleep(20000);
				}
				Thread.Sleep(10000);
			}
		}

		private static void SendToFtp(string path)
		{
			log.Info("Sending to ftp");
			var fullPath = Path.GetFullPath(path);
			var info = new ProcessStartInfo(@"c:\Windows\System32\ftp.exe", "-i -s:ftp.txt");
			info.WorkingDirectory = fullPath;
			info.CreateNoWindow = true;
			info.WindowStyle = ProcessWindowStyle.Hidden;
			info.UseShellExecute = false;
			info.RedirectStandardError = true;

			var process = Process.Start(info);
			var output = process.StandardError.ReadToEnd();
			if (!string.IsNullOrWhiteSpace(output))
				log.Error(output);
			var exitOk = process.WaitForExit(120000);
			if (!exitOk || process.ExitCode != 0)
			{
				log.Error("ftp failed! killing... ");
				process.Kill();
				process.WaitForExit(1000);
				throw new Exception("FTP send failed");
			}
		}
	}

	public class TagsRepo : Repo<List<TagInfo>>
	{
		public List<TagInfo> Tags { get { return items; }}

		public TagsRepo(string filename) : base(filename)
		{
			foreach (var t in items)
			{
				t.Tag = t.Tag.ToLower();
			}

		}

		public bool Contains(string tag)
		{
			return items.Any(t => t.Tag.ToLowerInvariant() == tag.ToLowerInvariant());
		}

		public void Add(TagInfo tagInfo)
		{
			Changed = true;
			items.Add(tagInfo);
		}
	}

	public class TagInfo
	{
		public string Tag { get; set; }
		public string Title { get; set; }
		public string Where { get; set; }
		public DateTime Start { get; set; }
		public DateTime Finish { get; set; }
		public bool Hidden { get; set; }
	}

	public class PlayerRegistrationInfo
	{
		public static PlayerRegistrationInfo Parse(string s)
		{
			var result = new PlayerRegistrationInfo();
			var parts = s.Split(new[] {','}).Select(part => part.Trim()).ToList();
			if (parts.Count > 0) result.Name = parts[0];
			if (parts.Count > 1) result.Position = parts[1];
			if (parts.Count > 2) result.Company = parts[2];
			return result;
		}

		public string Email { get; set; }
		public string Name { get; set; }
		public string Position { get; set; }
		public string Company { get; set; }
		public DateTime Timestamp { get; set; }

		public override string ToString()
		{
			return string.Format("Email: {0}, Name: {1}, Position: {2}, Company: {3}", Email, Name, Position, Company);
		}

		//TODO public bool Approved { get; set; }
	}

	public class PlayerAnswers
	{
		public string PlayerEmail { get; set; }
		public string Tag { get; set; }
		public DateTime Timestamp { get; set; }
		public string[] Answers { get; set; }
		//TODO public bool Approved { get; set; }
		public static PlayerAnswers Parse(string subject)
		{
			subject = subject.Replace('_', ' ').Replace(',', ' ');
			if (subject.EndsWith("."))
				subject = subject.Substring(0, subject.Length - 1);
			if (subject.StartsWith("FWD: ", StringComparison.InvariantCultureIgnoreCase))
				subject = subject.Substring(5, subject.Length - 5);
			if (subject.StartsWith("FW: ", StringComparison.InvariantCultureIgnoreCase))
				subject = subject.Substring(4, subject.Length - 4);
			if (subject.StartsWith("RE: ", StringComparison.InvariantCultureIgnoreCase))
				subject = subject.Substring(4, subject.Length - 4);
			string[] parts = subject.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			return new PlayerAnswers {Tag = parts[0], Answers = parts.Skip(1).Distinct().Take(5).ToArray()};
		}
	}
}
