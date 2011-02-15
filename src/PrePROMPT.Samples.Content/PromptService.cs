using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Prompt.Data;

namespace PrePROMPT.Samples.Content
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class PromptService
    {
        private readonly IPromptRepository _repo;
        public PromptService(IPromptRepository repo)
        {
            _repo = repo;
        }

        [WebGet(UriTemplate="courses")]
        IEnumerable<Course> GetCourses()
        {
            return _repo.Courses;
        }
    }
}
