using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReferenceExplorer.Models;

namespace ReferenceExplorer.Tests
{
    public class TestSolutionProjectsProvider : ISolutionProjectsProvider
    {
        public async IAsyncEnumerable<Project> GetProjectsFrom(string slnPath, IProgress<int> progress = null)
        {
            var referencedProjects = new List<Reference>
            {
                new Reference {UsagesInSource = 5, Project = null}
            };

           // var projects = new List<Project>
            //{
            await Task.Delay(1000);
            yield return new Project {Name = "name1", Path = "path1", FormalReferences = referencedProjects};
            await Task.Delay(2000);
            yield return new Project {Name = "name2", Path = "path2"};
                //};

                //return projects;
        }
    }
}