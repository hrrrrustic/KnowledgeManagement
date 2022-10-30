module KnowledgeManagement.TrainingModel

open KnowledgeManagement.Model
open Microsoft.ML
open Microsoft.ML.Data

module private Training =
    let ml = MLContext()
    let dummy = Unchecked.defaultof<DatasetModel>
    let data = ml.Data.LoadFromEnumerable<DatasetModel>(IO.readAnnotatedIssues())
    let split = ml.Data.TrainTestSplit(data, 0.2)
    let trainer = ml.BinaryClassification.Trainers.FastTree(labelColumnName = "ClassificationLabel", featureColumnName = "Features")
    let dataProcessPipeline = 
        EstimatorChain()
            .Append(ml.Transforms.Text.FeaturizeText("TitleFeaturized", nameof(dummy.Title)))
            .Append(ml.Transforms.Text.FeaturizeText("DescriptionFeaturized", nameof(dummy.Description)))
            .Append(ml.Transforms.Text.FeaturizeText("AuthorFeaturized", nameof(dummy.Author)))
            .Append(ml.Transforms.Text.FeaturizeText("LabelsFeaturized", nameof(dummy.Labels)))
            .Append(ml.Transforms.Concatenate("Features", "TitleFeaturized", "DescriptionFeaturized", "AuthorFeaturized", "LabelsFeaturized"))
            .Append(trainer)

open Training

let trainedModel = dataProcessPipeline.Fit(split.TrainSet)
let predictions = trainedModel.Transform(split.TestSet)
let metrics = ml.BinaryClassification.Evaluate(data = predictions, labelColumnName = "ClassificationLabel", scoreColumnName = "Score")
let predEngine = ml.Model.CreatePredictionEngine<DatasetModel, PredictionResult>(trainedModel)