using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ExOperation.OperatorHelper
{
    public class Project
    {
        protected const string _mainPath = "/project/";
        protected string _projectName;

        public Project(string name)
        {
            _projectName = name;
        }

        public string[] GetProjectTree()
        {
            string[] project = Directory.GetFiles(_mainPath + _projectName, "*", SearchOption.AllDirectories);
            for (int i = 0; i < project.Length; i++)
            {
                project[i] = project[i].Remove(0, _mainPath.Length);
            }
            return project;
        }

        public int GetTotalFiles()
        {
            string[] project = Directory.GetFiles(_mainPath + _projectName, "*", SearchOption.AllDirectories);
            return project.Length;
        }

        public string ProjectName
        {
            get
            {
                return _projectName;
            }
        }

        public string FullPath
        {
            get
            {
                return _mainPath + _projectName;
            }
        }
    }

    public class ProjectCollection : List<Project>
    {
        public ProjectCollection()
        {
            string path = "/project/";
            string[] mainPath = Directory.GetDirectories(path);
            for (int i = 0; i < mainPath.Length; i++)
            {
                mainPath[i].Remove(0, path.Length);
                this.Add(new Project(mainPath[i]));
            }
        }

        public void RefreshProjectList()
        {
            this.Clear();
            string path = "/project/";
            string[] mainPath = Directory.GetDirectories(path);
            for (int i = 0; i < mainPath.Length; i++)
            {
                mainPath[i].Remove(0, path.Length);
                this.Add(new Project(mainPath[i]));
            }
        }
    }
}
