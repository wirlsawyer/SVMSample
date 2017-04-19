using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVMSample
{
   
    class DataInfo
    {
        public double Group;
        public double X;
        public double Y;

        public DataInfo(double group, double x, double y)
        {
            this.Group = group;
            this.X = x;
            this.Y = y;
        }
    }
}
