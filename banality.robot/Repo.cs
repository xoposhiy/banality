using System;
using System.IO;
using Newtonsoft.Json;
using log4net;

namespace banality.robot
{
	public class Repo<TItems> : IDisposable where TItems : class, new()
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(PlayersRepo));
		private readonly string filename;
		protected readonly TItems items;
		private readonly DateTime lastModificationTime;

		public Repo(string filename)
		{
			this.filename = filename;
			items = null;
			if (File.Exists(filename))
			{
				items = JsonConvert.DeserializeObject<TItems>(File.ReadAllText(filename));
				lastModificationTime = new FileInfo(filename).LastWriteTimeUtc.ToLocalTime();
			}
			items = items ?? new TItems();
			Changed = false;
		}

		public DateTime LastModificationTime
		{
			get { return lastModificationTime; }
		}

		public bool Changed { get; protected set; }

		public void Dispose()
		{
			if (Changed)
			{
				log.Info("saving changes in " + filename);
				File.WriteAllText(filename, JsonConvert.SerializeObject(items, Formatting.Indented));
			}
		}
	}
}