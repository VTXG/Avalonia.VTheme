using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Avalonia.VTheme {
    public class VTheme : Styles {
        public VTheme() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
