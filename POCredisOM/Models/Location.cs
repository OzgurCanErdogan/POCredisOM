using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redis.OM.Modeling;

namespace POCredisOM.Models;

public class Location
{
    public string Country { get; set; }

    [Indexed]
    public string City { get; set; }
}
