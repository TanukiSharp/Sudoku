using SudokuLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Sudoku.ViewModels
{
	public interface ICompletelySetNumber
	{
		int Number { get; }
		bool IsCompletelySet { get; }
	}

	public class GameOverEventArgs : EventArgs
	{
		public bool IsGameOver { get; }

		public GameOverEventArgs(bool isGameOver)
		{
			IsGameOver = isGameOver;
		}
	}

    public class GridViewModel : ViewModelBase
	{
		private Stopwatch gameDuration;

		private readonly SudokuGrid sudokuGrid = new SudokuGrid();
		private readonly CellViewModel[,] grid = new CellViewModel[9, 9];

		public SquareViewModel Square00 { get; }
		public SquareViewModel Square01 { get; }
		public SquareViewModel Square02 { get; }

		public SquareViewModel Square10 { get; }
		public SquareViewModel Square11 { get; }
		public SquareViewModel Square12 { get; }

		public SquareViewModel Square20 { get; }
		public SquareViewModel Square21 { get; }
		public SquareViewModel Square22 { get; }

		private readonly CompletelySetNumberViewModel[] completelySetNumbers;
		public ReadOnlyCollection<ICompletelySetNumber> CompletelySetNumbers { get; }

		public event EventHandler<GameOverEventArgs> GameOverChanged;

		private bool isGameOver;
		public bool IsGameOver
		{
			get { return isGameOver; }
			private set
			{
				if (isGameOver != value)
				{
					isGameOver = value;
					GameOverChanged?.Invoke(this, new GameOverEventArgs(value));
				}
			}
		}

		private class CheckGridCellActionHandler : ICellActionHandler
		{
			private readonly RootViewModel root;
			private readonly GridViewModel parent;

			public CheckGridCellActionHandler(RootViewModel root, GridViewModel parent)
			{
				this.root = root;
				this.parent = parent;
			}

			public void OnCellAction(CellViewModel viewModel, CellActionType actionType)
			{
				if (parent.IsGameOver)
					return;

				root.Toolbar.OnCellAction(viewModel, actionType);
				parent.OnCellAction(actionType);
			}
		}

		private class CompletelySetNumberViewModel : ViewModelBase, ICompletelySetNumber
		{
			public int Number { get; }

			private bool isCompletelySet;
			public bool IsCompletelySet
			{
				get { return isCompletelySet; }
				set { SetValue(ref isCompletelySet, value); }
			}

			public CompletelySetNumberViewModel(int number)
			{
				Number = number;
			}
		}

		private readonly RootViewModel root;

		public GridViewModel(RootViewModel root)
		{
			this.root = root;

			for (int y = 0; y < 9; y++)
			{
				for (int x = 0; x < 9; x++)
					grid[x, y] = new CellViewModel(new CheckGridCellActionHandler(root, this));
			}

			completelySetNumbers = Enumerable
				.Range(1, 9)
				.Select(x => new CompletelySetNumberViewModel(x))
				.ToArray();

			CompletelySetNumbers = new ReadOnlyCollection<ICompletelySetNumber>(completelySetNumbers);

			Square00 = new SquareViewModel(grid, 0, 0);
			Square01 = new SquareViewModel(grid, 0, 3);
			Square02 = new SquareViewModel(grid, 0, 6);

			Square10 = new SquareViewModel(grid, 3, 0);
			Square11 = new SquareViewModel(grid, 3, 3);
			Square12 = new SquareViewModel(grid, 3, 6);

			Square20 = new SquareViewModel(grid, 6, 0);
			Square21 = new SquareViewModel(grid, 6, 3);
			Square22 = new SquareViewModel(grid, 6, 6);

			Generate();
		}

		private HashSet<Coordinate> GenerateLockedCellCoordinates()
		{
			int min = root.Toolbar.MinimumAvailableValues;
			int max = root.Toolbar.MaximumAvailableValues;

			var coordinates = new HashSet<Coordinate>();

			GetRandomCoordinates(coordinates, 0, 0, 6, 6, min, max);
			GetRandomCoordinates(coordinates, 3, 0, 3, 6, min, max);
			GetRandomCoordinates(coordinates, 6, 0, 0, 6, min, max);
			GetRandomCoordinates(coordinates, 0, 3, 6, 3, min, max);

			GetRandomCoordinates(coordinates, 3, 3, 3, 3, min, max, true);

			return coordinates;
		}

		public void Generate()
		{
			sudokuGrid.Generate();

			HashSet<Coordinate> lockedCoordinates = GenerateLockedCellCoordinates();

			for (int y = 0; y < 9; y++)
			{
				for (int x = 0; x < 9; x++)
				{
					bool isLocked = lockedCoordinates.Contains(new Coordinate(x, y));
					grid[x, y].Reset(sudokuGrid.GetValueAt(x, y), isLocked);
				}
			}

			IsGameOver = false;

			gameDuration = Stopwatch.StartNew();

			OnCellAction(CellActionType.Primary);
		}

		private void OnCellAction(CellActionType actionType)
		{
			if (IsGameOver)
				return;

			UpdateCompletelySetNumbers();

			ResetWrongValues();

			if (actionType == CellActionType.Primary && CheckValidity(false))
			{
				gameDuration.Stop();

				IsGameOver = true;

				System.Windows.MessageBox.Show($"You win!\nGame duration: {GetTimeString(gameDuration.Elapsed)}");
			}
		}

		private void GetRandomCoordinates(HashSet<Coordinate> output, int offsetX, int offsetY, int dualOffsetX, int dualOffsetY, int inclusiveMin, int inclusiveMax, bool isCenter = false)
		{
			if (inclusiveMin > inclusiveMax)
				throw new ArgumentOutOfRangeException(nameof(inclusiveMin), inclusiveMin, $"Argument '{nameof(inclusiveMin)}' ({inclusiveMin}) must not be greater than '{nameof(inclusiveMax)}' ({inclusiveMax}).");

			int number;

			if (inclusiveMin != inclusiveMax)
				number = RandomHelper.Next(inclusiveMin, inclusiveMax + 1);
			else
				number = inclusiveMax;

			if (isCenter)
				number = (int)Math.Ceiling(number / 2.0);

			for (int i = 0; i < number; i++)
			{
				while (true)
				{
					var baseCoordinate = new Coordinate(RandomHelper.Next(3), RandomHelper.Next(3));

					if (output.Add(baseCoordinate.Translate(offsetX, offsetY)))
					{
						output.Add(baseCoordinate.Rotate().Translate(dualOffsetX, dualOffsetY));
						break;
					}
				}
			}
		}

		public void ResetWrongValues()
		{
			for (int y = 0; y < 9; y++)
			{
				for (int x = 0; x < 9; x++)
					grid[x, y].IsWrong = false;
			}
		}

		public bool CheckValidity(bool showWrongValues)
		{
			bool result = true;

			for (int y = 0; y < 9; y++)
			{
				for (int x = 0; x < 9; x++)
				{
					if (grid[x, y].CheckValue == 0)
					{
						if (showWrongValues)
							SetEmptyWrong();

						return false;
					}
				}
			}

			for (int x = 0; x < 9; x++)
			{
				if (IsColumnOK(x) == false)
				{
					if (showWrongValues == false)
						return false;

					SetColumnWrong(x);
					result = false;
				}
			}

			for (int y = 0; y < 9; y++)
			{
				if (IsLineOK(y) == false)
				{
					if (showWrongValues == false)
						return false;

					SetLineWrong(y);
					result = false;
				}
			}

			for (int y = 0; y < 9; y += 3)
			{
				for (int x = 0; x < 9; x += 3)
				{
					if (IsSquareOK(x, y) == false)
					{
						if (showWrongValues == false)
							return false;

						SetSquareWrong(x, y);
						result = false;
					}
				}
			}

			return result;
		}

		public static string GetTimeString(TimeSpan ts)
		{
			return $"{ts.Minutes} min {ts.Seconds} sec {ts.Milliseconds} ms";
		}

		private bool IsSquareOK(int minX, int minY)
		{
			for (int y = minY; y < minY + 3; y++)
			{
				for (int x = minX; x < minX + 3; x++)
				{
					byte val = (byte)grid[x, y].CheckValue;

					if (val == 0)
						return false;

					int total = 0;

					for (int yy = minY; yy < minY + 3; yy++)
					{
						for (int xx = minX; xx < minX + 3; xx++)
						{
							if (grid[xx, yy].CheckValue == 0xFF)
								return false;

							if (grid[xx, yy].CheckValue == val)
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
				byte val = (byte)grid[x, y].CheckValue;

				if (val == 0)
					return false;

				int total = 0;

				for (int yy = 0; yy < 9; yy++)
				{
					if (grid[x, yy].CheckValue == val)
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
				byte val = (byte)grid[x, y].CheckValue;

				if (val == 0)
					return false;

				int total = 0;

				for (int xx = 0; xx < 9; xx++)
				{
					if (grid[xx, y].CheckValue == 0xFF)
						return false;

					if (grid[xx, y].CheckValue == val)
						total++;
				}

				if (total > 1)
					return false;
			}

			return true;
		}

		private void SetEmptyWrong()
		{
			for (int y = 0; y < 9; y++)
			{
				for (int x = 0; x < 9; x++)
				{
					if (grid[x, y].CheckValue == 0)
						grid[x, y].IsWrong = true;
				}
			}
		}

		private void SetSquareWrong(int minX, int minY)
		{
			for (int y = minY; y < minY + 3; y++)
			{
				for (int x = minX; x < minX + 3; x++)
					grid[x, y].IsWrong = true;
			}
		}

		private void SetColumnWrong(int x)
		{
			for (int y = 0; y < 9; y++)
				grid[x, y].IsWrong = true;
		}

		private void SetLineWrong(int y)
		{
			for (int x = 0; x < 9; x++)
				grid[x, y].IsWrong = true;
		}

		public void UpdateCompletelySetNumbers()
		{
			foreach (CompletelySetNumberViewModel completelySetNumber in completelySetNumbers)
				completelySetNumber.IsCompletelySet = IsNumberCompletelySet(completelySetNumber.Number);
		}

		private bool IsNumberCompletelySet(int value)
		{
			int total = 0;

			for (int y = 0; y < 9; y++)
			{
				for (int x = 0; x < 9; x++)
				{
					if (grid[x, y].CheckValue == value)
						total++;
				}
			}

			return total >= 9;
		}
	}
}
