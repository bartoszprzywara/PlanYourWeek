using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanYourWeek.Models
{
    public interface IComplexProperty
    {
        int Id { get; set; }
        string Name { get; set; }
    }
}
