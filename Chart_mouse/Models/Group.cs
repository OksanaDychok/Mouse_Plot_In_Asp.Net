using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chart_mouse.Models
{
    public class Group
    {
        public string Name { get; set; }
        public List<Session> Sessions {get; set; }
    }
}