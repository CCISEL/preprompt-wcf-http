using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prompt.Data
{
    public class Course
    {
        public string Name { get; private set; }
        public Uri HtmlSyllabus { get; private set; }
        public string Syllabus { get; private set; }

        public IEnumerable<Teacher> Teachers { get; set; }

        public Course(string name, string uri, string syllabus)
        {
            Name = name;
            HtmlSyllabus = new Uri(uri);
            Syllabus = syllabus;
        }
    }
}
