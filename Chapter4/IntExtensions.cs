using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter4
{
    public static class IntExtensions
    {
        public static int Multiply(this int x, int y)
        {
            return x * y;
        }
    }
}
