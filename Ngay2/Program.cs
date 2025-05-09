using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudentManagementSystem
{
    public enum CourseLevel
    {
        Beginner,
        Intermediate,
        Advanced
    }

    public abstract class User
    {
        public abstract void Login();
        public abstract void Logout();
    }

    public interface ICanLearn
    {
        void RegisterCourse(Course course);
        void TakeExam(string courseId, double score);
    }

    public class Person
    {
        public string FullName { get; set; }
        public string Email { get; set; }

        public virtual void DisplayInfo()
        {
            Console.WriteLine($"Tên: {FullName} | Email: {Email}");
        }
    }

    public class Student : Person, ICanLearn
    {
        public string ID { get; set; }
        public string Level { get; set; }
        public List<Enrollment> Enrollments { get; set; }

        public Student(string id, string fullName, string email)
        {
            ID = id;
            FullName = fullName;
            Email = email;
            Enrollments = new List<Enrollment>();
            Level = "Chưa xác định";
        }

        public void RegisterCourse(Course course)
        {
            Enrollments.Add(new Enrollment(this, course));
            Console.WriteLine($"--> Đăng ký thành công khóa: {course.CourseName}");
        }

        public void TakeExam(string courseId, double score)
        {
            var enrollment = Enrollments.FirstOrDefault(e => e.Course.CourseID == courseId);
            if (enrollment != null)
            {
                enrollment.Score = score;
                Console.WriteLine("--> Nhập điểm thành công.");
            }
            else
            {
                Console.WriteLine("Không tìm thấy khóa học.");
            }
        }

        public void CalculateLevel()
        {
            double avg = Enrollments.Count > 0 ? Enrollments.Average(e => e.Score) : 0;
            if (avg >= 8) Level = "Advanced";
            else if (avg >= 5) Level = "Intermediate";
            else Level = "Beginner";
        }

        public override void DisplayInfo()
        {
            base.DisplayInfo();
            Console.WriteLine($"ID: {ID} | Trình độ: {Level}");
            foreach (var e in Enrollments)
            {
                Console.WriteLine($"   - {e.Course.CourseName} ({e.Course.Level}) | Điểm: {e.Score}");
            }
        }

        public void Login() => Console.WriteLine($"{FullName} đã đăng nhập.");
        public void Logout() => Console.WriteLine($"{FullName} đã đăng xuất.");
    }

    public class Course
    {
        public string CourseID { get; set; }
        public string CourseName { get; set; }
        public CourseLevel Level { get; set; }

        public Course(string id, string name, CourseLevel level)
        {
            CourseID = id;
            CourseName = name;
            Level = level;
        }
    }

    public class Enrollment
    {
        public Student Student { get; set; }
        public Course Course { get; set; }
        public double Score { get; set; }

        public Enrollment(Student student, Course course)
        {
            Student = student;
            Course = course;
            Score = 0;
        }
    }

    class Program
    {
        static List<Student> students = new List<Student>();
        static List<Course> courses = new List<Course>();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            SeedCourses();

            while (true)
            {
                Console.WriteLine("\n======== MENU =========");
                Console.WriteLine("1. Thêm học viên");
                Console.WriteLine("2. Đăng ký khóa học");
                Console.WriteLine("3. Nhập điểm");
                Console.WriteLine("4. Hiển thị danh sách");
                Console.WriteLine("5. Ghi dữ liệu ra file");
                Console.WriteLine("6. Thoát");
                Console.Write("Chọn: ");

                try
                {
                    int choice = int.Parse(Console.ReadLine());
                    switch (choice)
                    {
                        case 1: AddStudent(); break;
                        case 2: RegisterCourse(); break;
                        case 3: InputScore(); break;
                        case 4: ShowStudents(); break;
                        case 5: SaveToFile(); break;
                        case 6: return;
                        default: Console.WriteLine("Lựa chọn không hợp lệ."); break;
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Lỗi: Vui lòng nhập số nguyên.");
                }
            }
        }

        static void SeedCourses()
        {
            courses.Add(new Course("C001", "C# Cơ bản", CourseLevel.Beginner));
            courses.Add(new Course("C002", "Java nâng cao", CourseLevel.Advanced));
            courses.Add(new Course("C003", "Python nền tảng", CourseLevel.Intermediate));
        }

        static void AddStudent()
        {
            Console.Write("ID: "); string id = Console.ReadLine();
            Console.Write("Tên: "); string name = Console.ReadLine();
            Console.Write("Email: "); string email = Console.ReadLine();

            students.Add(new Student(id, name, email));
            Console.WriteLine("--> Thêm học viên thành công.");
        }

        static void RegisterCourse()
        {
            Console.Write("ID học viên: "); string id = Console.ReadLine();
            var student = students.FirstOrDefault(s => s.ID == id);
            if (student == null)
            {
                Console.WriteLine("Không tìm thấy học viên.");
                return;
            }

            Console.WriteLine("--> Danh sách khóa học:");
            foreach (var c in courses)
                Console.WriteLine($"{c.CourseID}: {c.CourseName} ({c.Level})");

            Console.Write("Nhập ID khóa học: ");
            string courseId = Console.ReadLine();
            var course = courses.FirstOrDefault(c => c.CourseID == courseId);

            if (course == null)
            {
                Console.WriteLine("Không tìm thấy khóa học.");
                return;
            }

            student.RegisterCourse(course);
        }

        static void InputScore()
        {
            Console.Write("ID học viên: ");
            string id = Console.ReadLine();
            var student = students.FirstOrDefault(s => s.ID == id);
            if (student == null)
            {
                Console.WriteLine("Không tìm thấy học viên.");
                return;
            }

            Console.Write("ID khóa học: ");
            string courseId = Console.ReadLine();

            try
            {
                Console.Write("Điểm: ");
                double score = double.Parse(Console.ReadLine());
                student.TakeExam(courseId, score);
                student.CalculateLevel();
            }
            catch (FormatException)
            {
                Console.WriteLine("Lỗi: Điểm phải là số.");
            }
        }

        static void ShowStudents()
        {
            foreach (var s in students)
            {
                Console.WriteLine("--------------------");
                s.DisplayInfo();
            }
        }

        static void SaveToFile()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("students.csv"))
                {
                    foreach (var s in students)
                    {
                        foreach (var e in s.Enrollments)
                        {
                            sw.WriteLine($"{s.ID},{s.FullName},{s.Email},{s.Level},{e.Course.CourseID},{e.Course.CourseName},{e.Score}");
                        }
                    }
                }
                Console.WriteLine("Đã lưu dữ liệu vào file students.csv");
            }
            catch (IOException ex)
            {
                Console.WriteLine("Lỗi ghi file: " + ex.Message);
            }
        }
    }
}
