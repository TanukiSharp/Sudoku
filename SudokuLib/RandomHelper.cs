using System;

namespace SudokuLib
{
	public static class RandomHelper
	{
		private static readonly Random random = new Random(Guid.NewGuid().GetHashCode());

		public static int Next(int value)
		{
			return random.Next(value);
		}

		public static int Next(int min, int max)
		{
			return random.Next(min, max);
		}

		public static byte NextByte(int max)
		{
			return (byte)random.Next(max);
		}

		public static byte NextCellValue()
		{
			return (byte)(random.Next(9) + 1);
		}
	}
}
