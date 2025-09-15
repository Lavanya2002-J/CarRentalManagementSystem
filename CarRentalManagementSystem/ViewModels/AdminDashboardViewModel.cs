namespace CarRentalManagementSystem.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int AvailableCarsCount { get; set; }
        public int ActiveBookingsCount { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TotalCustomersCount { get; set; }
    }
}
