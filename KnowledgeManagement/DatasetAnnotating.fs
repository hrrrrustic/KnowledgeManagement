module KnowledgeManagement.DatasetAnnotating

open System
open KnowledgeManagement.Model

type AnnotationProgress =
    | Start of DatasetModel
    | Done of bool

let (>=>) x f =
    match x with
    | Start datasetModel -> f datasetModel
    | Done _ -> x

module AnnotationProgress =
    let ofBool value f x =
        match f x with
        | true -> Done value
        | false -> Start x
    let ofPositiveBool = ofBool true
    let ofNegativeBool = ofBool false
    
    let defaultValue value x =
        match x with
        | Start _ -> value
        | Done label -> label

let annotationFolder folder acc value = acc >=> folder value
let annotateBy folder model values = Seq.fold (annotationFolder folder) (Start model) values

module LabelAnnotation =
    let isLabeled = DatasetModel.containsLabel >> AnnotationProgress.ofPositiveBool

    let positiveLabels =
        [|
            "api-approved"
            "api-ready-for-review"
        |]

    let negativeLabels =
        [|
            "api-suggestion"
            "api-needs-work"
        |]

    let annotateByPositiveLabel model = annotateBy isLabeled model positiveLabels
    let annotateByNegativeLabel model = annotateBy isLabeled model negativeLabels
    
    let annotate model =
        model
        |> Start
        >=> annotateByPositiveLabel
        >=> annotateByPositiveLabel

module AuthorAnnotation =
    let isFromPositive = DatasetModel.createdBy >> AnnotationProgress.ofPositiveBool
    let isFromNegative = DatasetModel.createdBy >> AnnotationProgress.ofNegativeBool

    let positiveUsernames =
        [|
            "StephenToub"
            "hrrrrustic"
        |]

    let negativeUserNames =
        [|
            "EgorBo"
        |]

    let annotateByPositive model = annotateBy isFromPositive model positiveUsernames
    let annotateByNegative model = annotateBy isFromNegative model negativeUserNames

    let annotate model =
        model
        |> Start 
        >=> annotateByNegative
        >=> annotateByPositive

let stopWords =
    [|
        "jit"
        "wasm"
        "gc"
        "nativeaot"
        "mono"
        "pgo"
    |]

module TitleAnnotation =
    let containsStopWord = DatasetModel.titleHas >> AnnotationProgress.ofNegativeBool
    
    let annotateByNegative model = annotateBy containsStopWord model stopWords
    
    let annotate model =
        model
        |> Start
        >=> annotateByNegative

module DescriptionAnnotation =
    let containsStopWord = DatasetModel.descriptionHas >> AnnotationProgress.ofNegativeBool
    
    let annotateByNegative model = annotateBy containsStopWord model stopWords
    
    let annotate model =
        model
        |> Start
        >=> annotateByNegative

module RandomAnnotation =
    let getRandBool() = Random.Shared.Next(0, 2) > 0
    let random value model = if getRandBool() then Done value else Start model
    let positiveRandom = random true
    let negativeRandom = random false
    let annotate model =
        model
        |> Start
        >=> positiveRandom
        >=> negativeRandom
let annotateModel model =
    Start model
    >=> LabelAnnotation.annotate
    >=> AuthorAnnotation.annotate
    >=> TitleAnnotation.annotate
    >=> DescriptionAnnotation.annotate
    >=> RandomAnnotation.annotate
    |> AnnotationProgress.defaultValue false