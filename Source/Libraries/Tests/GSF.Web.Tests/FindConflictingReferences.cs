using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Web.Tests
{
    //[TestClass]
    public class UtilityTest
    {
        //[TestMethod]
        public void FindConflictingReferences()
        {
            string path = @"C:\Projects\gsf\Build\Output\Debug\Libraries\Web\";

            System.Console.WriteLine("Target Path = " + path);

            List<Assembly> assemblies = GetAllAssemblies(path);

            List<Reference> references = GetReferencesFromAllAssemblies(assemblies);

            IEnumerable<IGrouping<string, Reference>> groupsOfConflicts = FindReferencesWithTheSameShortNameButDiffererntFullNames(references);

            foreach (IGrouping<string, Reference> group in groupsOfConflicts)
            {
                System.Console.WriteLine("Possible conflicts for {0}:", group.Key);
                foreach (Reference reference in group)
                {
                    System.Console.WriteLine("{0} references {1}",
                                          reference.Assembly.Name.PadRight(25),
                                          reference.ReferencedAssembly.FullName);
                }
            }
        }

        private IEnumerable<IGrouping<string, Reference>> FindReferencesWithTheSameShortNameButDiffererntFullNames(List<Reference> references)
        {
            return from reference in references
                   group reference by reference.ReferencedAssembly.Name
                       into referenceGroup
                   where referenceGroup.ToList().Select(reference => reference.ReferencedAssembly.FullName).Distinct().Count() > 1
                   select referenceGroup;
        }

        private List<Reference> GetReferencesFromAllAssemblies(List<Assembly> assemblies)
        {
            List<Reference> references = new List<Reference>();
            foreach (Assembly assembly in assemblies)
            {
                foreach (AssemblyName referencedAssembly in assembly.GetReferencedAssemblies())
                {
                    references.Add(new Reference
                    {
                        Assembly = assembly.GetName(),
                        ReferencedAssembly = referencedAssembly
                    });
                }
            }
            return references;
        }

        private List<Assembly> GetAllAssemblies(string path)
        {
            List<FileInfo> files = new List<FileInfo>();
            DirectoryInfo directoryToSearch = new DirectoryInfo(path);
            files.AddRange(directoryToSearch.GetFiles("*.dll", SearchOption.AllDirectories));
            files.AddRange(directoryToSearch.GetFiles("*.exe", SearchOption.AllDirectories));

            return files.ConvertAll(file =>
            {
                try
                {
                    return Assembly.LoadFile(file.FullName);
                }
                catch
                {
                    return null;
                }
            })
            .Where(assembly => assembly != null)
            .ToList();
        }

        private class Reference
        {
            public AssemblyName Assembly { get; set; }
            public AssemblyName ReferencedAssembly { get; set; }
        }

    }
}