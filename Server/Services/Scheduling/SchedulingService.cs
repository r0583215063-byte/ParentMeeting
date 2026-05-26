using DataContext;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Drawing.Chart;
using Repository;
using Repository.Entities;
using Service.Dto;
using Service.Interfaces;
using System.Collections;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Service.Scheduling
{


    public class SchedulingService : ISchedulingService
    {
        private readonly SchoolParentMeetingSystemContext _context;

        public SchedulingService(SchoolParentMeetingSystemContext context)
        {
            _context = context;
        }

        public async Task<List<ParentMeetingDto>> ProcessSchedulingAsync(int schoolId)
        {
            var school = await _context.Schools.AsNoTracking().FirstOrDefaultAsync(s => s.Id == schoolId);
            if (school == null) return new List<ParentMeetingDto>();

            var students = await _context.Students.AsNoTracking().Where(s => s.SchoolId == schoolId).ToListAsync();
            var parents = await _context.Parents.AsNoTracking().Where(p => p.SchoolId == schoolId).ToListAsync();
            var parentConstraints = await _context.ParentAvailability.AsNoTracking().Where(pa => pa.SchoolId == schoolId && pa.MeetingDate.Date == school.MeetingDate.Date).ToListAsync();
            var teachers = await _context.Teachers.AsNoTracking().Where(t => t.SchoolId == schoolId).ToListAsync();

            //Classroom → Teacher Mapping
            var classTeacherIdMap = teachers.ToDictionary(t => t.ClassName, t => t.Id);
            var classTeacherNameMap = teachers.ToDictionary(t => t.ClassName, t => t.FullName);
            //ParentId → ParentIdentity Mapping
            var parentIdentityMap = parents.ToDictionary(p => p.Id, p => p.ParentIdentity);

            //Creating slots
            var timeSlots = GenerateTimeSlots(school);

            //Create busy meetings
            var classNames = students.Select(s => s.ClassName).Distinct().ToList();
            var classBusySlots = classNames.ToDictionary(name => name, name => new HashSet<TimeSpan>());

            //Building availability
            var parentAvailabilityDict = parentConstraints
                .Where(pc => !string.IsNullOrEmpty(pc.ParentIdentity))
                .GroupBy(pc => pc.ParentIdentity)
                .ToDictionary(g => g.Key, g => g.ToList());

            var allMeetings = new List<ParentMeetingDto>();

            //Grouping students by family
            var parentGroups = students
                .Where(s => {
                    int pId = Convert.ToInt32(s.ParentId);
                    return pId != 0 && parentIdentityMap.ContainsKey(pId);
                })
                .GroupBy(s => parentIdentityMap[Convert.ToInt32(s.ParentId)])
                .OrderByDescending(g => g.Count())
                .ToList();

            foreach (var group in parentGroups)
            {
                var parentIdentity = group.Key;
                var family = group.ToList();
                List<TimeSpan> selectedSlots = null;
                //All meetings within 30 minutes, must have positive availability, must have unavailability
                selectedSlots = FindOptimalSlots(parentIdentity, family, timeSlots, classBusySlots, parentAvailabilityDict, school.SlotDurationMinutes, 30, true);
                if (selectedSlots == null)
                    selectedSlots = FindOptimalSlots(parentIdentity, family, timeSlots, classBusySlots, parentAvailabilityDict, school.SlotDurationMinutes, 30, false);
                if (selectedSlots == null)
                    selectedSlots = FindOptimalSlots(parentIdentity, family, timeSlots, classBusySlots, parentAvailabilityDict, school.SlotDurationMinutes, 999, false, true);

                if (selectedSlots != null)
                {
                    for (int i = 0; i < family.Count; i++)
                    {
                        var student = family[i];
                        var slot = selectedSlots[i];

                        classBusySlots[student.ClassName].Add(slot);

                        classTeacherIdMap.TryGetValue(student.ClassName, out int tId);
                        classTeacherNameMap.TryGetValue(student.ClassName, out string tName);

                        allMeetings.Add(new ParentMeetingDto
                        {
                            SchoolId = school.Id,
                            StudentId = student.Id,
                            ParentId = Convert.ToInt32(student.ParentId),
                            TeacherId = tId,
                            TeacherName = tName ?? "לא הוגדר מורה",
                            ClassName = student.ClassName,
                            MeetingDate = school.MeetingDate,
                            StartTime = slot,
                            EndTime = slot.Add(TimeSpan.FromMinutes(school.SlotDurationMinutes))
                        });
                    }
                }
            }

            await SaveToDatabaseAsync(schoolId, allMeetings);
            return allMeetings;
        }

        private List<TimeSpan> FindOptimalSlots(string parentIdentity, List<Student> students, List<TimeSpan> allSlots, Dictionary<string, HashSet<TimeSpan>> classBusy, Dictionary<string, List<ParentAvailability>> availDict, int duration, double maxGap, bool strictConstraints, bool ignoreConstraintsEntirely = false)
        {
            var bestOptions = new List<(List<TimeSpan> Slots, double Gap)>();

            //Every hour is checked as a start.
            foreach (var startAnchor in allSlots)
            {
                var trialSlots = new List<TimeSpan>();
                bool canFit = true;

                foreach (var student in students)
                {
                    var slot = allSlots
                        .Where(s => s >= startAnchor)
                        .Where(s => !classBusy[student.ClassName].Contains(s))
                        .Where(s => !trialSlots.Contains(s)) 
                        .Where(s => ignoreConstraintsEntirely || IsSlotAllowed(parentIdentity, s, availDict, duration, strictConstraints, true))
                        .OrderBy(s => s)
                        .FirstOrDefault();

                    if (slot == TimeSpan.Zero && !allSlots.Contains(TimeSpan.Zero)) { canFit = false; break; }
                    trialSlots.Add(slot);
                }

                if (canFit && trialSlots.Count == students.Count)
                {
                    double currentGap = (trialSlots.Max() - trialSlots.Min()).TotalMinutes;
                    if (currentGap <= maxGap)
                        bestOptions.Add((new List<TimeSpan>(trialSlots), currentGap));
                }
            }
            return bestOptions.OrderBy(o => o.Gap).ThenBy(o => o.Slots.Min()).Select(o => o.Slots).FirstOrDefault();
        }

        //Checks if the time is suitable for the parent.
        private bool IsSlotAllowed(string parentIdentity, TimeSpan slot, Dictionary<string, List<ParentAvailability>> dict, int duration, bool usePositive, bool useNegative)
        {
            if (string.IsNullOrEmpty(parentIdentity) || !dict.ContainsKey(parentIdentity)) return true;
            var constraints = dict[parentIdentity];
            var end = slot.Add(TimeSpan.FromMinutes(duration));

            if (useNegative && constraints.Any(c => !c.IsAvailable && ((slot >= c.StartTime && slot < c.EndTime) || (end > c.StartTime && end <= c.EndTime))))
                return false;

            if (usePositive)
            {
                var pos = constraints.Where(c => c.IsAvailable).ToList();
                if (pos.Any() && !pos.Any(c => slot >= c.StartTime && end <= c.EndTime))
                    return false;
            }
            return true;
        }

        //Creating slots
        private List<TimeSpan> GenerateTimeSlots(School school)
        {
            var slots = new List<TimeSpan>();
            var duration = TimeSpan.FromMinutes(school.SlotDurationMinutes);
            for (var t = school.MeetingStartTime; t + duration <= school.MeetingEndTime; t += duration)
                slots.Add(t);
            return slots;
        }

        private async Task SaveToDatabaseAsync(int schoolId, List<ParentMeetingDto> dtos)
        {
            var existing = await _context.ParentMeetings.Where(m => m.SchoolId == schoolId).ToListAsync();
            _context.ParentMeetings.RemoveRange(existing);

            _context.ParentMeetings.AddRange(dtos.Select(d => new ParentMeeting
            {
                SchoolId = d.SchoolId,
                StudentId = d.StudentId,
                ParentId = d.ParentId,
                TeacherId = d.TeacherId,
                ClassName = d.ClassName,
                MeetingDate = d.MeetingDate,
                StartTime = d.StartTime,
                EndTime = d.EndTime,
                IsPast = d.IsPast
            }));
            await _context.SaveChangesAsync();
        }
    }
}