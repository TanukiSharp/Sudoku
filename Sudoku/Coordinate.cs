using System;

namespace Sudoku
{
	public struct Coordinate : IEquatable<Coordinate>
	{
		public int X { get; set; }
		public int Y { get; set; }

		public Coordinate(int x, int y)
		{
			X = x;
			Y = y;
		}

		public Coordinate Rotate()
		{
			int x = X;
			int y = Y;

			if (x == 0)
				x = 2;
			else if (x == 2)
				x = 0;

			if (y == 0)
				y = 2;
			else if (y == 2)
				y = 0;

			return new Coordinate(x, y);
		}

		public Coordinate Translate(int offsetX, int offsetY)
		{
			return new Coordinate(X + offsetX, Y + offsetY);
		}

		public override bool Equals(object obj)
		{
			if (obj is Coordinate coords)
				return Equals(coords);

			return false;
		}

		public bool Equals(Coordinate other)
		{
			return X == other.X && Y == other.Y;
		}

		public override int GetHashCode()
		{
			return $"{X}:{Y}".GetHashCode();
		}

		public static bool operator ==(Coordinate left, Coordinate right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Coordinate left, Coordinate right)
		{
			return !(left == right);
		}
	}
}
