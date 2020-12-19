Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.System.Configuration

Namespace System.Package.File

    Public Module PackageLoader2

        <Extension>
        Public Function GetPackageDirectory(opt As Options, packageName$) As String
            Dim libDir As String

            libDir = opt.lib & $"/Library/{packageName}"
            libDir = libDir.GetDirectoryFullPath

            Return libDir
        End Function

        ''' <summary>
        ''' attach installed package
        ''' </summary>
        ''' <param name="dir"></param>
        ''' <param name="env"></param>
        Public Sub LoadPackage(dir As String, env As GlobalEnvironment)
            Dim meta As DESCRIPTION = $"{dir}/index.json".LoadJsonFile(Of DESCRIPTION)
            Dim symbols As Dictionary(Of String, String) = $"{dir}/manifest/symbols.json".LoadJsonFile(Of Dictionary(Of String, String))

            For Each symbol In symbols
                Using bin As New BinaryReader($"{dir}/src/{symbol.Value}".Open)
                    Call BlockReader.Read(bin).Parse(desc:=meta).Evaluate(env)
                End Using
            Next

            Dim onLoad As String = $"{dir}/.onload"

            If onLoad.FileExists Then
                Using bin As New BinaryReader(onLoad.Open)
                    Call DirectCast(BlockReader.Read(bin).Parse(desc:=meta).Evaluate(env), DeclareNewFunction).Invoke(env, params:={})
                End Using
            End If
        End Sub
    End Module
End Namespace