using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CommonPluginsShared
{
    public class Paths
    {
        public static string GetSafePath(string path, bool lastIsName = false)
        {
            string pathReturn = string.Empty;
            List<string> PathFolders = path.Split('\\').ToList();
            foreach (string folder in PathFolders)
            {
                if (pathReturn.IsNullOrEmpty())
                {
                    pathReturn += folder;
                }
                else
                {
                    if (PathFolders.IndexOf(folder) == PathFolders.Count - 1 && lastIsName)
                    {
                        pathReturn += "\\" + Paths.GetSafePathName(folder, false);
                    }
                    else
                    {
                        pathReturn += "\\" + Paths.GetSafePathName(folder, true);
                    }
                }
            }

            return pathReturn;
        }

        public static string GetSafePathName(string filename, bool keepNameSpace = false)
        {
            return keepNameSpace
                ? string.Join(" ", filename.Split(Path.GetInvalidFileNameChars())).Trim()
                : CommonPlayniteShared.Common.Paths.GetSafePathName(filename);
        }
    }
}
