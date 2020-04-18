using System;

namespace SudokuLib
{
	public class SudokuGrid
	{
		// Pre-allocated reusable buffers for allocation-free implementation.
		private static readonly byte[] multiPurposeBuffer = new byte[9];
		private static readonly int[] weightsBuffer = new int[9];

		public const byte EmptyValue = 0xFF;
		private const byte CellOverflowValue = 10;

		private readonly byte[,] grid;
		private readonly bool[,,] available;

		public SudokuGrid()
		{
			grid = new byte[9, 9];
			available = new bool[9, 9, 9];
		}

		public byte GetValueAt(int x, int y)
		{
			if (x < 0 || x > 8)
				throw new ArgumentOutOfRangeException(nameof(x), $"Argument '{nameof(x)}' must be in range [0; 8].");
			if (y < 0 || y > 8)
				throw new ArgumentOutOfRangeException(nameof(y), $"Argument '{nameof(y)}' must be in range [0; 8].");

			return grid[x, y];
		}

		public void Populate(byte[,] grid)
		{
			if (grid == null)
				throw new ArgumentNullException(nameof(grid));

			if (grid.GetLength(0) != 9)
				throw new ArgumentException($"Dimension 0 of argument '{nameof(grid)}' must be 9.", nameof(grid));
			if (grid.GetLength(1) != 9)
				throw new ArgumentException($"Dimension 1 of argument '{nameof(grid)}' must be 9.", nameof(grid));

			CopyGrid(grid);
			ResetAvailable();
		}

		public void Generate()
		{
			while (true)
			{
				ResetGrid();
				ResetAvailable();

				FulFillSquareRandomly(0, 0);
				FulFillSquareRandomly(3, 3);
				FulFillSquareRandomly(6, 6);

				if (Resolve())
					return;
			}
		}

		public bool Resolve()
		{
			UpdateUnavailability();

			bool? result;

			while ((result = FulfillSquareIntelligently()) != null)
			{
				if (result == false)
					return false;
			}

			return true;
		}

		private void FulFillSquareRandomly(int startX, int startY)
		{
			FillRandomLine(multiPurposeBuffer);

			int k = 0;

			for (int y = startY; y < startY + 3; y++)
			{
				for (int x = startX; x < startX + 3; x++)
					grid[x, y] = multiPurposeBuffer[k++];
			}
		}

		private bool? FulfillSquareIntelligently()
		{
			if (FindLeastAvailableCellValues(out int emptiestX, out int emptiestY, multiPurposeBuffer, out int valueCount) == false)
				return null;

			int startX = AlignToSquareStart(emptiestX);
			int startY = AlignToSquareStart(emptiestY);

			byte val = FindLeastAvailableCellValue(startX, startY, multiPurposeBuffer, valueCount);

			if (val == CellOverflowValue)
				return false;

			grid[emptiestX, emptiestY] = (byte)(val + 1);

			available[emptiestX, emptiestY, val] = false;
			UpdateUnavailability(emptiestX, emptiestY, val);

			return true;
		}

		private bool FindLeastAvailableCell(out int emptiestX, out int emptiestY)
		{
			int min = CellOverflowValue;

			emptiestX = -1;
			emptiestY = -1;

			for (int yy = 0; yy < 9; yy++)
			{
				for (int xx = 0; xx < 9; xx++)
				{
					if (grid[xx, yy] != EmptyValue)
						continue;

					int availableCount = FindAvailableCount(xx, yy);

					if (availableCount < min)
					{
						min = availableCount;
						emptiestX = xx;
						emptiestY = yy;
					}
				}
			}

			return emptiestX != -1 && emptiestY != -1;
		}

		private bool FindLeastAvailableCellValues(out int emptiestX, out int emptiestY, byte[] values, out int valueCount)
		{
			if (FindLeastAvailableCell(out emptiestX, out emptiestY) == false)
			{
				emptiestX = -1;
				emptiestY = -1;
				valueCount = 0;

				return false;
			}

			Array.Clear(values, 0, values.Length);

			valueCount = FindAvailableCount(emptiestX, emptiestY);

			int k = 0;

			for (byte i = 0; i < 9; i++)
			{
				if (available[emptiestX, emptiestY, i])
					values[k++] = i;
			}

			return true;
		}

