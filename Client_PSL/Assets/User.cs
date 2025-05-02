using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client_PSL.Assets
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GenerateUIAttribute : Attribute
    {

    }

    [GenerateUI]
    public class User
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public bool IsOnline { get; set; }
    }
}
