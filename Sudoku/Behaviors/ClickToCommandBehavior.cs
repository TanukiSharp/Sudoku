using System;
using System.Windows;
using System.Windows.Input;

namespace Sudoku.Behaviors
{
    public static class ClickToCommandBehavior
    {
        public static ICommand GetCommand(DependencyObject target)
        {
            return (ICommand)target.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(ClickToCommandBehavior),
            new PropertyMetadata(OnCommandChanged)
        );

        private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                element.PreviewMouseUp += ElementMouseDown;
                element.Unloaded += ElementUnloaded;
            }
        }

        private static void ElementUnloaded(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;

            element.PreviewMouseDown -= ElementMouseDown;
            element.Unloaded -= ElementUnloaded;
        }

        private static void ElementMouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = (FrameworkElement)sender;

            if (e.ChangedButton == MouseButton.Left)
            {
                if (Keyboard.Modifiers == ModifierKeys.None)
                    GetCommand(element).ExecuteIfPossible(CellActionType.Primary);
                else if (Keyboard.Modifiers == ModifierKeys.Shift)
                    GetCommand(element).ExecuteIfPossible(CellActionType.Secondary);
                else if (Keyboard.Modifiers == ModifierKeys.Control)
                    GetCommand(element).ExecuteIfPossible(CellActionType.Tertiary);
            }
            else if (e.ChangedButton == MouseButton.Right)
                GetCommand(element).ExecuteIfPossible(CellActionType.Secondary);
            else if (e.ChangedButton == MouseButton.Middle)
                GetCommand(element).ExecuteIfPossible(CellActionType.Tertiary);
        }
    }
}
