using System.Collections.Generic;
using MarinaRegSystem.Models;

public class DoctorScheduleCalendarViewModel
{
    public List<SubDepartment> SubDepartments { get; set; }
    public Dictionary<int, List<DoctorScheduleEvent>> EventsBySubDepartment { get; set; }
}
public class DoctorScheduleEvent
{
    public string Title { get; set; }
    public string Start { get; set; }
    public string End { get; set; }
}


public class DoctorScheduleTabsViewModel
{
    public List<SubDepartment> SubDepartments { get; set; }
    public List<DoctorSchedule> Schedules { get; set; }
}