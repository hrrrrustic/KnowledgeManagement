module KnowledgeManagement.DatasetDumping

open System
open System.Collections.Generic
open System.Text.RegularExpressions
open Octokit
open KnowledgeManagement.Model

let githubClient =
    let client = GitHubClient(ProductHeaderValue("KnowledgeManagement"))
    client.Credentials <- Credentials("TokenHere")
    client

let rgx = Regex("[^a-zA-Z0-9 -]", RegexOptions.Compiled);

let mapIssueToModel (issue: Issue) =
    let upcastLabels (labels: IReadOnlyList<Label>) = labels :> seq<Label>
    let labelName (label: Label) = label.Name
    
    {
        IssueNumber = issue.Number
        Labels = issue.Labels |> upcastLabels |> Seq.map labelName |> Array.ofSeq
        Title = rgx.Replace(issue.Title, " ")
        Description = if isNull issue.Body then "" else rgx.Replace(issue.Body, " ")
        Author = issue.User.Login
        ClassificationLabel = false
    }

let excelMaxCellValue = 32767

let getOptions (count: int) =
    let pageCount = (count / 100)
    ApiOptions(PageSize = 100, PageCount = pageCount)
    
let getIssues count  = task {
    let issueIsNotPr (issue: Issue) = isNull issue.PullRequest
    let issueDescriptionIsSmall (issue: Issue) = isNull issue.Body || issue.Body.Length < excelMaxCellValue
    
    let apiOptions = getOptions count
    let! issues = githubClient.Issue.GetAllForRepository("dotnet", "runtime", apiOptions)
    return issues
           |> Seq.filter issueIsNotPr
           |> Seq.filter issueDescriptionIsSmall
           |> Array.ofSeq
}