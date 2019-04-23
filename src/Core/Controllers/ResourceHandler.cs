using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;

namespace ResxEditor.Core.Controllers
{
    public class ResourceHandler
    {
        public ResourceHandler(string fileName)
        {
            Resources = new List<ResXDataNode>();
            using (var reader = new ResXResourceReader(fileName) { UseResXDataNodes = true })
            {
                LoadResources(reader);
            }
        }

        public List<ResXDataNode> Resources { get; private set; }

        public int RemoveResource(string name)
        {
            return Resources.RemoveAll(resource => resource.Name == name);
        }

        public void AddResource(string name, string value, string comment = null)
        {
            Resources.Add(new ResXDataNode(name, value) { Comment = comment });
        }

        public void WriteToFile(string fileName)
        {
            using (var resxWriter = new ResXResourceWriter(fileName))
            {
                foreach(var resource in Resources.OrderBy(node => node.Name))
                {
                    resxWriter.AddResource(resource);
                }

                if (Resources.Count == 0)
                {
                    resxWriter.AddMetadata("", "");
                }
            }

            var resxFileText = string.Empty;
            using (var streamReader = new StreamReader(fileName))
            {
                resxFileText = streamReader.ReadToEnd();
            }

            var preserveSpaceLeadingNode = "<xsd:attribute name=\"mimetype\" type=\"xsd:string\" msdata:Ordinal=\"4\" />";
            var preserveSpaceNode = "<xsd:attribute ref=\"xml:space\" />";
            var preserveSpaceNodeSpacing = "              ";
            if (!resxFileText.Contains(preserveSpaceNode))
            {
                if (!resxFileText.Contains(preserveSpaceLeadingNode))
                {
                    throw new Exception("Failed to save resx file. No xsd attribute mimetype node");
                }

                var replaceText = $"{preserveSpaceLeadingNode}{Environment.NewLine}{preserveSpaceNodeSpacing}{preserveSpaceNode}";
                resxFileText = resxFileText.Replace(preserveSpaceLeadingNode, replaceText);
            }

            var resHeaderRegex = new Regex("(<resheader.*?>)(<value>.*?</value>)(</resheader>)");

            var resHeaderMatches = resHeaderRegex.Matches(resxFileText);
            foreach (var match in resHeaderMatches.Cast<Match>())
            {
                var stringToReplace = match.Value;
                var groups = match.Groups.Cast<Group>().Select(g => g.Value).ToList();
                var newStringBuilder = new StringBuilder();
                newStringBuilder.AppendLine($"  {groups[1]}");
                newStringBuilder.AppendLine($"    {groups[2]}");
                newStringBuilder.AppendLine($"  {groups[3]}");
                var newString = newStringBuilder.ToString();

                resxFileText = resxFileText.Replace(stringToReplace, newString);
            }

            var dataRegex = new Regex("( *?)<data name=\"(.*?)\">(<value>.*?</value>)(<comment>.*?</comment>)?</data>");

            var dataMatches = dataRegex.Matches(resxFileText);
            foreach (var match in dataMatches.Cast<Match>())
            {
                var stringToReplace = match.Value;
                var groups = match.Groups.Cast<Group>().Select(g => g.Value).ToList();
                var newStringBuilder = new StringBuilder();
                newStringBuilder.AppendLine($"  <data name=\"{groups[2]}\" xml:space=\"preserve\">");
                newStringBuilder.AppendLine($"    {groups[3]}");

                if (groups.Count > 4 && !string.IsNullOrWhiteSpace(groups[4]))
                {
                    newStringBuilder.AppendLine($"    {groups[4]}");
                }

                newStringBuilder.Append($"  </data>");
                var newString = newStringBuilder.ToString();

                resxFileText = resxFileText.Replace(stringToReplace, newString);
            }

            var rootEndingRegex = new Regex("( *?)</root>");
            var rootEndingMatch = rootEndingRegex.Match(resxFileText);
            if (rootEndingMatch.Success)
            {
                resxFileText = resxFileText.Replace(rootEndingMatch.Value, "</root>");
            }

            using (var streamWriter = new StreamWriter(fileName, false))
            {
                streamWriter.Write(resxFileText);
            }
        }

        private void LoadResources(ResXResourceReader resxReader)
        {
            var enumerator = resxReader.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Resources.Add(enumerator.Value as ResXDataNode);
            }
        }
    }
}