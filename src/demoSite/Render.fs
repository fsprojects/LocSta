module DemoSite.Render

open DemoSite.Types
open DemoSite.SvgChart
open DemoSite.BlockDemos

let private esc (s: string) =
    s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;")

let private blockCard (block: BlockDemo) =
    let chart = renderChart (block.InputSeries @ block.OutputSeries)
    let source =
        match BlockSources.sources |> Map.tryFind block.Name with
        | Some code -> "<details class=\"block-source-details\"><summary>Source for the " + esc block.Name + " block</summary><pre class=\"block-source\">" + esc code + "</pre></details>"
        | None -> ""
    let desc =
        BlockSources.descriptions
        |> Map.tryFind block.Name
        |> Option.defaultValue block.Description
    "<div class=\"block-card\">"
    + "<h3>" + esc block.Name + "</h3>"
    + "<p class=\"block-desc\">" + esc desc + "</p>"
    + "<code class=\"block-snippet\">" + esc block.CodeSnippet + "</code>"
    + chart
    + source
    + "</div>"

let private navBlockLink (slug: string) (block: BlockDemo) =
    let thumb = renderThumb (block.InputSeries @ block.OutputSeries)
    "<a class=\"nav-block\" href=\"#/category/" + slug + "\">"
    + "<span class=\"nav-block-name\">" + esc block.Name + "</span>"
    + thumb
    + "</a>"

let private navGroup (cat: Category) =
    let blocks = cat.Blocks |> List.map (navBlockLink cat.Slug) |> String.concat ""
    "<div class=\"nav-group\">"
    + "<div class=\"nav-group-title\">" + esc cat.Name + "</div>"
    + blocks
    + "</div>"

let private navMenu () =
    let groups = categories |> List.map navGroup |> String.concat ""
    "<div class=\"nav-menu\">"
    + "<div class=\"nav-title\"><a href=\"#\">LocSta2</a></div>"
    + "<nav>" + groups + "</nav>"
    + "</div>"

let private categoryCard (cat: Category) =
    "<a class=\"category-card\" href=\"#/category/" + cat.Slug + "\">"
    + "<span class=\"category-card-name\">" + esc cat.Name + "</span>"
    + "<span class=\"category-card-count\">" + string cat.Blocks.Length + " blocks</span>"
    + "</a>"

let private homePage () =
    let totalBlocks = categories |> List.sumBy (fun c -> c.Blocks.Length)
    let cards = categories |> List.map categoryCard |> String.concat ""
    "<h1>LocSta2</h1>"
    + "<p class=\"intro\">A stateful signal processing library for F#.</p>"

    + "<div class=\"about\">"
    + "<h2>What is LocSta2?</h2>"
    + "<p>LocSta2 (<em>Local State</em>) is an F# library for building signal processing pipelines "
    + "where each block automatically retains its own local state between evaluations. "
    + "It provides " + string totalBlocks + " composable blocks across " + string categories.Length + " categories &mdash; "
    + "from simple delays and filters to oscillators, envelope generators, and dynamics processors.</p>"

    + "<h2>Why Local State?</h2>"
    + "<p>In traditional OOP, every stateful processor needs a class with fields, a constructor, "
    + "and manual wiring. Composing them means instantiating each object upfront, then plumbing "
    + "calls through in the right order. Here is a smoothing filter followed by a difference detector:</p>"

    + "<div class=\"comparison\">"
    + "<div class=\"comparison-col\">"
    + "<div class=\"comparison-label comparison-label-bad\">C# &mdash; Traditional</div>"
    + "<pre class=\"code-example\">"
    + "public class Smoother {\n"
    + "    private double _prev = 0.0;\n"
    + "    public double Process(double input) {\n"
    + "        var v = _prev * 0.9 + input * 0.1;\n"
    + "        _prev = v;\n"
    + "        return v;\n"
    + "    }\n"
    + "}\n"
    + "\n"
    + "public class Differ {\n"
    + "    private double _prev;\n"
    + "    public Differ(double init) =&gt; _prev = init;\n"
    + "    public double Process(double input) {\n"
    + "        var d = input - _prev;\n"
    + "        _prev = input;\n"
    + "        return d;\n"
    + "    }\n"
    + "}\n"
    + "\n"
    + "// Compose: instantiate, wire, call in order\n"
    + "var smoother = new Smoother();\n"
    + "var differ   = new Differ(0.0);\n"
    + "\n"
    + "var results = inputs\n"
    + "    .Select(x =&gt; differ.Process(\n"
    + "                    smoother.Process(x)))\n"
    + "    .ToList();"
    + "</pre>"
    + "</div>"

    + "<div class=\"comparison-col\">"
    + "<div class=\"comparison-label comparison-label-good\">F# &mdash; LocSta2</div>"
    + "<pre class=\"code-example\">"
    + "let smoothed = stream {\n"
    + "    let! input = getCtx()\n"
    + "    let! prev  = useState 0.0\n"
    + "    let  v     = prev.Value * 0.9 + input * 0.1\n"
    + "    prev.Value &lt;- v\n"
    + "    return v\n"
    + "}\n"
    + "\n"
    + "let smoothedDiff = stream {\n"
    + "    let! v = smoothed\n"
    + "    return! diff 0.0\n"
    + "}\n"
    + "\n"
    + "// Compose: just bind. No objects.\n"
    + "smoothedDiff |&gt; Eval.runWith inputs"
    + "</pre>"
    + "</div>"
    + "</div>"

    + "<p>With LocSta2, each block declares its own local state via "
    + "<code class=\"inline-code\">useState</code> inside a "
    + "<code class=\"inline-code\">stream { }</code> computation expression. "
    + "No classes, no constructors, no manual wiring. "
    + "State is allocated on first use, retained across evaluations, and composed with "
    + "<code class=\"inline-code\">let!</code> &mdash; "
    + "the runtime threads it automatically.</p>"

    + "<h2>Explore the Blocks</h2>"
    + "<p>Each demo below shows input (blue) and output (red) signals. "
    + "Click a category to see all its blocks with live-rendered SVG charts.</p>"
    + "</div>"
    + "<div class=\"category-grid\">" + cards + "</div>"

let private categoryPage (slug: string) =
    match categories |> List.tryFind (fun c -> c.Slug = slug) with
    | Some cat ->
        let blocks = cat.Blocks |> List.map blockCard |> String.concat ""
        "<h1>" + esc cat.Name + "</h1>" + blocks
    | None ->
        "<h1>Category not found</h1>"

let renderShell () =
    "<div class=\"page\">"
    + "<div class=\"sidebar\">" + navMenu () + "</div>"
    + "<main class=\"main-content\" id=\"content\"></main>"
    + "</div>"

let renderContent (hash: string) =
    let parts = hash.TrimStart('#').TrimStart('/').Split('/') |> Array.toList
    match parts with
    | [ "category"; slug ] -> categoryPage slug
    | _ -> homePage ()
