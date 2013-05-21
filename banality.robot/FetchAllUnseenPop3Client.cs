using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenPop.Mime;
using OpenPop.Pop3;
using log4net;

namespace banality.robot
{
	class FetchAllUnseenPop3Client
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (FetchAllUnseenPop3Client));
		private readonly string seenUidsFile;
		private readonly Action<Pop3Client> pop3Authorize;
		
		public FetchAllUnseenPop3Client(string seenUidsFile, string serverAddr, int serverPort, string userName, string password)
		{
			this.seenUidsFile = seenUidsFile;
			pop3Authorize = client =>
			                	{
			                		client.Connect(serverAddr, serverPort, false);
			                		client.Authenticate(userName, password);
			                	};
		}

		public void Fetch(Action<Message> process)
		{
			var seenUids = File.Exists(seenUidsFile) ? new HashSet<string>(File.ReadAllLines(seenUidsFile)) : new HashSet<string>();
			using (var client = new Pop3Client())
			{
				pop3Authorize(client);
				List<string> messageUids = client.GetMessageUids();
				foreach (var msgInfo in messageUids.Select((uid, index) => new{uid, index}).Where(msgInfo => !seenUids.Contains(msgInfo.uid)))
				{
					try
					{
						Message message = client.GetMessage(msgInfo.index+1);
						message.Save(new FileInfo(msgInfo.uid + ".mime"));
						process(message);
						seenUids.Add(msgInfo.uid);
					}
					catch(Exception e)
					{
						log.Error("Error while processing message " + msgInfo + "\r\n" + e);
					}
				}
				File.WriteAllLines(seenUidsFile, seenUids);
				for(int i=messageUids.Count; i>=1; i--) client.DeleteMessage(i);
			}

		}
	}
}