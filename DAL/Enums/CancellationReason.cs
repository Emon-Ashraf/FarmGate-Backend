using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Enums;

public enum CancellationReason
{
    CustomerCancelled = 1,
    PaymentTimeout = 2,
    FarmerUnavailable = 3,
    CustomerNoShow = 4,
    AdminCancelled = 5
}
