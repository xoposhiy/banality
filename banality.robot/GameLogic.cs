using System;
using System.Collections.Generic;
using System.Linq;

namespace banality.robot
{
	public class GameLogic
	{
		public GameResult[] Calculate(PlayersRepo playersRepo, AnswersRepo answersRepo, TagsRepo tagsRepo)
		{
			return
				tagsRepo.Tags.Select(
					tag => GetResults(tag, playersRepo, answersRepo.GetFor(tag.Tag))).OrderByDescending(res => res.Tag.Finish).ToArray();
		}

		private GameResult GetResults(TagInfo tag, PlayersRepo playersRepo, Dictionary<string, PlayerAnswers> playerAnswers)
		{
			IDictionary<string, int> wordStats = playerAnswers.SelectMany(ans => ans.Value.Answers).CountStatistics(word => word);
			return
				new GameResult
					{
						Results = playerAnswers.Keys.Select(
							k => new PlayerResult
							     	{
							     		Player = playersRepo.Find(k),
										WordsScores =
							     			playerAnswers[k].Answers.Select(ans => Tuple.Create(ans, wordStats[ans])).ToArray(),
										Score = playerAnswers[k].Answers.Sum(ans => wordStats[ans])
							     	}
							).OrderByDescending(p => p.Score).ToArray(),
						Tag = tag,
						WordsScores =
							wordStats.OrderByDescending(kv => kv.Value).Select(kv => Tuple.Create(kv.Key, kv.Value)).ToArray()
					};
		}
	}
}