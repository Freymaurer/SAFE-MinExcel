module ExcelInterop

open ExcelJS
open Fable.Core
open ExcelJS.Fable
open Excel
open GlobalBindings


// For more example see the Swate repository: https://github.com/nfdi4plants/Swate/blob/developer/src/Client/OfficeInterop/OfficeInterop.fs and neighboring files.

let insertHelloWorld () =
    Excel.run(fun context ->

        let worksheet = context.workbook.worksheets.getActiveWorksheet()

        let range = worksheet.getCell(1.,1.)

        context.sync().``then``(fun _ ->
            let customValue = ResizeArray [|
                ResizeArray [|
                    Some (box "Hello World!")
                |]
            |]

            range.values <- customValue

            if Office.context.requirements.isSetSupported("ExcelApi", "1.2") then
                range.format.autofitColumns()
        )
    )