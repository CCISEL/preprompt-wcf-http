using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prompt.Data
{
    public interface IPromptRepository
    {
        IQueryable<Teacher> Teachers { get; }
        IQueryable<Course> Courses { get; }
    }
}
