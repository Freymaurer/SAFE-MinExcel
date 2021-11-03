module Index

open Elmish
open Fable.Remoting.Client
open Shared

open ExcelJS.Fable.GlobalBindings

type Model = {
    Todos: Todo list;
    Input: string
    ExcelIsInit: bool
}

let initExcel () = Office.onReady()

type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo
    | Initialized of string * string
    | ExcelHelloWorldRequest
    | ExcelHelloWorldResponse of unit

let todosApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>

let init () : Model * Cmd<Msg> =
    let model = { Todos = []; Input = ""; ExcelIsInit = false }
    let initExcel =
        Cmd.OfPromise.perform
            initExcel
            ()
            (fun x -> (x.host.ToString(),x.platform.ToString()) |> Initialized)

    let cmd =
        Cmd.OfAsync.perform todosApi.getTodos () GotTodos

    model, Cmd.batch [initExcel; cmd]

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | GotTodos todos -> { model with Todos = todos }, Cmd.none
    | SetInput value -> { model with Input = value }, Cmd.none
    | Initialized (h,p) ->
        let welcomeMsg = sprintf "Ready to go in %s running on %s" h p
        Fable.Core.JS.console.log welcomeMsg
        let nextModel = {
            model with
                ExcelIsInit = true
        }
        nextModel, Cmd.none
    | ExcelHelloWorldRequest ->
        Fable.Core.JS.console.log "Start Request!"
        let cmd =
            Cmd.OfPromise.perform
                ExcelInterop.insertHelloWorld
                ()
                ExcelHelloWorldResponse
        model, cmd
    | ExcelHelloWorldResponse () ->
        Fable.Core.JS.console.log "Success!"
        model, Cmd.none
    | AddTodo ->
        let todo = Todo.create model.Input

        let cmd =
            Cmd.OfAsync.perform todosApi.addTodo todo AddedTodo

        { model with Input = "" }, cmd
    | AddedTodo todo ->
        { model with
              Todos = model.Todos @ [ todo ] },
        Cmd.none

open Feliz
open Feliz.Bulma

let navBrand =
    Bulma.navbarBrand.div [
        Bulma.navbarItem.a [
            prop.href "https://safe-stack.github.io/"
            navbarItem.isActive
            prop.children [
                Html.img [
                    prop.src "/favicon.png"
                    prop.alt "Logo"
                ]
            ]
        ]
    ]

let containerBox (model: Model) (dispatch: Msg -> unit) =
    Bulma.box [
        Bulma.content [
            Html.ol [
                for todo in model.Todos do
                    Html.li [ prop.text todo.Description ]
            ]
        ]
        Bulma.field.div [
            field.isGrouped
            prop.children [
                Bulma.control.p [
                    control.isExpanded
                    prop.children [
                        Bulma.input.text [
                            prop.value model.Input
                            prop.placeholder "What needs to be done?"
                            prop.onChange (fun x -> SetInput x |> dispatch)
                        ]
                    ]
                ]
                Bulma.control.p [
                    Bulma.button.a [
                        color.isPrimary
                        prop.disabled (Todo.isValid model.Input |> not)
                        prop.onClick (fun _ -> dispatch AddTodo)
                        prop.text "Add"
                    ]
                ]
            ]
        ]
    ]

let interopBox (model: Model) (dispatch: Msg -> unit) =
    Bulma.box [
        Bulma.field.div [
            field.hasAddons
            prop.children [
                Bulma.control.div [
                    Bulma.button.a [
                        button.isStatic
                        prop.children [
                            Html.div [ prop.text "Is connected to Excel: "]
                            Html.div [
                                prop.style [
                                    style.fontWeight 700
                                    style.color (if model.ExcelIsInit then "#00d1b2" else "red")
                                    style.marginLeft (length.rem 0.5)
                                ]
                                prop.text $" {model.ExcelIsInit}"
                            ]
                        ]

                    ]
                ]
                Bulma.control.div [
                    prop.style [style.flexGrow 1]
                    prop.children [
                        Bulma.button.a [
                            button.isFullWidth
                            color.isPrimary
                            prop.onClick (fun _ -> dispatch ExcelHelloWorldRequest)
                            prop.text "Get a good day message in excel!"
                        ]
                    ]
                ]
            ]
        ]
    ]

let view (model: Model) (dispatch: Msg -> unit) =
    Bulma.hero [
        hero.isFullHeight
        color.isPrimary
        prop.style [
            style.backgroundSize "cover"
            style.backgroundImageUrl "https://unsplash.it/1200/900?random"
            style.backgroundPosition "no-repeat center center fixed"
        ]
        prop.children [
            Bulma.heroHead [
                Bulma.navbar [
                    Bulma.container [ navBrand ]
                ]
            ]
            Bulma.heroBody [
                Bulma.container [
                    Bulma.column [
                        column.is6
                        column.isOffset3
                        prop.children [
                            Bulma.title [
                                text.hasTextCentered
                                prop.text "NewSAFE_"
                            ]
                            containerBox model dispatch
                            interopBox model dispatch
                        ]
                    ]
                ]
            ]
        ]
    ]
