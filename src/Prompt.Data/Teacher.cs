using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prompt.Data
{
    public class Teacher
    {
        public string Name { get; private set; }
        public string Bio { get; private set; }
        public Uri Photo { get; private set; }

        public IEnumerable<Course> Courses { get; set; }

        public Teacher(string name, string photo, string bio)
        {
            Name = name;
            Photo = new Uri(photo);
            Bio = bio;
        }
    }
}
