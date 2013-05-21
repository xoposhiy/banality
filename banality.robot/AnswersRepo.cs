using System.Collections.Generic;
using log4net;

namespace banality.robot
{
	public class AnswersRepo : Repo<Dictionary<string, Dictionary<string, PlayerAnswers>>>
	{
		public AnswersRepo(string filename)
			: base(filename)
		{
		}

		public bool Add(PlayerAnswers playerAnswers)
		{
			Changed = true;
			Dictionary<string, PlayerAnswers> ansByTag = items.GetOrCreate(playerAnswers.Tag, new Dictionary<string, PlayerAnswers>());
			PlayerAnswers ans = ansByTag.GetOrCreate(playerAnswers.PlayerEmail, playerAnswers);
			if (ans.Timestamp < playerAnswers.Timestamp)
			{
				ansByTag[playerAnswers.PlayerEmail] = playerAnswers;
				return true;
			}
			return false;
		}

		public Dictionary<string, PlayerAnswers> GetFor(string tag)
		{
			return items.GetOrCreate(tag, new Dictionary<string, PlayerAnswers>());
		}
	}

	public class PlayersRepo : Repo<Dictionary<string, PlayerRegistrationInfo>>
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (PlayersRepo));
		public PlayersRepo(string filename)
			: base(filename)
		{
		}

		public void UpdateOrCreate(PlayerRegistrationInfo playerInfo)
		{
			Changed = true;
			if (items.ContainsKey(playerInfo.Email))
			{
				items[playerInfo.Email] = playerInfo;
				log.Info("Update player info: " + playerInfo);
			}
			else
			{
				items.Add(playerInfo.Email, playerInfo);
				log.Info("New player: " + playerInfo);
			}
			
		}

		public PlayerRegistrationInfo Find(string email)
		{
			return items.ContainsKey(email) ? items[email] : null;
		}
	}
}