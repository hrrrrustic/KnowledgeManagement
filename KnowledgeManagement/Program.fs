module Program

open System
open KnowledgeManagement
open KnowledgeManagement.DatasetDumping
open KnowledgeManagement.Model
open KnowledgeManagement.Extensions
open Microsoft.ML
open Microsoft.ML.Data

module MainFlow =
    let dumpIssues count =
        getIssues count
        |> Task.runSynchronously
        |> Seq.map mapIssueToModel
        |> IO.saveDatasetIssuesToFile IO.rawDatasetFileName

    let annotateDataset() =
        IO.readRawIssues()
        |> Seq.map (fun x -> x, DatasetAnnotating.annotateModel x)
        |> Seq.map (fun (model, label) -> DatasetModel.classify model label)
        |> IO.saveDatasetIssuesToFile IO.annotatedDatasetFileName
    
MainFlow.dumpIssues 20000
MainFlow.annotateDataset()

let fakeModel: DatasetModel = {
    Labels = [| "api-approved" |]
    Author = "hrrrrustic"
    IssueNumber = 1
    Title = "Int32 doesn't work"
    Description = "fix this"
    ClassificationLabel = true
}

let metrics = TrainingModel.metrics
let test = TrainingModel.predEngine.Predict(fakeModel)
Console.WriteLine()