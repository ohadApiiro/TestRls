using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using LspTypes;

namespace ConsoleApp;

public class InitializeParamsExt
{
    [DataMember(Name = "capabilities")]
    public ClientCapabilities capabilities { get; set; }
    
    [DataMember(Name = "processId")]
    public int? processId { get; set; }
    
    [DataMember(Name = "rootUri")]
    public string rootUri { get; set; }
    
    [DataMember(Name = "initializationOptions")]
    public object initializationOptions { get; set; }
}

public class ParamsFactory
{
    private static string _rootPath;
    private static string _cacheFolderPath = "/home/ohad/tmp";

    public static InitializeParamsExt GetInitObject(string rootPath)
    {
        _rootPath = rootPath;

        var initParams = new InitializeParamsExt
        {
            processId = Environment.ProcessId,
            rootUri = new Uri(rootPath).ToString(),

            capabilities = new()
            {
                TextDocument = new()
                {
                    Hover = new() { ContentFormat = new[] { MarkupKind.PlainText } },
                    References = new()
                },
                Workspace = new() { WorkspaceFolders = true }
            },
            initializationOptions = { } //new Foo {items = new []{"rust-analyzer"}}
        };

        return initParams;
    }

    public static string[] GetAllFiles()
    {
        var files = new List<string>();

        DirSearch(_rootPath, files);

        return files.ToArray();
    }

    static void DirSearch(string sDir, List<string> dirs)
    {
        if (sDir.EndsWith("venv"))
        {
            return;
        }

        foreach (string d in Directory.GetDirectories(sDir))
        {
            foreach (string fileName in Directory.GetFiles(d, "*.py"))
            {
                dirs.Add(fileName);
            }

            DirSearch(d, dirs);
        }
    }

    public static DidOpenTextDocumentParams GetDidOpenParams(string filePath) =>
        new()
        {
            TextDocument = new TextDocumentItem
            {
                Text = File.ReadAllText(filePath),
                Uri = filePath,
                LanguageId = "rust",
                Version = 0
            }
        };

    public static TextDocumentPositionParams GetPositionParams(string filePath, Position position)
    {
        TextDocumentPositionParams res = new TextDocumentPositionParams
        {
            TextDocument = new() { Uri = filePath },
            Position = position,
        };

        return res;
    }

    public static GotoDefinitionParams GetGotoDefinitionParams(string path, Position position) =>
        new()
        {
            position = position,
            textDocument = new()
            {
                uri = new Uri(path).ToString(),

                // TextDocument = new()
                // {
                //     Uri = new Uri(path).ToString()
                // }
            },
        };

    public static HoverParams GetHoverParams(string filePath, Position position)
    {
        return new HoverParams
        {
            position = position,
            textDocument = new TextDocumentIdentifier
            {
                Uri = new Uri(filePath).ToString()
            }
        };
    }
}

public class TextDocumentPositionParams2
{
    public string uri { get; set; }
    public Position position { get; set; }
}

public class GotoDefinitionParams
{
    public Position position { get; set; }

    public TextDocumentPositionParams2 textDocument { get; set; }

    public WorkDoneProgressParams workDone { get; set; }

    public PartialResultParams partialResult { get; set; }
}

public class HoverParams
{
    public TextDocumentIdentifier textDocument { get; set; }
    public Position position { get; set; }

    //[DataMember(Name = "work_done_progress_params")]
    public WorkDoneProgressParams WorkDone { get; set; }
}