using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DAL.Enums;

namespace DAL.Models;

public class OrderItem : BaseEntity
{
    public long OrderId { get; set; }
    public long ProductId { get; set; }

    public string ProductNameSnapshot { get; set; } = null!;
    public UnitType UnitTypeSnapshot { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal? FulfilledQuantity { get; set; }

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}