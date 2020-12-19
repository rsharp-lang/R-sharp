Imports System.IO
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime

Namespace System.Package.File

    Public Module PackageLoader2

        Public Function LoadPackage(dir As String, env As GlobalEnvironment)
            Dim meta As DESCRIPTION = $"{dir}/index.json".LoadJsonFile(Of DESCRIPTION)
            Dim symbols As Dictionary(Of String, String) = $"{dir}/manifest/symbols.json".LoadJsonFile(Of Dictionary(Of String, String))

            For Each symbol In symbols
                Using bin As New BinaryReader($"{dir}/src/{symbol.Value}".Open)
                    Dim symbolObj As Expression = BlockReader.Read(bin).Parse(desc:=meta)
                    Dim result As Object = symbolObj.Evaluate(env)
                End Using
            Next
        End Function
    End Module
End Namespace