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
			smtpClient.Send("banality@bk.ru", to, "����������� � ������ :-(", "�� ������� � �������� ���� ���-�� ��� �� ���������: " + tag + "\r\n");
			log.WarnFormat("unknown tag [{0}]", tag);
		}

		public void ReplyWrongFormat(string to, string message)
		{
			smtpClient.Send("banality@bk.ru", to, "����������� � ������ :-(", "�� ������� � ���� ���-�� ���������� � � ��� ��������� �������� ������: " + message + "\r\n");
			log.WarnFormat("wrong format. [{0}]", message);
		}

		public void ReplyTooLate(string to, TagInfo tag)
		{
			smtpClient.Send("banality@bk.ru", to, "����������� � �� �������� :-(", 
				string.Format("���� {0} ({1}), �� ������� �� ���������� ��������� ����������� ��� �������. ���� �����������, ������ ������������, ��� ��������� �� ����� �, �� ��������, ��� ����� ��� ������ �� ���������. � ��������� ��� ������ ����������� :-)\r\n", tag.Tag, tag.Title));
			log.Warn("Too late");
		}

		public void ReplyOK(string to, string subject, string registeredAnswers, bool answersReplaced)
		{
			var body = string.Format(@"��� ����� ������!

{0}
{1}

�������� �� ��� ������ � �����:

reg ���� ���, ���� ���������, ���� ��������

����� �������� ������, ������� �� ������ ������������ � ������� �����������!",
			 answersReplaced ? "���� ������ ����������� ���� �������� ������:" : "���� �����������:",
			 registeredAnswers);
			smtpClient.Send("banality@bk.ru", to, subject, body);
			log.Info("answer sent to " + to);
		}
	}
}