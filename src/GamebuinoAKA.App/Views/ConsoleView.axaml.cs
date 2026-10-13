using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GamebuinoAKA.App.ViewModels;

namespace GamebuinoAKA.App.Views
{
    public partial class ConsoleView : UserControl
    {
        private ScrollViewer? _scroller;
        private TextBox? _input;
        private ConsoleViewModel? _vm;

        public ConsoleView()
        {
            AvaloniaXamlLoader.Load(this);
            _scroller = this.FindControl<ScrollViewer>("LogScroller");
            _input = this.FindControl<TextBox>("InputBox");
            if (_input != null)
                _input.KeyDown += (_, e) =>
                {
                    if (e.Key == Key.Enter && _vm != null && _vm.SendCommand.CanExecute(null))
                    {
                        _vm.SendCommand.Execute(null);
                        e.Handled = true;
                    }
                };
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object? sender, System.EventArgs e)
        {
            if (_vm != null) _vm.PropertyChanged -= OnVmPropertyChanged;
            _vm = DataContext as ConsoleViewModel;
            if (_vm != null) _vm.PropertyChanged += OnVmPropertyChanged;
        }

        private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_vm is null) return;
            if (e.PropertyName == nameof(ConsoleViewModel.Log) && _vm.AutoScroll)
                Dispatcher.UIThread.Post(() => _scroller?.ScrollToEnd());
        }
    }
}
