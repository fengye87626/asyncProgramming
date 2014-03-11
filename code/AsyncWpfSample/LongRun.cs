using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncWpfSample {
    public class LongRun {
        public static long Calculate(long n) {
            if (n <= 1) {
                return n;
            }
            else {
                return Calculate(n - 1) + Calculate(n - 2);
            }
        }
    }
}
