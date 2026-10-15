// Copyright (c) Gamebuino AKA. Tous droits réservés.
// Licence : GPL-3.0-or-later

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GamebuinoAKA.App.Views
{
    public partial class BlocklyView : UserControl
    {
        public BlocklyView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
