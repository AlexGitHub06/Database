using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    internal class StudentDB
    {
        private string _connectionString;

        public StudentDB(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<(string, string)> GetClasses(string surname)
        {
            var classes = new List<(string, string)>();
            using (SqliteConnection connection = new SqliteConnection())
            {
                connection.ConnectionString = _connectionString;
                connection.Open();
                SqliteCommand command = connection.CreateCommand();

                //paramaterize
                command.CommandText = "select Class.name ClassName, Teacher.FirstName || \" \" || Teacher.LastName TeacherName\r\nfrom Class join ClassAttendance\r\non Class.ClassID = ClassAttendance.ClassID\r\njoin Teacher\r\non Teacher.TeacherID = Class.TeacherID\r\njoin Student\r\non Student.StudentID = ClassAttendance.StudentID\r\nwhere Student.LastName = (@name)";
                var nameParameter = command.Parameters.Add("@name", SqliteType.Text);
                nameParameter.Value = surname;

                var dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    var studentClass = dataReader.GetString(0);
                    var classTeacher = dataReader.GetString(1);
                    classes.Add((studentClass, classTeacher));
                }

                return classes;
            }
        }

        public void AddClass(string teacherLastName, string courseTitle, string name)
        {
            string teacherID = "";
            string courseID = "";

            using (SqliteConnection connection = new SqliteConnection())
            {
                connection.ConnectionString = _connectionString;
                connection.Open();
                SqliteCommand command = connection.CreateCommand();

                command.CommandText = "select TeacherID from Teacher where Teacher.LastName == (@teacherLastname)";
                var lastNameParameter = command.Parameters.Add("@teacherLastname", SqliteType.Text);
                lastNameParameter.Value = teacherLastName;

                var dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    teacherID = dataReader.GetString(0); //will be either 1 or 0 records (teacher Lastname unique)
                }

            }

            using (SqliteConnection connection = new SqliteConnection())
            {
                connection.ConnectionString = _connectionString;
                connection.Open();
                SqliteCommand command = connection.CreateCommand();

                command.CommandText = "select CourseID from Course where Course.Title == (@courseTitle)";
                var courseTitleParameter = command.Parameters.Add("@courseTitle", SqliteType.Text);
                courseTitleParameter.Value = courseTitle;

                var dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    courseID = dataReader.GetString(0); //same here
                    var studentClass = dataReader.GetString(0);
                }
            }

            using (SqliteConnection connection = new SqliteConnection())
            {
                connection.ConnectionString = _connectionString;
                connection.Open();
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = $"insert into Class (TeacherID, CourseID, Name) values ({teacherID}, {courseID}, (@name))";

                var nameParameter = command.Parameters.Add("@name", SqliteType.Text);
                nameParameter.Value = name;
                command.ExecuteNonQuery();
            }
        }


        public void JoinClass(string surname, string className)
        {
            string studentID = "";
            string classID = "";

            studentID = FindStudentID(surname);

            using (SqliteConnection connection = new SqliteConnection())
            {
                connection.ConnectionString = _connectionString;
                connection.Open();
                SqliteCommand command = connection.CreateCommand();

                command.CommandText = $"select ClassID from Class where Class.Name == (@cName)";
                var cName = command.Parameters.Add("@cName", SqliteType.Text);
                cName.Value = className;

                var dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    classID = dataReader.GetString(0); //should only be 1 record, but if there isn't...
                }

            }

            using (SqliteConnection connection = new SqliteConnection())
            {
                connection.ConnectionString = _connectionString;
                connection.Open();
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = $"insert into ClassAttendance(StudentID, ClassID) values({studentID}, {classID})";
                command.ExecuteNonQuery();
            }
        }

        public void UnenrollClass(string surname, string className)
        {
            string classID = "";

            using (SqliteConnection connection = new SqliteConnection())
            {
                connection.ConnectionString = _connectionString;
                connection.Open();
                SqliteCommand command = connection.CreateCommand();

                command.CommandText = $"select ClassID from Class where Class.Name == (@cName)";
                var cName = command.Parameters.Add("@cName", SqliteType.Text);
                cName.Value = className;

                var dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    classID = dataReader.GetString(0); //should only be 1 record, but if there isn't...
                }

            }

            using (SqliteConnection connection = new SqliteConnection())
            {
                string studentID = FindStudentID(surname);

                connection.ConnectionString = _connectionString;
                connection.Open();
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = $"delete from ClassAttendance where StudentID == {studentID} and ClassID == {classID}";
                command.ExecuteNonQuery();
            }
        }

        public string VerifyClasses(string surname)
        {
            var classes = new List<string>();
            using (SqliteConnection connection = new SqliteConnection())
            {
                connection.ConnectionString = _connectionString;
                connection.Open();
                SqliteCommand command = connection.CreateCommand();

                command.CommandText = "select Class.name ClassName from Class join ClassAttendance on Class.ClassID = ClassAttendance.ClassID join Student on Student.StudentID = ClassAttendance.StudentID where Student.LastName = (@name)";
                var nameParameter = command.Parameters.Add("@name", SqliteType.Text);
                nameParameter.Value = surname;

                var dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    var studentClass = dataReader.GetString(0);
                    classes.Add(studentClass);
                }

                if(classes.Count != 5)
                {
                    return ($"Invalid: you need 5 classes, you currently have {classes.Count}");
                }

                bool hasHumanity = false;
                bool hasMaths = false;
                bool hasScience = false;

                foreach (string s in classes)
                {
                    if(GetCategory(s) == "Humanity") { hasHumanity = true; }
                    if (GetCategory(s) == "Maths") { hasMaths = true; }
                    if (GetCategory(s) == "Science") { hasScience = true; }
                }

                if (hasHumanity && hasMaths && hasScience)
                {
                    return ("Valid classes");
                }

                else
                {
                    return ($"Invalid: you need a maths, humanity and science\nYou currently have \nHumanity: {hasHumanity}\nMaths: {hasMaths}\nScience: {hasScience}");
                }


            }
        }

        

        string GetCategory(string className)
        {
            using (SqliteConnection connection = new SqliteConnection())
            {
                var course = "";
                connection.ConnectionString = _connectionString;
                connection.Open();
                SqliteCommand command = connection.CreateCommand();

                command.CommandText = "select Course.Category from Course join Class on class.CourseID = Course.CourseID where Class.name = (@className)";
                var cName = command.Parameters.Add("@className", SqliteType.Text);
                cName.Value = className;

                var dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    course = dataReader.GetString(0);
                }
                return course;
            }
        }

        string FindStudentID(string surname)
        {
            string studentID = "";

            using (SqliteConnection connection = new SqliteConnection())
            {
                connection.ConnectionString = _connectionString;
                connection.Open();
                SqliteCommand command = connection.CreateCommand();

                command.CommandText = $"select StudentID from Student where Student.LastName == (@sName)";
                var sName = command.Parameters.Add("@sName", SqliteType.Text);
                sName.Value = surname;

                var dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    studentID = dataReader.GetString(0); //none or 1, student lastname unique
                }
            }

            return studentID;
        }


    }
}
