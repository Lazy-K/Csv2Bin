using CsvHelper;
using System.Collections.Generic;

namespace CsvHelperUtil
{
	public class Util
	{
		public static List<T> Read<T>(CsvReader reader)
		{
			var result = new List<T>();
			if (!reader.Read()) return result;
			T value;
			for (var i = 0; reader.TryGetField<T>(i, out value); ++i)
			{
				result.Add(value);
			}
			return result;
		}
	}
}
