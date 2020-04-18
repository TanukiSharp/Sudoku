using System;

namespace Sudoku.ViewModels
{
    public class SquareViewModel : ViewModelBase
    {
        public CellViewModel Cell00 { get; }
        public CellViewModel Cell01 { get; }
        public CellViewModel Cell02 { get; }

        public CellViewModel Cell10 { get; }
        public CellViewModel Cell11 { get; }
        public CellViewModel Cell12 { get; }

        public CellViewModel Cell20 { get; }
        public CellViewModel Cell21 { get; }
        public CellViewModel Cell22 { get; }

        public SquareViewModel(CellViewModel[,] grid, int startX, int startY)
        {
            Cell00 = grid[startX + 0, startY + 0];
            Cell01 = grid[startX + 0, startY + 1];
            Cell02 = grid[startX + 0, startY + 2];

            Cell10 = grid[startX + 1, startY + 0];
            Cell11 = grid[startX + 1, startY + 1];
            Cell12 = grid[startX + 1, startY + 2];

            Cell20 = grid[startX + 2, startY + 0];
            Cell21 = grid[startX + 2, startY + 1];
            Cell22 = grid[startX + 2, startY + 2];
        }
    }
}
