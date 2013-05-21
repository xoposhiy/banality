using System;
using System.Globalization;
using OpenPop.Mime;
using log4net;
using System.Linq;

namespace banality.robot
{
	internal class EmailProcessing
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (EmailProcessing));
		private readonly AnswersRepo answersRepo;
		private readonly FetchAllUnseenPop3Client client;
		private readonly PlayersRepo playersRepo;
		private readonly Replier replier;
		private readonly TagsRepo tagsRepo;

		public EmailProcessing(FetchAllUnseenPop3Client client, PlayersRepo playersRepo, AnswersRepo answersRepo,
		                       TagsRepo tagsRepo, Replier replier)
		{
			this.client = client;
			this.playersRepo = playersRepo;
			this.answersRepo = answersRepo;
			this.tagsRepo = tagsRepo;
			this.replier = replier;
		}

		public bool Changed { get; private set; }

		public void ProcessInbox()
		{
			Changed = false;
			client.Fetch(ProcessMessage);
		}

		private void ProcessMessage(Message message)
		{
			string subject = message.Headers.Subject;
			string fromEmail = message.Headers.From.Address;
			string fromDisplayName = message.Headers.From.DisplayName;
			DateTime dateSent = message.Headers.DateSent.ToLocalTime();
			ProcessMessage(fromEmail, fromDisplayName, subject, dateSent);
		}

		private void ProcessMessage(string fromEmail, string fromDisplayName, string subject, DateTime dateSent)
		{
			if (fromDisplayName == "Mail Delivery System") 
			{
				log.Warn("Message from Mail Delivery System ignored");
				return; // битву роботов мы устраивать не хотим :)
			}
			log.Info("receive " + subject + " from: " + fromEmail);
			try
			{
				var parsed = new ParsedSubject(subject);
				if (parsed.RegInfo != null)
				{
					parsed.RegInfo.Email = fromEmail.ToLower(CultureInfo.InvariantCulture);
					parsed.RegInfo.Name = parsed.RegInfo.Name ?? fromDisplayName;
					parsed.RegInfo.Timestamp = dateSent;
					playersRepo.UpdateOrCreate(parsed.RegInfo);
					Changed = true;
				}
				else if (parsed.Answers != null)
				{
					bool notRegisteredPlayer = false;
					var playerInfo = playersRepo.Find(fromEmail);
					if (playerInfo == null)
					{
						playersRepo.UpdateOrCreate(
							playerInfo = new PlayerRegistrationInfo
								{
									Email = fromEmail.ToLower(CultureInfo.InvariantCulture),
									Name = fromDisplayName,
									Timestamp = dateSent,
								});
					}
					TagInfo tag = tagsRepo.Tags.FirstOrDefault(t => t.Tag == parsed.Answers.Tag);
					if (tag == null)
						replier.ReplyWrongTag(fromEmail, parsed.Answers.Tag);
					else if (tag.Finish < dateSent)
						replier.ReplyTooLate(fromEmail, tag);
					else
					{
						parsed.Answers.PlayerEmail = fromEmail.ToLower(CultureInfo.InvariantCulture);
						parsed.Answers.Timestamp = dateSent;
						bool replaced = answersRepo.Add(parsed.Answers);
						replier.ReplyOK(fromEmail, subject, string.Join(", ", parsed.Answers.Answers), replaced);
						Changed = true;
					}
				}
			}
			catch (Exception e)
			{
				replier.ReplyWrongFormat(fromEmail, e.Message);
				throw;
			}
		}
	}
}