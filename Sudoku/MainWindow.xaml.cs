using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Sudoku.ViewModels;

namespace Sudoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RootViewModel rootViewModel = new RootViewModel();

        public MainWindow()
        {
            InitializeComponent();

            string version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            Title = $"Sudoku - v{version}";

            DataContext = rootViewModel;
        }

        private bool IsNumericKey(Key key)
        {
            int numKey = (int)key;

            return
                (numKey >= (int)Key.D0 && numKey <= (int)Key.D9) ||
                (numKey >= (int)Key.NumPad0 && numKey <= (int)Key.NumPad9);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (IsNumericKey(e.Key))
            {
                string strNum = Enum.GetName(typeof(Key), e.Key);
                int num = int.Parse(strNum.Substring(strNum.Length - 1));
                rootViewModel.Toolbar.CurrentNumber = num;
            }
        }
    }
}
