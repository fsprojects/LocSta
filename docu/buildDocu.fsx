open System
open System.IO

let root = __SOURCE_DIRECTORY__
let contentDir = root + "/content"
let outputFileName = "../Readme.md"
let inputFileName = contentDir + "/docu.fsx"

type LineType =
    | BeginDocument
    | Markdown
    | Code
    | StartMarkdown
    | StartCode

let lines =
    [ let mutable state = BeginDocument
      for line in File.ReadLines inputFileName do
          match line.Trim() with
          | "(*" ->
              match state with
              | BeginDocument ->
                  state <- StartMarkdown
              | Code ->
                  state <- StartMarkdown
                  yield "```"
                  yield ""
              | x -> failwithf "Detected open comment while being in %A" x
          | "*)" ->
              match state with
              | Markdown ->
                  state <- StartCode
                  yield ""
                  yield "```fsharp"
              | x -> failwithf "Detected closing comment while being in %A" x
          | "" ->
              match state with
              | Markdown
              | Code ->
                  yield line
              | BeginDocument
              | StartMarkdown
              | StartCode ->
                  ()
          | _ ->
              match state with
              | BeginDocument ->
                  failwith "Expected empty line of open comment at beginning of document."
              | Markdown
              | Code ->
                  yield line
              | StartMarkdown ->
                  state <- Markdown
                  yield line
              | StartCode ->
                  state <- Code
                  yield line ]

let lastAndCurrentLine =
    (lines @ [""])
    |> List.zip ("" :: lines)

let renderedMarkdown =
    [ for last,curr in lastAndCurrentLine do
          match last.Trim(), curr.Trim() with
          | "", "```" -> ()
          | _ -> last ]
    |> String.concat "\n"

File.WriteAllText(outputFileName, renderedMarkdown)
