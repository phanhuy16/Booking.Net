﻿namespace BookingApp.DTOs.Schedule
{
    public class ScheduleQueryParams
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
