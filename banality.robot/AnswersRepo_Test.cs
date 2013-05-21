using System;
using System.IO;
using NUnit.Framework;

namespace banality.robot
{
	[TestFixture]
	public class AnswersRepo_Test
	{
		[Test]
		public void Test()
		{
			const string filename = "answers";
			if (File.Exists(filename)) File.Delete(filename);
			using(var repo = new AnswersRepo(filename))
			{
				repo.Add(new PlayerAnswers{Answers = new[]{"a", "b c", "d"}, PlayerEmail = "pe@k.ru", Tag = "tag1", Timestamp = new DateTime(2012, 01, 01)});
				repo.Add(new PlayerAnswers { Answers = new[] { "a", "BBBBB", "DDDDD" }, PlayerEmail = "pe@k.ru", Tag = "tag1", Timestamp = new DateTime(2012, 01, 02) });
				repo.Add(new PlayerAnswers { Answers = new[] { "a", "b c", "d" }, PlayerEmail = "pe@k.ru", Tag = "TAG2", Timestamp = new DateTime(2012, 01, 01) });
				repo.Add(new PlayerAnswers { Answers = new[] { "a", "b c", "d" }, PlayerEmail = "eeee@k.ru", Tag = "TAG2", Timestamp = new DateTime(2012, 01, 01) });
			}
			using(var repo = new AnswersRepo(filename))
			{
				Assert.AreEqual(1, repo.GetFor("tag1").Count);
				Assert.AreEqual(2, repo.GetFor("TAG2").Count);
			}
		}
	}
}