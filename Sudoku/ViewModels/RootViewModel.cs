using System;

namespace Sudoku.ViewModels
{
    public class RootViewModel: ViewModelBase
    {
        public ToolbarViewModel Toolbar { get; }
        public GridViewModel Grid { get; }

        public RootViewModel()
        {
            Toolbar = new ToolbarViewModel(this);
            Grid = new GridViewModel(this);

            Toolbar.OnRootInitializationDone();
        }
    }
}
