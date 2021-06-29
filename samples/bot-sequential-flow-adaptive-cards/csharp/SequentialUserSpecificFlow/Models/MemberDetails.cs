using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SequentialUserSpecificFlow.Models
{
    public class MemberDetails
    {
        public Info info { get; set; }
    }
    public class Info
    {
        public string value { get; set; }
        public string title { get; set; }
    }
}
