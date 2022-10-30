module KnowledgeManagement.IO

open System.Globalization
open System.IO
open CsvHelper
open CsvHelper.Configuration
open KnowledgeManagement.Model

let rawDatasetFileName = "Dataset.csv"
let annotatedDatasetFileName = "AnnotatedDataset.csv"
let isRawExists = File.Exists rawDatasetFileName

let saveAllIssuesToFile filePath (issues: CsvModel seq) =
    use writer = new StreamWriter(filePath, append = isRawExists)
    let csvConfig =
        let config = CsvConfiguration(CultureInfo.InvariantCulture)
        config.HasHeaderRecord <- not isRawExists
        config
    
    use csvWriter = new CsvWriter(writer, csvConfig)
    csvWriter.WriteRecords issues

let saveDatasetIssuesToFile filePath issues =
    issues
    |> Seq.map DatasetModel.toCsv
    |> saveAllIssuesToFile filePath

let readIssues (fileName: string) =
    use writer = new StreamReader(fileName)
    let csvConfig =
        let config = CsvConfiguration(CultureInfo.InvariantCulture)
        config.HasHeaderRecord <- true
        config
    use csvReader = new CsvReader(writer, csvConfig)
    
    csvReader.GetRecords<CsvModel>()
    |> Seq.map DatasetModel.fromCsv
    |> Array.ofSeq

let readRawIssues() = readIssues rawDatasetFileName
let readAnnotatedIssues() = readIssues annotatedDatasetFileName