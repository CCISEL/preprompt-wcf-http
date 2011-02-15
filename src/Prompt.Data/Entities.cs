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

    public interface IPromptRepository
    {
        IQueryable<Teacher> Teachers { get; }
        IQueryable<Course> Courses { get; }
    }

    public class InMemoryPromptRepository : IPromptRepository
    {
        public IQueryable<Teacher> Teachers { get { return _teachers.AsQueryable(); } }
        public IQueryable<Course> Courses { get { return _courses.AsQueryable(); } }

        private static readonly IEnumerable<Teacher> _teachers;
        private static readonly IEnumerable<Course> _courses;

        static InMemoryPromptRepository()
        {
            var cguedes = new Teacher(
                "Carlos Guedes",
                "http://prompt.cc.isel.ipl.pt/wp-content/uploads/2010/10/Foto-Carlos-Guedes-2010.jpg",
                "Carlos Guedes concilia a actividade académica no ISEL com a actividade profissional no ..."
                );

            var cmartins = new Teacher(
                "Carlos Martins",
                "http://prompt.cc.isel.ipl.pt/wp-content/uploads/2010/10/Foto-Carlos-Martins-2010.jpg",
                "Carlos Martins é professor do ISEL e membro do CCISEL com um longo percurso de colaboração ..."
                );

            var mcarvalho = new Teacher(
                "Fernando Miguel Carvalho",
                "http://prompt.cc.isel.ipl.pt/wp-content/uploads/2010/10/FM-Nov-2009.jpg",
                "Fernando Miguel Carvalho licenciou-se em 1997 em Engenharia Electrotécnica e de Computadores pelo IST e no mesmo ano iniciou a sua actividade profissional  ..."
                );

            var pgweb2 = new Course(
                "Programação para a Web 2",
                "http://prompt.cc.isel.ipl.pt/curso/plano-curricular/programacao-para-a-web-2/",
                "Este módulo tem por objectivo dotar os participantes com as competências essenciais para a utilização das tecnologias cliente, no Web Browser, na construção de aplicações Web."
                );
            pgweb2.Teachers = new List<Teacher>(){cguedes,mcarvalho};
            
            var pgconcur = new Course(
                "Programação concorrente e assíncrona",
                "http://prompt.cc.isel.ipl.pt/curso/plano-curricular/programacao-concorrente-e-assincrona/",
                "Conhecer as características do suporte para concorrência dos ambientes de execução actuais, incluindo as interfaces assíncronas e o respectivo modelo de programação."
                );

            cguedes.Courses = new List<Course>() {pgweb2};
            mcarvalho.Courses = new List<Course>() {pgweb2};
            cmartins.Courses = new List<Course>() {pgconcur};

            _teachers = new List<Teacher>(){cguedes,cmartins, mcarvalho};
            _courses = new List<Course>() {pgweb2, pgconcur};
            pgconcur.Teachers = new List<Teacher>() {cmartins};

        }
    

}
}
