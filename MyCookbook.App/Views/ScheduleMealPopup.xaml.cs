using System;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;

namespace MyCookbook.App.Views;

public partial class ScheduleMealPopup : Popup<ScheduleMealResult?>
{
    private readonly string _recipeName;
    private readonly Guid _recipeGuid;
    private DateTime? _selectedDate;
    private string? _selectedMealType;

    public ScheduleMealPopup(string recipeName, Guid recipeGuid)
    {
        InitializeComponent();
        _recipeName = recipeName;
        _recipeGuid = recipeGuid;

        // Set the question label with the recipe name
        QuestionLabel.Text = $"When would you like to have {recipeName}?";

        // Initialize with today's date
        _selectedDate = DateTime.Now.Date;
    }

    private void DatePicker_DateSelected(object? sender, DateChangedEventArgs e)
    {
        _selectedDate = e.NewDate;
        ValidateForm();
    }

    private void MealTypePicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (MealTypePicker.SelectedIndex >= 0)
        {
            _selectedMealType = MealTypePicker.Items[MealTypePicker.SelectedIndex];
        }
        else
        {
            _selectedMealType = null;
        }
        ValidateForm();
    }

    private void ValidateForm()
    {
        // Enable save button only if both date and meal type are selected
        SaveButton.IsEnabled = _selectedDate.HasValue && !string.IsNullOrEmpty(_selectedMealType);
    }

    private async void CloseButton_Clicked(object? sender, EventArgs e)
    {
        await CloseAsync(null);
    }

    private async void CancelButton_Clicked(object? sender, EventArgs e)
    {
        await CloseAsync(null);
    }

    private async void SaveButton_Clicked(object? sender, EventArgs e)
    {
        if (_selectedDate.HasValue && !string.IsNullOrEmpty(_selectedMealType))
        {
            // Return the selected values to the caller
            await CloseAsync(new ScheduleMealResult(_recipeGuid, _selectedDate.Value, _selectedMealType));
        }
    }
}

public record ScheduleMealResult(Guid RecipeGuid, DateTime Date, string MealType);

