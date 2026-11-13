using System;
using System.Collections.Generic;
using System.Globalization;

namespace DynamicIsland.Services;

public class CalendarDayItem
{
    public int DayNumber { get; set; }
    public bool IsToday { get; set; }
    public bool IsCurrentMonth { get; set; }
}

public class CalendarService
{
    private static readonly CultureInfo IdCulture = new("id-ID");

    public string CurrentFormattedDate => DateTime.Now.ToString("dddd, dd MMMM yyyy", IdCulture);
    public string CurrentTime => DateTime.Now.ToString("HH:mm:ss");
    public string MonthYearText => DateTime.Now.ToString("MMMM yyyy", IdCulture);

    public List<CalendarDayItem> GetMonthDays()
    {
        var result = new List<CalendarDayItem>();
        var now = DateTime.Now;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
        int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);

        // DayOfWeek: Sunday = 0, Monday = 1, dst.
        int offset = (int)firstDayOfMonth.DayOfWeek; // 0 untuk Minggu

        // Hari kosong / bulan sebelumnya
        for (int i = 0; i < offset; i++)
        {
            result.Add(new CalendarDayItem { DayNumber = 0, IsCurrentMonth = false });
        }

        // Hari dalam bulan ini
        for (int day = 1; day <= daysInMonth; day++)
        {
            result.Add(new CalendarDayItem
            {
                DayNumber = day,
                IsToday = (day == now.Day),
                IsCurrentMonth = true
            });
        }

        return result;
    }
}
