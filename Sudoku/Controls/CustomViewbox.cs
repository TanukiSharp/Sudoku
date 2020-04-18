using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Sudoku.Controls
{
    public class CustomViewbox : Decorator
    {
        public CustomViewbox()
        {
        }

        protected override Size MeasureOverride(Size constraint)
        {
            double min = Math.Min(constraint.Width, constraint.Height);

            var newSize = new Size(min, min);

            Child.Measure(newSize);

            return newSize;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Child.Arrange(new Rect(DesiredSize));

            return DesiredSize;
        }
    }
}
