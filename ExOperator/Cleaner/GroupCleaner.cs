using OperationLibrary.DatabaseStorage;
using OperationLibrary.OperatorHelper;
using System;

namespace OperationLibrary.Cleaner
{
    internal class GroupCleaner
    {
        public static void RemoveGroupFromEveryProject(string groupname)
        {
            string confGrp = "[G] " + groupname; 
            ProjectCollection projCol = ProjectBrowserStore.GetListProject();

            if (projCol.Count == 0)
                return;

            foreach (Project proj in projCol)
            {
                try
                {
                    ProjectBrowserStore.RemoveMemberFromProject(confGrp, proj);
                    MySQLAccountParsing.RemoveMemberFromProject(confGrp, proj);
                }
                catch (Exception ex)
                {
                    if (!(ex is MissingMemberException))
                    {
                        throw;
                    }
                    else continue;
                }
            }
        }
    }
}
