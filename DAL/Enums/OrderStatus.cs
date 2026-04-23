using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Enums;

public enum OrderStatus
{
    Pending = 1,
    AwaitingFee = 2,
    Confirmed = 3,
    Completed = 4,
    Rejected = 5,
    Cancelled = 6
}
