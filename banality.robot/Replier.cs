using System.Net;
using System.Net.Mail;
using log4net;

namespace banality.robot
{
	internal class Replier
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (Replier));
		private SmtpClient smtpClient;

		public Replier()
		{
			smtpClient = new SmtpClient("smtp.bk.ru", 25);
			smtpClient.Credentials = new NetworkCredential("banality", ",4y4kmyjcnb");
		}

		public void ReplyWrongTag(string to, string tag)
		{
			smtpClient.Send("banality@bk.ru", to, "Банальности — ошибка :-(", "Вы указали в качестве тэга что-то нам не известное: " + tag + "\r\n");
			log.WarnFormat("unknown tag [{0}]", tag);
		}

		public void ReplyWrongFormat(string to, string message)
		{
			smtpClient.Send("banality@bk.ru", to, "Банальности — ошибка :-(", "Вы указали в теме что-то непонятное и у нас случилась страшная ошибка: " + message + "\r\n");
			log.WarnFormat("wrong format. [{0}]", message);
		}

		public void ReplyTooLate(string to, TagInfo tag)
		{
			smtpClient.Send("banality@bk.ru", to, "Банальности — вы опаздали :-(", 
				string.Format("Тема {0} ({1}), на которую вы попытались отправить банальности уже закрыта. Игра закончилась, ответы опубликованы, все разошлись по домам и, уж извините, ваш ответ уже никому не интересен. В следующий раз будьте расторопнее :-)\r\n", tag.Tag, tag.Title));
			log.Warn("Too late");
		}

		public void ReplyOK(string to, string subject, string registeredAnswers, bool answersReplaced)
		{
			var body = string.Format(@"Ваш ответ принят!

{0}
{1}

Ответьте на это письмо с темой:

reg Ваше Имя, Ваша должность, Ваша компания

чтобы изменить способ, которым вы будете отображаться в таблице результатов!",
			 answersReplaced ? "Ваши старые банальности были заменены новыми:" : "Ваши банальности:",
			 registeredAnswers);
			smtpClient.Send("banality@bk.ru", to, subject, body);
			log.Info("answer sent to " + to);
		}
	}
}