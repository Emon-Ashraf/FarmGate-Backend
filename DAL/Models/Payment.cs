using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DAL.Enums;

namespace DAL.Models;

public class Payment : BaseEntity
{
    public long OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionReference { get; set; }
    public DateTime? PaidAt { get; set; }

    public Order Order { get; set; } = null!;
}
