using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Sudoku.ViewModels
{
    public interface ICellActionHandler
    {
        void OnCellAction(CellViewModel viewModel, CellActionType actionType);
    }

    public class CellViewModel : ViewModelBase
    {
        private static void EnsureValidCellValue(int value)
        {
            if (value < 1 || value > 9)
                throw new ArgumentOutOfRangeException(nameof(value));
        }

        private int correctValue;
        public int CorrectValue
        {
            get { return correctValue; }
            set
            {
                EnsureValidCellValue(value);
                SetValue(ref correctValue, value);
            }
        }

        private bool isLocked;
        public bool IsLocked
        {
            get { return isLocked; }
            private set { SetValue(ref isLocked, value); }
        }

        private int userValue;
        public int UserValue
        {
            get { return userValue; }
            set
            {
                if (SetValue(ref userValue, value))
                    NotifyPropertyChanged(nameof(IsSet));
            }
        }

        public int CheckValue
        {
            get
            {
                if (IsLocked)
                    return CorrectValue;

                return UserValue;
            }
        }

        private bool isWrong = true;
        public bool IsWrong
        {
            get { return isWrong; }
            set { SetValue(ref isWrong, value); }
        }

        private readonly ObservableCollection<int> suggestions = new ObservableCollection<int>();
        public ReadOnlyObservableCollection<int> Suggestions { get; }

        public CellViewModel(ICellActionHandler actionHandler)
        {
            if (actionHandler == null)
                throw new ArgumentNullException(nameof(actionHandler));

            Suggestions = new ReadOnlyObservableCollection<int>(suggestions);

            ActionCommand = new AnonymousCommand<CellActionType>(actionType => actionHandler.OnCellAction(this, actionType));
        }

        public void Reset(int correctValue, bool isLocked)
        {
            CorrectValue = correctValue;
            UserValue = 0;

            IsLocked = isLocked;

            suggestions.Clear();
        }

        public bool IsSet
        {
            get
            {
                return UserValue != 0;
            }
        }

        public void Erase()
        {
            UserValue = 0;
        }

        public ICommand ActionCommand { get; }

        public void ToogleSuggestion(int value)
        {
            if (IsSet || IsLocked)
                return;

            EnsureValidCellValue(value);

            if (suggestions.Contains(value))
                suggestions.Remove(value);
            else
                InsertSuggestion(value);
        }

        private void InsertSuggestion(int value)
        {
            for (int i = 0; i < suggestions.Count; i++)
            {
                if (suggestions[i] > value)
                {
                    suggestions.Insert(i, value);
                    return;
                }
            }

            // In case value is the largest one.
            suggestions.Add(value);
        }
    }
}
