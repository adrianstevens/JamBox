﻿using Avalonia.Controls;
using JamBox.Core.ViewModels;

namespace JamBox.Core.Services.Interfaces;

public interface INavigationService
{
    void SetMainViewModel(MainViewModel mainViewModel);

    void NavigateTo<TView>() where TView : UserControl;

    void NavigateTo<TView, TViewModel>() where TView : UserControl where TViewModel : class;

    void NavigateBack();
}
