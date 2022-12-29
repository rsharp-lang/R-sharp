Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime

''' <summary>
''' .NET CLR tools
''' </summary>
<Package("CLR")>
Public Module CLRTool

    <ExportAPI("assembly")>
    Public Function LoadAssembly(pstr As String, Optional env As Environment = Nothing) As Object
        If pstr.FileExists Then
            Return Assembly.LoadFile(pstr.GetFullPath)
        ElseIf pstr.isFilePath(includeWindowsFs:=True) Then
            Return Internal.debug.stop({
                $".NET assembly is not exists on the given file location: '{pstr}'",
                $"Full_path: {pstr.GetFullPath}"
            }, env)
        Else
            Return Assembly.Load(pstr)
        End If
    End Function
End Module
