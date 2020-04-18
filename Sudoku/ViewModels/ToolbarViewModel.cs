using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Sudoku.ViewModels
{
    public class NumberViewModel : ViewModelBase
    {
        public string Display { get; }
        public int Number { get; }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetValue(ref isSelected, value); }
        }

        public ICommand Command { get; }

        public NumberViewModel(ToolbarViewModel parent, int number, bool isSelected)
        {
            Display = number == 0 ? "Erase" : number.ToString();
            Number = number;
            IsSelected = isSelected;

            Command = new AnonymousCommand(() => parent.CurrentNumber = Number);
        }
    }

    public class ToolbarViewModel : ViewModelBase, ICellActionHandler
    {
        private int currentNumber = 1;
        public int CurrentNumber
        {
            get { return currentNumber; }
            set
            {
                if (value < 0 || value > 9)
                    throw new ArgumentOutOfRangeException(nameof(value), value, $"Argument '{nameof(value)}' is not in range.");

                int previousNumber = currentNumber;

                if (SetValue(ref currentNumber, value))
                {
                    Numbers[previousNumber].IsSelected = false;
                    Numbers[currentNumber].IsSelected = true;
                }
            }
        }

        public ReadOnlyCollection<NumberViewModel> Numbers { get; }

        public ICommand GenerateCommand { get; }
        public ICommand CheckWrongValuesCommand { get; }

        private int minimumAvailableValues = 3;
        public int MinimumAvailableValues
        {
            get { return minimumAvailableValues; }
            set { SetValue(ref minimumAvailableValues, Math.Clamp(value, 1, maximumAvailableValues)); }
        }

        private int maximumAvailableValues = 4;
        public int MaximumAvailableValues
        {
            get { return maximumAvailableValues; }
            set { SetValue(ref maximumAvailableValues, Math.Clamp(value, minimumAvailableValues, 8)); }
        }

        private bool isGameOver;
        public bool IsGameOver
        {
            get { return isGameOver; }
            private set { SetValue(ref isGameOver, value); }
        }

        private readonly RootViewModel root;

        public ToolbarViewModel(RootViewModel root)
        {
            this.root = root;

            NumberViewModel[] numbers = Enumerable
                .Range(0, 10)
                .Select(x => new NumberViewModel(this, x, x == 1))
                .ToArray();

            Numbers = new ReadOnlyCollection<NumberViewModel>(numbers);

            GenerateCommand = new AnonymousCommand(GenerateGrid);
            CheckWrongValuesCommand = new AnonymousCommand(CheckWrongValues);
        }

        public void OnRootInitializationDone()
        {
            root.Grid.GameOverChanged += (object sender, GameOverEventArgs e) => IsGameOver = e.IsGameOver;
        }

        private void GenerateGrid()
        {
            root.Grid.Generate();
        }

        private void CheckWrongValues()
        {
            root.Grid.CheckValidity(true);
        }

        public void OnCellAction(CellViewModel viewModel, CellActionType actionType)
        {
            if (actionType == CellActionType.Primary)
                viewModel.UserValue = CurrentNumber;
            else if (actionType == CellActionType.Secondary && CurrentNumber != 0)
                viewModel.ToogleSuggestion(CurrentNumber);
            else if (actionType == CellActionType.Tertiary)
                viewModel.Erase();
        }
    }
}
