//******************************************************************************************************
//  Program.cs - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/15/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using GSF.IO;

namespace GenerateGrafanaFunctionsMarkdown;

public class MarkdownGenerator
{
    private static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: MarkdownGenerator <XMLPath> <MarkdownOutputPath>");
            return;
        }

        string xmlPath = args[0];
        string outputMarkdownPath = args[1];

        try
        {
            // Load the XML file
            XDocument xmlDoc = XDocument.Load(xmlPath);

            // Extract the members from the BuiltIn namespace
            var members = xmlDoc.Descendants("member")
                .Where(m => m.Attribute("name")?.Value.StartsWith("T:GrafanaAdapters.Functions.BuiltIn") ?? false)
                .Where(m => m.Attribute("name")?.Value.Split(['.']).Length == 4)
                .Select(m => new
                {
                    Name = m.Attribute("name")?.Value,
                    Summary = GetCleanMarkdown(m.Element("summary")?.ToString(SaveOptions.DisableFormatting)),
                    Remarks = GetCleanMarkdown(m.Element("remarks")?.ToString(SaveOptions.DisableFormatting))
                })
                .OrderBy(m => m.Name)
                .ToList();

            // Build the markdown content
            StringBuilder markdownBuilder = new();

            // Include the forward section
            markdownBuilder.AppendLine(File.ReadAllText(FilePath.GetAbsolutePath("ForwardText.md")));

            // Build the table of contents section
            markdownBuilder.AppendLine("## Available Functions");
            markdownBuilder.AppendLine();

            foreach (var member in members)
            {
                string functionName = member.Name?.Split('.').LastOrDefault()?.Split('`').FirstOrDefault()!;

                if (string.IsNullOrEmpty(functionName))
                    continue;

                markdownBuilder.AppendLine($"* [`{functionName}`](#{functionName.ToLower()})");
            }

            // Build the function details section
            foreach (var member in members)
            {
                string functionName = member.Name?.Split('.').LastOrDefault()?.Split('`').FirstOrDefault()!;

                if (string.IsNullOrEmpty(functionName))
                    continue;

                markdownBuilder.AppendLine();
                markdownBuilder.AppendLine($"## {functionName}");
                markdownBuilder.AppendLine();

                if (!string.IsNullOrEmpty(member.Summary))
                {
                    foreach (string line in member.Summary.Split([Environment.NewLine], StringSplitOptions.None).Where(line => !string.IsNullOrWhiteSpace(line)))
                        markdownBuilder.Append($"{line.Trim().Replace("<br/>", $"{Environment.NewLine}{Environment.NewLine}")} ");

                    markdownBuilder.AppendLine();
                }

                if (!string.IsNullOrEmpty(member.Remarks))
                {
                    markdownBuilder.AppendLine();
                    
                    bool breakEncountered = false;

                    foreach (string line in member.Remarks.Split([Environment.NewLine], StringSplitOptions.None).Where(line => !string.IsNullOrWhiteSpace(line)))
                    {
                        if (breakEncountered)
                        {
                            markdownBuilder.Append($"{line.Trim()} ");
                        }
                        else
                        {
                            if (line.Contains("<br/>")) // Start of <para> block in remarks
                            {
                                breakEncountered = true;
                                markdownBuilder.AppendLine();
                            }
                            else
                            {
                                string detail = line.Trim();

                                //                     0123456789 
                                if (detail.StartsWith("Variants:", StringComparison.OrdinalIgnoreCase))
                                {
                                    detail = detail[9..].Trim();
                                    string[] variants = detail.Split(',');

                                    markdownBuilder.Append("* Variants: ");
                                    bool first = true;

                                    foreach (string variant in variants)
                                    {
                                        if (first)
                                            first = false;
                                        else
                                            markdownBuilder.Append(", ");

                                        markdownBuilder.Append(variant.Trim().StartsWith("`") ? variant.Trim() : $"`{variant.Trim()}`");
                                    }

                                    markdownBuilder.AppendLine();
                                }
                                else
                                {
                                    markdownBuilder.AppendLine($"* {detail}");
                                }
                            }
                        }
                    }
                }
            }

            // Write to the output markdown file
            File.WriteAllText(outputMarkdownPath, markdownBuilder.ToString());
            Console.WriteLine($"Markdown file successfully generated: {outputMarkdownPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static string GetCleanMarkdown(string? xmlCommentText)
    {
        return xmlCommentText?.Trim()
            .ReplaceTag("remarks")
            .ReplaceTag("summary")
            .ReplaceTag("code", "`")
            .ReplaceTag("c", "`")
            .ReplaceDoubleBreak("<para>")
            .ReplaceBreak()
            .ReplaceTag("para", "<br/>")
            .ReplaceSeeHref()
            .ReplaceSeeCref()
            .ReplaceExecutionModeLinks()
            ?? string.Empty;
    }
}

internal static class HtmlHelpers
{
    private const string HtmlTemplate = @"<\s*/?\s*{0}\s*>";
    private const string HtmlBreak = @"<\s*br\s*/?\s*>";
    private const string HtmlDoubleBreak = $@"{HtmlBreak}\s*{HtmlBreak}";
    private const string HtmlSeeHref = """<\s*see\s+href\s*=\s*"([^""]*)"\s*>(.*?)<\s*/\s*see\s*>""";
    private const string HtmlSeeCref = """<\s*see\s+cref\s*=\s*"T:GrafanaAdapters\.Functions\.BuiltIn\.([^`]+)(?:`[0-9]+)?"\s*/?>""";

    public static string ReplaceTag(this string text, string tag, string replacement = "")
    {
        return Regex.Replace(text, string.Format(HtmlTemplate, tag), replacement, RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }

    public static string ReplaceBreak(this string text, string replacement = "")
    {
        return Regex.Replace(text, HtmlBreak, replacement, RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }

    public static string ReplaceDoubleBreak(this string text, string replacement = "")
    {
        return Regex.Replace(text, HtmlDoubleBreak, replacement, RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }

    public static string ReplaceSeeHref(this string text)
    {
        return Regex.Replace(text, HtmlSeeHref, "[$2]($1)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }

    public static string ReplaceSeeCref(this string text)
    {
        return Regex.Replace(text, HtmlSeeCref, match =>
        {
            string functionName = match.Groups[1].Value;
            return $"[{functionName}](#{functionName.ToLower()})";
        },
        RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }

    public static string ReplaceExecutionModeLinks(this string text)
    {
        return text
            .Replace("Immediate in-memory array load", "[Immediate in-memory array load](#execution-modes)")
            .Replace("Immediate enumeration", "[Immediate enumeration](#execution-modes)")
            .Replace("Deferred enumeration", "[Deferred enumeration](#execution-modes)");
    }
}