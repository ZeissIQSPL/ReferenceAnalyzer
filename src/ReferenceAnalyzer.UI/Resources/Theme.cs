using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Markup.Xaml;

namespace ReferenceAnalyzer.UI.Resources
{
    public class Theme : Avalonia.Styling.Styles
    {
        public Theme() => AvaloniaXamlLoader.Load(this);
    }
}
