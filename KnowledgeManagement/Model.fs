module KnowledgeManagement.Model

open System
open CsvHelper.Configuration.Attributes
open Microsoft.FSharp.Collections
open Microsoft.ML.Data

[<CLIMutable>]
type CsvModel = {
    [<Index(0)>]
    IssueNumber: int
    [<Index(1)>]
    Title: string
    [<Index(2)>]
    Author: string
    [<Index(3)>]
    Labels: string
    [<Index(4)>]
    Description: string
    [<Index(5)>]
    ClassificationLabel: bool
}

type DatasetModel = {
    [<LoadColumn(0)>]
    IssueNumber: int
    [<LoadColumn(1)>]
    Title: string
    [<LoadColumn(2)>]
    Author: string
    [<LoadColumn(3)>]
    Labels: string array
    [<LoadColumn(4)>]
    Description: string
    [<LoadColumn(5)>]
    ClassificationLabel: bool
}

[<CLIMutable>]
type PredictionResult = {
    Label: bool
    Probability: float32
}

module DatasetModel =
    let fromCsv (model: CsvModel) =
        {
            IssueNumber = model.IssueNumber
            Title = model.Title
            Author = model.Author
            Labels = model.Labels.Trim('[', ']').Split(',', StringSplitOptions.TrimEntries) |> Array.ofSeq
            Description = model.Description
            ClassificationLabel = model.ClassificationLabel
        }
    
    let toCsv (model: DatasetModel): CsvModel =
        {
            IssueNumber = model.IssueNumber
            Title = model.Title
            Author = model.Author
            Labels = $"[{String.Join(',', model.Labels)}]"
            Description = model.Description
            ClassificationLabel = model.ClassificationLabel
        }
    
    let classify (model: DatasetModel) label = {model with ClassificationLabel = label}
    let containsLabel label model = Array.contains label model.Labels
    let createdBy author model = model.Author.Equals(author, StringComparison.OrdinalIgnoreCase)
    let titleHas (word: string) model = model.Title.Contains(word, StringComparison.OrdinalIgnoreCase)
    let descriptionHas (word: string) model = model.Description.Contains(word, StringComparison.OrdinalIgnoreCase)