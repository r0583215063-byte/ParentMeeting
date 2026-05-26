using DataContext;
using OfficeOpenXml;
using Repository;
using Repository.Entities;

namespace Service.Importing
{
    public class ExcelImportService
    {
        private readonly SchoolParentMeetingSystemContext _context;

        public ExcelImportService(SchoolParentMeetingSystemContext context)
        {
            _context = context;
        }
        //Stream - You can read it from memory, faster.
        public async Task ImportFromExcel(Stream fileStream, int schoolId)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using var package = new ExcelPackage(fileStream);

            if (package.Workbook.Worksheets.Count == 0)
                throw new Exception("Excel file is empty");

            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.Rows;

            var meetingsToRemove = _context.ParentMeetings.Where(m => m.SchoolId == schoolId);
            _context.ParentMeetings.RemoveRange(meetingsToRemove);

            var studentsToRemove = _context.Students.Where(s => s.SchoolId == schoolId);
            _context.Students.RemoveRange(studentsToRemove);

            var teachersToRemove = _context.Teachers.Where(t => t.SchoolId == schoolId);
            _context.Teachers.RemoveRange(teachersToRemove);

            var parentsToRemove = _context.Parents.Where(p => p.SchoolId == schoolId);
            _context.Parents.RemoveRange(parentsToRemove);

            await _context.SaveChangesAsync();

            //Dictionary works in memory and is very fast.
            var parentsDict = new Dictionary<string, Parent>();
            var teachersDict = new Dictionary<string, Teacher>();

            for (int row = 2; row <= rowCount; row++)
            {
                var firstName = worksheet.Cells[row, 1].Text.Trim();
                var lastName = worksheet.Cells[row, 2].Text.Trim();
                var studentIdentity = worksheet.Cells[row, 3].Text.Trim();
                var className = worksheet.Cells[row, 4].Text.Trim();
                var teacherName = worksheet.Cells[row, 5].Text.Trim();
                var parentIdentity = worksheet.Cells[row, 6].Text.Trim();
                var parentName = worksheet.Cells[row, 7].Text.Trim();
                var parentEmail = worksheet.Cells[row, 8].Text.Trim();

                if (string.IsNullOrEmpty(studentIdentity) || string.IsNullOrEmpty(parentIdentity))
                    continue;

                if (!parentsDict.TryGetValue(parentIdentity, out var parent))
                {
                    parent = new Parent
                    {
                        ParentIdentity = parentIdentity,
                        ParentName = parentName,
                        ParentEmail = parentEmail,
                        SchoolId = schoolId
                    };

                    _context.Parents.Add(parent);
                    parentsDict[parentIdentity] = parent;
                }

                if (!teachersDict.TryGetValue(className, out var teacher))
                {
                    teacher = new Teacher
                    {
                        FullName = teacherName,
                        ClassName = className,
                        SchoolId = schoolId
                    };

                    _context.Teachers.Add(teacher);
                    teachersDict[className] = teacher;
                }

                var student = new Student
                {
                    StudentIdentity = studentIdentity,
                    FirstName = firstName,
                    LastName = lastName,
                    ClassName = className,
                    Parent = parent,
                    Teacher = teacher,
                    SchoolId = schoolId
                };

                _context.Students.Add(student);
            }

            await _context.SaveChangesAsync();
        }
    }
}