		private byte FindLeastAvailableCellValue(int startX, int startY, byte[] values, int valueCount)
		{
			int index;
			Array.Clear(weightsBuffer, 0, weightsBuffer.Length);

			for (int w = 0; w < valueCount; w++)
			{
				index = values[w];

				for (int y = startY; y < startY + 3; y++)
				{
					for (int x = startX; x < startX + 3; x++)
					{
						if (available[x, y, index])
							weightsBuffer[w]++;
					}
				}
			}

			index = -1;
			int min = CellOverflowValue;

			for (int w = 0; w < valueCount; w++)
			{
				if (weightsBuffer[w] < min)
				{
					min = weightsBuffer[w];
					index = w;
				}
			}

			if (index < 0)
				return CellOverflowValue;

			return values[index];
		}

		private int FindAvailableCount(int x, int y)
		{
			int total = 0;

			for (int i = 0; i < 9; i++)
			{
				if (available[x, y, i])
					total++;
			}

			return total;
		}

		private void UpdateUnavailability()
		{
			for (int y = 0; y < 9; y++)
			{
				for (int x = 0; x < 9; x++)
				{
					byte val = grid[x, y];

					if (val != EmptyValue)
						UpdateUnavailability(x, y, (byte)(val - 1));
				}
			}
		}

		private static int AlignToSquareStart(int index)
		{
			return index / 3 * 3;
		}

		private void UpdateUnavailability(int x, int y, byte val)
		{
			for (int xx = 0; xx < 9; xx++)
				available[xx, y, val] = false;

			for (int yy = 0; yy < 9; yy++)
				available[x, yy, val] = false;

			int startX = AlignToSquareStart(x);
			int startY = AlignToSquareStart(y);

			for (int yy = startY; yy < startY + 3; yy++)
			{
				for (int xx = startX; xx < startX + 3; xx++)
					available[xx, yy, val] = false;
			}
		}

		private void FillRandomLine(byte[] output)
		{
			void Swap(byte[] array, int a, int b)
			{
				byte temp = array[a];
				array[a] = array[b];
				array[b] = temp;
			}

			for (int i = 0; i < 9; i++)
				output[i] = (byte)(i + 1);

			for (int i = 0; i < 8; i++)
			{
				int j = i + RandomHelper.NextByte(9 - i);
				Swap(output, i, j);
			}
		}

		private void CopyGrid(byte[,] grid)
		{
			for (int y = 0; y < 9; y++)
			{
				for (int x = 0; x < 9; x++)
				{
					byte input = grid[x, y];

					if (input >= 1 && input <= 9 || input == EmptyValue)
						this.grid[x, y] = input;
					else
						throw new ArgumentException($"Argument '{nameof(grid)}' contains illegal value at position ({x}, {y}) [{input}]. Expected value in rage [1; 9] or {EmptyValue} for empty cell.");
				}
			}
		}

		private void ResetGrid()
		{
			for (int i = 0; i < 9; i++)
			{
				for (int j = 0; j < 9; j++)
					grid[i, j] = EmptyValue;
			}
		}

		private void ResetAvailable()
		{
			for (int i = 0; i < 9; i++)
			{
				for (int j = 0; j < 9; j++)
				{
					for (int k = 0; k < 9; k++)
						available[i, j, k] = true;
				}
			}
		}

		public bool Check()
		{
			for (int x = 0; x < 9; x++)
			{
				if (IsColumnOK(x) == false)
					return false;
			}

			for (int y = 0; y < 9; y++)
			{
				if (IsLineOK(y) == false)
					return false;
			}

			for (int y = 0; y < 9; y += 3)
			{
				for (int x = 0; x < 9; x += 3)
				{
					if (IsSquareOK(x, y) == false)
						return false;
				}
			}

			return true;
		}

		private bool IsSquareOK(int xmin, int ymin)
		{
			for (int y = ymin; y < ymin + 3; y++)
			{
				for (int x = xmin; x < xmin + 3; x++)
				{
					int total = 0;
					byte val = grid[x, y];

					for (int yy = ymin; yy < ymin + 3; yy++)
					{
						for (int xx = xmin; xx < xmin + 3; xx++)
						{
							if (grid[xx, yy] == val)
								total++;
						}
					}

					if (total > 1)
						return false;
				}
			}

			return true;
		}

		private bool IsColumnOK(int x)
		{
			for (int y = 0; y < 9; y++)
			{
				int total = 0;
				byte val = grid[x, y];

				for (int yy = 0; yy < 9; yy++)
				{
					if (grid[x, yy] == val)
						total++;
				}

				if (total > 1)
					return false;
			}

			return true;
		}

		private bool IsLineOK(int y)
		{
			for (int x = 0; x < 9; x++)
			{
				int total = 0;
				byte val = grid[x, y];

				for (int xx = 0; xx < 9; xx++)
				{
					if (grid[xx, y] == val)
						total++;
				}

				if (total > 1)
					return false;
			}

			return true;
		}
	}
}
