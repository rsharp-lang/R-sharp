Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.System.Package.File

Module BuildPackagePipeline

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="dir">the R source input directory</param>
    ''' <param name="output">the R package output file path</param>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Function BuildPackage(meta As DESCRIPTION, dir As String, output As String) As Boolean
        Using save As FileStream = output.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False), zip As New ZipArchive(save, ZipArchiveMode.Create)
            Using file As New StreamWriter(zip.CreateEntry("index.json").Open)
                Call file.WriteLine(meta.GetJson)
                Call file.Flush()
            End Using

            Dim loader As New RInterpreter()
            Dim [imports] As Environment = Nothing
            Dim srcR As String = $"{dir}/R".GetDirectoryFullPath

            For Each script As String In srcR.ListFiles("*.R")
                Dim fileKey As String = script _
                    .GetFullPath _
                    .Replace($"{dir}/R".GetDirectoryFullPath, "") _
                    .DoCall(Function(ref) $"src\{ref}") _
                    .Replace("/", "\") _
                    .StringReplace("\\+", "\")

                Call loader.Source(script, globalEnv:=[imports])

                Using file As New StreamWriter(zip.CreateEntry(fileKey).Open)
                    Call file.WriteLine(script.ReadAllText)
                    Call file.Flush()
                End Using
            Next

            Dim symbols As New List(Of String)

            For Each symbol As Symbol In [imports].symbols.Values
                If symbol.constraint = TypeCodes.closure Then
                    symbols.Add(symbol.name)
                End If
            Next

            Using file As New StreamWriter(zip.CreateEntry("symbols.json").Open)
                Call file.WriteLine(symbols.ToArray.GetJson)
                Call file.Flush()
            End Using
        End Using

        Return True
    End Function
End Module
