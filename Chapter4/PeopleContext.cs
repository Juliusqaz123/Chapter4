using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter4
{
    public class PeopleContext : DbContext
    {
        public IDbSet<Person> People { get; set; }
    }
}
