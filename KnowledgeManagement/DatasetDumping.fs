module KnowledgeManagement.DatasetCreation

open Octokit

let githubClient =
    let client = GitHubClient(ProductHeaderValue("KnowledgeManagement"))
    client.Credentials <- Credentials("TokenHere")
    client

let excelMaxCellValue = 32767
let options = ApiOptions(PageSize = 100, PageCount = 2)
let getAllIssues() = task {
    let! issues = githubClient.Issue.GetAllForRepository("dotnet", "runtime", options)
    return issues |> Seq.filter (fun (x: Issue) -> isNull x.PullRequest && x.Body.Length < excelMaxCellValue) |> Array.ofSeq
}