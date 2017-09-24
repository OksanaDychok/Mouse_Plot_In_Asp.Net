using Chart_mouse.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using Highsoft.Web.Mvc.Charts;

namespace Chart_mouse.Controllers
{
    public class HomeController : Controller
    {
        const string FILENAME = @"C:\Users\Oksana\Desktop\Chart_mouse\Chart_mouse\App_Data\data.xml";
        XDocument doc = XDocument.Load(FILENAME);
        SessionViewModel svm;
        static string previous_group_name;

        //action that return in view all sessions
        public ActionResult Index()
        {
            svm = GetSessionsInitialModel();
            GetDataForPlot(svm.AvailableSessions);
            return View(svm);
        }

        //action that return in view only selected sessions
        [HttpPost, ActionName("Index")]
        public ActionResult Index(string Name, PostedSessions postedSessions)
        {
            svm = GetSessionsModel(Name, postedSessions);
            GetDataForPlot(svm.SelectedSessions);
            return View(svm);
        }

        private void GetDataForPlot(IEnumerable<Session> sessions)
        {
            //list for saving average tumor size
            List<double> ploty = new List<double>();
            //list for saving date  
            List<string> plotx = new List<string>();

            foreach (var xy in sessions)
            {
                ploty.Add(xy.averagseTumorSize);
                plotx.Add(xy.SessionDate.Substring(0, 10));
            }

            List<LineSeriesData> plotyData = new List<LineSeriesData>();

            ploty.ForEach(p => plotyData.Add(new LineSeriesData { Y = p }));

            //data transmission about the axis x and y in view
            ViewBag.plotyData = plotyData;
            ViewBag.plotx = plotx;

            //get names of all groups for drop down list
            var groups = from a in doc.Descendants("groups").Descendants("group")
                         select new Group
                         {
                             Name = a.Element("name").Value,
                         };
            ViewBag.SelectedGroups = new SelectList(groups.ToList(), "Name", "Name");
        }

        private SessionViewModel GetSessionsModel(string Name, PostedSessions postedSessions)
        {
            var model = new SessionViewModel();
            var selectedSessions = new List<Session>();
            var postedSessionIds = new string[0];
            if (postedSessions == null) postedSessions = new PostedSessions();

            // if a view model array of posted sessions names exists
            // and is not empty,save selected names
            if (postedSessions.SessionNames != null && postedSessions.SessionNames.Any())
            {
                postedSessionIds = postedSessions.SessionNames;
            }

            //select group by name in dropdownlist
            var group = (from a in doc.Descendants("groups").Descendants("group")
                          where a.Element("name").Value == Name
                          select new Group
                         {
                             Name = a.Element("name").Value,
                             Sessions = a.Descendants("session")
                                       .Select(x => new Session
                                       {
                                           SessionDate = x.Element("sessiondate").Value,
                                           averagseTumorSize = Math.Round((x.Descendants("animals").Descendants("animal").Descendants("data").Descendants("datum")
                                           .Select(y => new Animal
                                           {
                                               TumorSize = Convert.ToDouble(y.Element("value").Value.Replace(".", ","))
                                           }).ToList().Average(t => t.TumorSize)),2)
                                       }).ToList()
                         }).SingleOrDefault();
            ViewBag.groupName = group.Name;

            //check the name of the group change
            //if the previous name is not equal to the current one, select all days
            if (previous_group_name == Name)
            {
                // if there are any selected names saved, create a list of sessions
                if (postedSessionIds.Any())
                {
                    selectedSessions = group.Sessions
                     .Where(x => postedSessionIds.Any(s => x.SessionDate.ToString().Equals(s)))
                     .ToList();
                }
                model.SelectedSessions = selectedSessions;
            }
            else
            {
                model.SelectedSessions = group.Sessions.ToList();
                previous_group_name = Name;
            }

            //setup a view model
            model.AvailableSessions = group.Sessions.ToList();
            model.PostedSessions = postedSessions;

            return model;
        }

        private SessionViewModel GetSessionsInitialModel()
        {
            var model = new SessionViewModel();

            //select first group in xml file for initializing model
            var group = (from a in doc.Descendants("groups").Descendants("group")
                          select new Group
                          {
                              Name = a.Element("name").Value,
                              Sessions = a.Descendants("session")
                                        .Select(x => new Session
                                        {
                                            SessionDate = x.Element("sessiondate").Value,
                                            averagseTumorSize = Math.Round((x.Descendants("animals").Descendants("animal").Descendants("data").Descendants("datum")
                                           .Select(y => new Animal
                                           {
                                               TumorSize = Convert.ToDouble(y.Element("value").Value.Replace(".", ","))
                                           }).ToList().Average(t => t.TumorSize)), 2)
                                        }).ToList()
                          }).FirstOrDefault();
            //get initial group name 
            previous_group_name = group.Name;
            ViewBag.groupName = group.Name;
            //setup a view model
            model.AvailableSessions = group.Sessions.ToList();
            model.SelectedSessions = group.Sessions.ToList();

            return model;
        }

    }
}