using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using RazorEngine;
using System.Linq;

namespace banality.robot
{
	internal class HtmlGenerator
	{
		private readonly GameResult[] results;

		public HtmlGenerator(GameResult[] results)
		{
			this.results = results;
		}

		public void Regenerate(string path)
		{
			if (!File.Exists(path))
				Directory.CreateDirectory(path);
			File.WriteAllText(path + "\\index.html", GenerateIndex());
			foreach (var res in results)
				if (res.Tag.Start <= DateTime.Now)
				{
					if (res.Tag.Finish > DateTime.Now)
						File.WriteAllText(path + "\\" + res.Tag.Tag + ".htm", GenerateOneResult(res));
					else
						File.WriteAllText(path + "\\" + res.Tag.Tag + ".html", GenerateOneResult(res));
				}
		}

		private string GenerateIndex()
		{
			var finishedGames = results.Where(res => res.Tag.Finish <= DateTime.Now).OrderByDescending(res => res.Tag.Finish).ToArray();
			var activeGame = results.SingleOrDefault(res => res.IsActive);
			var nextGame = results.Where(res => res.Tag.Start > DateTime.Now).OrderBy(res => res.Tag.Start).FirstOrDefault();
			return Razor.Parse(File.ReadAllText("index.cshtml"), 
				new GameModel
					{
						Current = activeGame == null ? null : activeGame.Tag, 
						Next = nextGame == null ? null : nextGame.Tag, 
						FinishedGames = finishedGames
					});
		}

		private string GenerateOneResult(GameResult result)
		{
			return Razor.Parse(File.ReadAllText("tag.cshtml"), result);
		}
	}

	[TestFixture]
	public class HtmlGenerator_Test
	{
		[Test]
		public void Test()
		{
			using (var playersRepo = new PlayersRepo("players"))
			using (var answersRepo = new AnswersRepo("answers"))
			using (var tagsRepo = new TagsRepo("tags"))
			{
				File.WriteAllText("index.html",
				                  Razor.Parse(File.ReadAllText("index.cshtml"),
				                              new GameLogic().Calculate(playersRepo, answersRepo, tagsRepo)
				                  	));
			}
			Process.Start("index.html");
		}
	}

	public class GameModel
	{
		public string CurrentTag
		{
			get { return Current == null ? "" : Current.Tag; }
		}
		public TagInfo Current { get; set; }
		public TagInfo Next { get; set; }
		public GameResult[] FinishedGames { get; set; }	
	}
	
	public class GameResult
	{
		public bool IsActive { get { return Tag.Start <= DateTime.Now && Tag.Finish >= DateTime.Now; } }
		public bool ResultsAreVisible { get { return !IsActive || Tag.Tag == "banality"; } }
		public PlayerResult[] Results;
		public Tuple<string, int>[] WordsScores;
		public TagInfo Tag { get; set; }
		public bool IsNext
		{
			get { return Tag.Start > DateTime.Now; }
		}

		public PlayerResult Winner
		{
			get { 
				return Results.OrderByDescending(r => r.Score).FirstOrDefault();
				
			}
		}
	}

	public class PlayerResult
	{
		public PlayerRegistrationInfo Player;
		public int Score;
		public Tuple<string, int>[] WordsScores;
	}
}