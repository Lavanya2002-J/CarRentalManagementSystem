namespace CarRentalManagementSystem.ViewModels.ReportViewModels
{
    public class TopCustomerViewModel
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
