module KnowledgeManagement.Extensions

open System
open System.Collections.Generic
open System.Threading.Tasks

module Option =
    let ofString str = if String.IsNullOrWhiteSpace str then None else Some str
    
module Task =
    let runSynchronously (task: Task<'T>) = task.GetAwaiter().GetResult() 