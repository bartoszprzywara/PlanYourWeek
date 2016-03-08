using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remonty.Models
{
    public class Project
    {
        public string ProjectName;
    }

    public class ProjectManager
    {
        public static List<string> getProjects()
        {
            var projects = new List<string>();
            projects.Add("Pomalować mieszkanie");
            projects.Add("Wymienić kaloryfery");
            projects.Add("Położyć panele");
            projects.Add("<Dodaj nowy>");

            return projects;
        }
    }
}
