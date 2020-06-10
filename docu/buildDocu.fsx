
open System
open System.IO

let root = __SOURCE_DIRECTORY__
let contentDir = root + "/content"
let outputFileName = "../Readme.md"
let inputMarkdownFileName = contentDir + "/docu.md"

let refs =
    Directory.GetFiles(contentDir, "*.fsx")
    |> Seq.map File.ReadAllText
    |> String.concat "\n\n\n"
    |> fun s -> s.Split([| "//$ref:" |], StringSplitOptions.None)
    |> Seq.map (fun s -> s.Replace("\r", "").Split([| "\n" |], StringSplitOptions.None) |> Array.toList)
    |> Seq.map (List.skipWhile String.IsNullOrWhiteSpace)
    |> Seq.filter (List.isEmpty >> not)
    |> Seq.map (List.rev >> List.skipWhile String.IsNullOrWhiteSpace >> List.rev)
    |> Seq.map (fun lines ->
        let refName = lines.Head.Trim()
        let content = String.concat "\n" lines.Tail
        (refName, content))
    |> Map.ofSeq

let renderedMarkdown =
    [|
        let refMarker = "$ref:"
        
        for line in File.ReadAllLines(inputMarkdownFileName) do
        if line.StartsWith refMarker then
            let refName = line.Replace(refMarker, "").Trim()
            match Map.tryFind refName refs with
            | Some value ->
                yield! [ "```fsharp"; value; "```" ]
            | None -> failwithf "Ref not found: %s" refName
        else
            yield line
    |]
    |> String.concat "\n"

File.WriteAllText(outputFileName, renderedMarkdown)
