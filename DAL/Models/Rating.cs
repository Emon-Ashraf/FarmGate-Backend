using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models;

public class Rating : BaseEntity
{
    public long OrderId { get; set; }
    public long CustomerId { get; set; }
    public long FarmerId { get; set; }
    public int Score { get; set; }
    public string? Comment { get; set; }

    public Order Order { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public Farmer Farmer { get; set; } = null!;
}