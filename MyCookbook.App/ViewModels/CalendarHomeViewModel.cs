using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyCookbook.Common.ApiModels;
using XCalendar.Core.Models;

namespace MyCookbook.App.ViewModels;

public enum CalendarViewMode
{
    Month,
    Week
}

public partial class CalendarHomeViewModel : BaseViewModel, IDisposable
{
    private readonly HttpClient _httpClient;
    private bool _disposed;
    public Guid CurrentAuthorId { get; private set; }

    [ObservableProperty]
    private Calendar<CalendarDay> _myCalendar = new();

    [ObservableProperty]
    private DateTime? _selectedDate;

    [ObservableProperty]
    private ObservableCollection<UserCalendarEntryModel> _allCalendarEntries = [];

    [ObservableProperty]
    private ObservableCollection<CalendarMealGroup> _selectedDayMeals = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ViewModeToggleText))]
    private CalendarViewMode _viewMode = CalendarViewMode.Month;

    [ObservableProperty]
    private string _headerText = string.Empty;

    public string ViewModeToggleText => ViewMode == CalendarViewMode.Month ? "Week" : "Month";

    partial void OnViewModeChanged(CalendarViewMode value)
    {
        UpdateHeaderText();
    }

    partial void OnSelectedDateChanged(DateTime? value)
    {
        UpdateHeaderText();
        UpdateSelectedDayMeals();
    }

    private void UpdateSelectedDayMeals()
    {
        if (!SelectedDate.HasValue)
        {
            SelectedDayMeals.Clear();
            return;
        }

        var mealsForDay = AllCalendarEntries
            .Where(x => x.Date.Date == SelectedDate.Value.Date)
            .OrderBy(x => x.MealType)
            .ToList();

        // Group by meal type
        var grouped = mealsForDay
            .GroupBy(x => new { x.MealType, x.MealTypeName })
            .Select(g => new CalendarMealGroup
            {
                MealType = g.Key.MealType,
                MealTypeName = g.Key.MealTypeName,
                Recipes = new ObservableCollection<UserCalendarEntryModel>(g)
            })
            .OrderBy(x => x.MealType)
            .ToList();

        SelectedDayMeals = new ObservableCollection<CalendarMealGroup>(grouped);
    }

    public CalendarHomeViewModel(HttpClient httpClient)
    {
        _httpClient = httpClient;
        SelectedDate = DateTime.Today;
        UpdateHeaderText();

        // Subscribe to calendar date selection changes
        MyCalendar.DateSelectionChanged += OnDateSelectionChanged;
    }

    private void OnDateSelectionChanged(object? sender, DateSelectionChangedEventArgs args)
    {
        // Based on the original implementation from CalendarHome.xaml.cs
        var oldStart = args.PreviousSelection.FirstOrDefault().ToString("D");
        var oldEnd = args.PreviousSelection.FirstOrDefault().ToString("D");
        var newStart = args.PreviousSelection.FirstOrDefault().ToString("D");
        var newEnd = args.PreviousSelection.FirstOrDefault().ToString("D");

        var oldDate = string.IsNullOrWhiteSpace(oldEnd)
            ? oldStart + "-" + oldEnd
            : oldStart;
        var newDate = string.IsNullOrWhiteSpace(newEnd)
            ? newStart + "-" + newEnd
            : newStart;

        // Update selected date based on the calendar's current selection
        if (MyCalendar.SelectedDates.Count > 0)
        {
            SelectedDate = MyCalendar.SelectedDates.First();
        }
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            // Get current user
            var userJson = await Microsoft.Maui.Storage.SecureStorage.GetAsync("UserProfile");
            if (!string.IsNullOrEmpty(userJson))
            {
                var user = JsonSerializer.Deserialize<UserProfileModel?>(userJson);
                if (user.HasValue)
                {
                    CurrentAuthorId = user.Value.Guid;
                    await LoadCalendarEntriesAsync();
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task LoadCalendarEntriesAsync()
    {
        try
        {
            // MOCK DATA - Replace with actual API call later
            await Task.Delay(100); // Simulate API call

            var mockEntries = new List<UserCalendarEntryModel>
            {
                new(
                    Id: Guid.NewGuid(),
                    AuthorId: CurrentAuthorId,
                    RecipeId: Guid.NewGuid(),
                    RecipeName: "Spaghetti Carbonara",
                    RecipeImageUrl: new Uri("https://picsum.photos/200/200?random=1"),
                    Date: DateTime.Today,
                    MealType: 1,
                    MealTypeName: "Lunch",
                    ServingsMultiplier: 2.0m),
                new(
                    Id: Guid.NewGuid(),
                    AuthorId: CurrentAuthorId,
                    RecipeId: Guid.NewGuid(),
                    RecipeName: "Grilled Chicken Salad",
                    RecipeImageUrl: new Uri("https://picsum.photos/200/200?random=2"),
                    Date: DateTime.Today,
                    MealType: 2,
                    MealTypeName: "Dinner",
                    ServingsMultiplier: 1.5m),
                new(
                    Id: Guid.NewGuid(),
                    AuthorId: CurrentAuthorId,
                    RecipeId: Guid.NewGuid(),
                    RecipeName: "Pancakes with Maple Syrup",
                    RecipeImageUrl: new Uri("https://picsum.photos/200/200?random=3"),
                    Date: DateTime.Today.AddDays(1),
                    MealType: 0,
                    MealTypeName: "Breakfast",
                    ServingsMultiplier: 1.0m),
                new(
                    Id: Guid.NewGuid(),
                    AuthorId: CurrentAuthorId,
                    RecipeId: Guid.NewGuid(),
                    RecipeName: "Beef Tacos",
                    RecipeImageUrl: new Uri("https://picsum.photos/200/200?random=4"),
                    Date: DateTime.Today.AddDays(2),
                    MealType: 2,
                    MealTypeName: "Dinner",
                    ServingsMultiplier: 2.0m),
                new(
                    Id: Guid.NewGuid(),
                    AuthorId: CurrentAuthorId,
                    RecipeId: Guid.NewGuid(),
                    RecipeName: "Caesar Salad",
                    RecipeImageUrl: new Uri("https://picsum.photos/200/200?random=5"),
                    Date: DateTime.Today.AddDays(-1),
                    MealType: 1,
                    MealTypeName: "Lunch",
                    ServingsMultiplier: 1.0m)
            };

            AllCalendarEntries = new ObservableCollection<UserCalendarEntryModel>(mockEntries);

            // Uncomment below for actual API call
            // var startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            // var endDate = startDate.AddMonths(1).AddDays(-1);
            // var url = $"/api/Calendar/Entries?AuthorId={CurrentAuthorId}&startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            // var entries = await _httpClient.GetFromJsonAsync<List<UserCalendarEntryModel>>(url);
            // if (entries != null)
            // {
            //     AllCalendarEntries = new ObservableCollection<UserCalendarEntryModel>(entries);
            // }
        }
        catch (Exception)
        {
            // Silently handle errors loading calendar entries
        }
    }

    public async Task<bool> DeleteCalendarEntryAsync(Guid entryId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/Calendar/Entry/{entryId}");
            if (response.IsSuccessStatusCode)
            {
                // Remove from local collection
                var entry = AllCalendarEntries.FirstOrDefault(x => x.Id == entryId);
                if (entry.Id != Guid.Empty)
                {
                    AllCalendarEntries.Remove(entry);
                    return true;
                }
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    [RelayCommand]
    private void ToggleViewMode()
    {
        ViewMode = ViewMode == CalendarViewMode.Month ? CalendarViewMode.Week : CalendarViewMode.Month;
    }

    [RelayCommand]
    private void NavigatePreviousDay()
    {
        if (SelectedDate.HasValue)
        {
            SelectedDate = SelectedDate.Value.AddDays(-1);
        }
    }

    [RelayCommand]
    private void NavigateNextDay()
    {
        if (SelectedDate.HasValue)
        {
            SelectedDate = SelectedDate.Value.AddDays(1);
        }
    }

    private void UpdateHeaderText()
    {
        if (!SelectedDate.HasValue)
        {
            HeaderText = DateTime.Today.ToString("MMMM yyyy");
            return;
        }

        if (ViewMode == CalendarViewMode.Month)
        {
            HeaderText = SelectedDate.Value.ToString("MMMM yyyy");
        }
        else // Week mode
        {
            var startOfWeek = SelectedDate.Value.AddDays(-(int)SelectedDate.Value.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(6);
            HeaderText = $"{startOfWeek:MMM d} - {endOfWeek:MMM d, yyyy}";
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Unsubscribe from event to prevent memory leaks
            MyCalendar.DateSelectionChanged -= OnDateSelectionChanged;
        }

        _disposed = true;
    }
}

public class CalendarMealGroup
{
    public int MealType { get; set; }
    public string MealTypeName { get; set; } = string.Empty;
    public ObservableCollection<UserCalendarEntryModel> Recipes { get; set; } = [];
}

