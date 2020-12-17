Imports System.ComponentModel
Imports System.IO
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.System.Package.File
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

<CLI> Module CLI

    <ExportAPI("--build")>
    <Description("build R# package")>
    <Usage("--build /src <folder> [/save <Rpackage.zip>]")>
    <Argument("/src", False, CLITypes.File, PipelineTypes.std_in,
              AcceptTypes:={GetType(String)},
              Description:="A folder path that contains the R source files and meta data files of the target R package, 
              a folder that exists in this folder path which is named 'R' is required!")>
    Public Function Compile(args As CommandLine) As Integer
        Dim src$ = args <= "/src"
        Dim meta As DESCRIPTION = DESCRIPTION.Parse($"{src}/DESCRIPTION")
        Dim save$ = args("/save") Or $"{src}/../{meta.Package}_{meta.Version}.zip"

        Using outputfile As FileStream = save.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
            Dim err As Message = meta.Build(src, outputfile)

            If RProgram.isException(err) Then
                Return CInt(debug.PrintMessageInternal(err, Nothing))
            Else
                Return 0
            End If
        End Using
    End Function

    <ExportAPI("--check")>
    <Usage("--check --target <package.zip>")>
    <Description("Verify a packed R# package is damaged or not?")>
    Public Function Check(args As CommandLine) As Integer
        Dim target As String = args <= "--target"

    End Function
End Module
