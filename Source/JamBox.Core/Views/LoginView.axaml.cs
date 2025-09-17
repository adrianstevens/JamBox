using Avalonia.Controls;
using JamBox.Core.ViewModels;

namespace JamBox.Core.Views;

public partial class LoginView : UserControl
{
    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}