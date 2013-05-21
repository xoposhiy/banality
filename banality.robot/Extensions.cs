using System;
using System.Collections.Generic;
using System.Linq;

namespace banality.robot
{
	public static class Extensions
	{
		public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key, TValue defaultValue)
		{
			TValue res;
			if (!d.TryGetValue(key, out res))
				d.Add(key, res = defaultValue);
			return res;
		}
		public static string PrettyFormat(this DateTime time)
		{
			if (time.Date == DateTime.Now.Date)
				return time.ToShortTimeString();
			return time.Date.ToShortDateString();
		}
		
		public static string Shorten(this string longWord)
		{
			if (longWord.Length <= 30) return longWord;
			return longWord.Substring(30) + "...";
		}
		public static IDictionary<TKey, int> CountStatistics<TItem, TKey>(this IEnumerable<TItem> items, Func<TItem, TKey> getGroupingKey)
		{
			return items.GroupBy(getGroupingKey).ToDictionary(g => g.Key, g => g.Count());
		}

	}
}