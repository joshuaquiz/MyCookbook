using System.Linq;
using XCalendar.Core.Models;

namespace MyCookbook.App.Views;

public partial class CalendarHome
{
    public Calendar<CalendarDay> MyCalendar { get; set; }

    public CalendarHome()
    {
        MyCalendar = new Calendar<CalendarDay>();
        BindingContext = this;
        InitializeComponent();
        MyCalendar.DateSelectionChanged += (sender, args) =>
        {
            var oldStart = args.PreviousSelection.First().ToString("D");
            var oldEnd = args.PreviousSelection.First().ToString("D");
            var newStart = args.PreviousSelection.First().ToString("D");
            var newEnd = args.PreviousSelection.First().ToString("D");
            var oldDate = string.IsNullOrWhiteSpace(oldEnd)
                ? oldStart + "-" + oldEnd
                : oldStart;
            var newDate = string.IsNullOrWhiteSpace(newEnd)
                ? newStart + "-" + newEnd
                : newStart;
            DisplayAlert(
                "DateChange",
                "old: " + oldDate + ", new: " + newDate,
                "Ok");
        };
    }
}