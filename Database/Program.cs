using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Reflection;

namespace Database
{
    internal class Program
    {
        static void Main()
        {
            string connectionString = "Data Source = C:\\Users\\Alex\\Documents\\!Lower 8th\\Computer Science\\GitRepo\\C#\\Database\\Database\\GCSEStudentDatabase.db; Mode = ReadWrite";
            Menu(connectionString);

        }

        static void Menu(string connectionString)
        {
            Console.WriteLine("Welcome to the GCSE Option Database");
            Console.WriteLine("Please enter your surname:");
            string surname = Console.ReadLine();

            StudentDB db = new(connectionString);
            bool run = true;

            while (run)
            {
                Console.WriteLine("Choose option:");
                Console.WriteLine("(1) View classes");
                Console.WriteLine("(2) Add a class"); //if the program was expanded a teacher would do this
                Console.WriteLine("(3) Join a class");
                Console.WriteLine("(4) Unenroll from a class"); 
                Console.WriteLine("(5) Check classes' validity");
                Console.WriteLine("(6) Exit program");
                string choice = Console.ReadLine();  //verify

                switch (choice)
                {
                    case "1":
                        var classes = db.GetClasses(surname); 
                        PrintClasses(classes);
                        break;
                    case "2":
                        Console.WriteLine("Enter teacher last name:");
                        string teacherLName = Console.ReadLine();
                        Console.WriteLine("Enter course title:");
                        string courseTitle = Console.ReadLine();
                        Console.WriteLine("Enter class name:");
                        string className = Console.ReadLine();
                        //db.AddClass(surname, teacherLName, courseTitle, className); //add try for errors
                        db.AddClass(teacherLName, courseTitle, className);
                        break;
                    case "3":
                        Console.WriteLine("What is the class name:");
                        string joinClassName = Console.ReadLine();
                        db.JoinClass(surname, joinClassName);
                        break;
                    case "4":
                        Console.WriteLine("What is the class name:");
                        string classUnenrollName = Console.ReadLine();
                        db.UnenrollClass(surname, classUnenrollName);
                        break;
                    case "5":
                        Console.WriteLine(db.VerifyClasses(surname));
                        break;
                    case "6":
                        run = false;
                        break;

                }
            }

            

        }

        static void PrintClasses(List<(string,string)> classes)
        {
            Console.WriteLine("Classes:");
            foreach (var sClass in classes)
            {
                Console.WriteLine("Class: " + sClass.Item1 + ", Teacher: " + sClass.Item2);
            }
            Console.WriteLine();
        }
    }
}