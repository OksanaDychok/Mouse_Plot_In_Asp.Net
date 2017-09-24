using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chart_mouse.Models
{
    public class SessionViewModel
    {
        public IEnumerable<Session> AvailableSessions { get; set; }
        public IEnumerable<Session> SelectedSessions { get; set; }
        public PostedSessions PostedSessions { get; set; }
    }
}