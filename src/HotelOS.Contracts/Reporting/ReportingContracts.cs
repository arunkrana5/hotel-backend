namespace HotelOS.Contracts.Reporting;

// 1. Executive Dashboard KPIs
public record DailyRevenuePointDto(
    string Date,
    decimal RoomRevenue,
    decimal FnBRevenue,
    decimal BanquetRevenue,
    decimal TotalRevenue
);

public record DepartmentRevenueSliceDto(
    string Department,
    decimal Amount,
    double Percentage
);

public record OperationalHealthSummaryDto(
    int TotalRooms,
    int CleanRooms,
    int DirtyRooms,
    int InspectedRooms,
    int OccupiedRooms,
    int MaintenanceRooms,
    int OpenDiningTables,
    int OccupiedDiningTables,
    int PendingKitchenOrders,
    int OpenWorkOrders,
    int LowStockItemsCount,
    int ActiveVisitorsInPremises,
    int OpenSecurityIncidents
);

public record ExecutiveDashboardDto(
    long PropertyId,
    double OccupancyRate,
    decimal Adr,
    decimal RevPar,
    decimal TodayTotalRevenue,
    decimal MonthToDateRevenue,
    int ArrivalsToday,
    int DeparturesToday,
    int InHouseGuests,
    OperationalHealthSummaryDto OperationalHealth,
    List<DailyRevenuePointDto> RevenueTrend7Days,
    List<DepartmentRevenueSliceDto> DepartmentRevenueDistribution
);

// 2. Managerial Reports
public record NightAuditReportDto(
    string AuditDate,
    long PropertyId,
    int TotalRooms,
    int OccupiedRooms,
    double OccupancyPercent,
    decimal RoomTariffRevenue,
    decimal FoodBeverageRevenue,
    decimal BanquetRevenue,
    decimal TaxRevenue,
    decimal TotalRevenueGross,
    decimal TotalPaymentsSettled,
    decimal OutstandingCityLedgerBalance,
    int CheckInsProcessed,
    int CheckOutsProcessed,
    int FoliosAudited
);

public record AccountBalanceRowDto(
    string AccountCode,
    string AccountName,
    string AccountType,
    decimal OpeningBalance,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal ClosingBalance
);

public record RevenueTrialBalanceReportDto(
    string AsOfDate,
    long PropertyId,
    decimal TotalDebits,
    decimal TotalCredits,
    bool IsBalanced,
    List<AccountBalanceRowDto> Accounts
);

public record ForecastDayDto(
    string Date,
    int TotalRooms,
    int RoomsBooked,
    int RoomsAvailable,
    double ProjectedOccupancyPercent,
    decimal ProjectedRevenue
);

public record OccupancyForecastReportDto(
    long PropertyId,
    string GeneratedAt,
    List<ForecastDayDto> ForecastDays
);

public record DepartmentSpendRowDto(
    string DepartmentCode,
    string DepartmentName,
    int TotalRequisitions,
    int TotalOrders,
    decimal TotalSpendAmount
);

public record DepartmentProcurementReportDto(
    long PropertyId,
    decimal TotalProcurementSpend,
    List<DepartmentSpendRowDto> DepartmentSpends
);

public record DepartmentPayrollRowDto(
    string DepartmentCode,
    string DepartmentName,
    int EmployeeCount,
    decimal TotalMonthlyBaseSalary,
    decimal TotalOvertimeHours,
    decimal EstimatedOvertimeCost
);

public record StaffPayrollSummaryReportDto(
    long PropertyId,
    int TotalEmployees,
    decimal TotalMonthlyPayroll,
    decimal TotalOvertimeHoursAccumulated,
    List<DepartmentPayrollRowDto> DepartmentPayrolls
);
