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
        | Some code -> "<details class=\"block-source-details\"><summary>Source</summary><pre class=\"block-source\">" + esc code + "</pre></details>"
        | None -> ""
    "<div class=\"block-card\">"
    + "<h3>" + esc block.Name + "</h3>"
    + "<p class=\"block-desc\">" + esc block.Description + "</p>"
    + "<code class=\"block-snippet\">" + esc block.CodeSnippet + "</code>"
    + chart
    + source
    + "</div>"

let private navLink (cat: Category) =
    "<a class=\"nav-link\" href=\"#/category/" + cat.Slug + "\">"
    + "<span class=\"nav-link-name\">" + esc cat.Name + "</span>"
    + "<span class=\"nav-link-count\">" + string cat.Blocks.Length + "</span>"
    + "</a>"

let private navMenu () =
    let links = categories |> List.map navLink |> String.concat ""
    "<div class=\"nav-menu\">"
    + "<div class=\"nav-title\"><a href=\"#\">LocSta2</a></div>"
    + "<nav>" + links + "</nav>"
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

    + "<h2>The Local State Pattern</h2>"
    + "<p>Traditional stateful signal processing requires you to manually thread state through your code, "
    + "allocate buffers, and manage lifecycles. LocSta2 eliminates this boilerplate.</p>"
    + "<p>Each block is a function of type "
    + "<code class=\"inline-code\">StateController&lt;'s&gt; &rarr; 'ctx &rarr; 'value &times; StateController&lt;'s&gt;</code>. "
    + "The <code class=\"inline-code\">stream { }</code> computation expression threads state automatically. "
    + "Blocks like <code class=\"inline-code\">useState</code>, <code class=\"inline-code\">useMemo</code>, and "
    + "<code class=\"inline-code\">getCtx</code> let you declare local state inside the computation &mdash; "
    + "state that persists across evaluations without any external bookkeeping.</p>"
    + "<p>This means a low-pass filter, an EMA, or an ADSR envelope are each just a few lines of pure F#, "
    + "and they compose freely:</p>"
    + "<pre class=\"code-example\">"
    + "let smoothed = stream {\n"
    + "    let! input = getCtx()\n"
    + "    let! prev  = useState 0.0\n"
    + "    let  value  = prev.Value * 0.9 + input * 0.1\n"
    + "    prev.Value &lt;- value\n"
    + "    return value\n"
    + "}\n"
    + "\n"
    + "// Evaluate with a list of inputs:\n"
    + "smoothed |&gt; Eval.runWith [1.0; 2.0; 3.0; ...]"
    + "</pre>"

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

let renderApp (hash: string) =
    let content =
        let parts = hash.TrimStart('#').TrimStart('/').Split('/') |> Array.toList
        match parts with
        | [ "category"; slug ] -> categoryPage slug
        | _ -> homePage ()
    "<div class=\"page\">"
    + "<div class=\"sidebar\">" + navMenu () + "</div>"
    + "<main class=\"main-content\">" + content + "</main>"
    + "</div>"
