module DemoSite.Types

type ChartSeries =
    { Label: string
      Values: float list
      Color: string
      IsStep: bool }

type BlockDemo =
    { Name: string
      Description: string
      CodeSnippet: string
      InputSeries: ChartSeries list
      OutputSeries: ChartSeries list }

type Category =
    { Name: string
      Slug: string
      Blocks: BlockDemo list }
