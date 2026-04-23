using DAL.Enums;

namespace DAL.Models;

public class Order : BaseEntity
{
    public long CustomerId { get; set; }
    public long FarmerId { get; set; }
    public long PickupLocationId { get; set; }

    public OrderStatus Status { get; set; }
    public CancellationReason? CancellationReason { get; set; }

    public decimal EstimatedProductTotal { get; set; }
    public decimal? ActualProductTotal { get; set; }
    public decimal ServiceFeeAmount { get; set; }

    public DateTime? FeePaidAt { get; set; }

    public string PickupCode { get; set; } = null!;
    public DateTime PickupCodeExpiresAt { get; set; }
    public DateTime? PickupCodeUsedAt { get; set; }

    public DateTime PickupDueAt { get; set; }

    public string PickupCitySnapshot { get; set; } = null!;
    public string PickupAreaSnapshot { get; set; } = null!;
    public string PickupAddressSnapshot { get; set; } = null!;
    public string? PickupInstructionsSnapshot { get; set; }
    public string? FarmerPhoneSnapshot { get; set; }

    public DateTime? StockReservedAt { get; set; }

    public Customer Customer { get; set; } = null!;
    public Farmer Farmer { get; set; } = null!;
    public PickupLocation PickupLocation { get; set; } = null!;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public Payment? Payment { get; set; }
    public Rating? Rating { get; set; }
    public ICollection<IssueReport> IssueReports { get; set; } = new List<IssueReport>();
}