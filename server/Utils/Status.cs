using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StdbModule.Utils
{
    public enum Status
    {
        Success = 0,
        UnknownError = 1,

        // Account creation 100
        UserNameTaken = 101,
    }
}
