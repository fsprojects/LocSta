namespace DemoSite.Data

open System.Collections.Generic

type ChartSeries =
    { Label: string
      Values: List<float>
      Color: string
      IsStep: bool }

type BlockDemo =
    { Name: string
      Description: string
      CodeSnippet: string
      InputSeries: List<ChartSeries>
      OutputSeries: List<ChartSeries> }

type Category =
    { Name: string
      Slug: string
      Blocks: List<BlockDemo> }
