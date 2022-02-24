using LibGit2Sharp;
using System.Text.RegularExpressions;

string solutionDirectory = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.FullName;
string repoPath = Path.Combine(solutionDirectory, ".git");

using (Repository repo = new Repository(repoPath))
{
    string version = Regex.Replace(repo.Head.Commits.First().Message, @"\s.*", "");
    string datetimeString = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
    string infoHtmlCode = $"<div id='version'>v{version}<br>{datetimeString}</div>";
    string appComponentPath = Path.Combine(solutionDirectory, @"Client\src\app\app.component.html");
    string appComponentText = File.ReadAllText(appComponentPath);

    if (Regex.IsMatch(appComponentText, @"<div id='version'>"))
    {
        appComponentText = Regex.Replace(appComponentText, @"<div id='version'>.*", infoHtmlCode);
    }
    else
    {
        appComponentText += infoHtmlCode;
    }
    
    File.WriteAllText(appComponentPath, appComponentText);
}