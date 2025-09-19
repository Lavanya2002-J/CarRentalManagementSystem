using System.Collections.Generic;

namespace CarRentalManagementSystem.ViewModels.ReportViewModels
{
    public class AdvancedReportViewModel
    {
        public List<MonthlyRevenueViewModel> MonthlyRevenue { get; set; }
        public List<PopularCarViewModel> PopularCars { get; set; }
        public List<TopCustomerViewModel> TopCustomers { get; set; }
    }
}
