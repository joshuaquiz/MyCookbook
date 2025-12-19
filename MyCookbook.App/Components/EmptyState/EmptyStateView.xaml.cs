using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace MyCookbook.App.Components.EmptyState;

public partial class EmptyStateView : ContentView
{
    public static readonly BindableProperty IconSourceProperty =
        BindableProperty.Create(
            nameof(IconSource),
            typeof(string),
            typeof(EmptyStateView),
            defaultValue: "search");

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(EmptyStateView),
            defaultValue: "No Items Found");

    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(
            nameof(Message),
            typeof(string),
            typeof(EmptyStateView),
            defaultValue: "There are no items to display.");

    public static readonly BindableProperty ActionButtonTextProperty =
        BindableProperty.Create(
            nameof(ActionButtonText),
            typeof(string),
            typeof(EmptyStateView),
            defaultValue: "Retry");

    public static readonly BindableProperty ActionCommandProperty =
        BindableProperty.Create(
            nameof(ActionCommand),
            typeof(ICommand),
            typeof(EmptyStateView),
            defaultValue: null);

    public static readonly BindableProperty ShowActionButtonProperty =
        BindableProperty.Create(
            nameof(ShowActionButton),
            typeof(bool),
            typeof(EmptyStateView),
            defaultValue: false);

    public string IconSource
    {
        get => (string)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string ActionButtonText
    {
        get => (string)GetValue(ActionButtonTextProperty);
        set => SetValue(ActionButtonTextProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public bool ShowActionButton
    {
        get => (bool)GetValue(ShowActionButtonProperty);
        set => SetValue(ShowActionButtonProperty, value);
    }

    public EmptyStateView()
    {
        InitializeComponent();
    }
}

