Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.IO
Imports System.Net.Sockets
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Flute.Http.Core
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.System.Configuration

Public Class RSession : Inherits HttpServer

    ReadOnly workspace$

    Public Sub New(port As Integer, Optional workspace$ = "./")
        Call MyBase.New(port, 1)

        Me.workspace = workspace
    End Sub

    Public Overrides Sub handleGETRequest(p As HttpProcessor)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub handlePOSTRequest(p As HttpProcessor, inputData As String)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub handleOtherMethod(p As HttpProcessor)
        Throw New NotImplementedException()
    End Sub

    Protected Overrides Function getHttpProcessor(client As TcpClient, bufferSize As Integer) As HttpProcessor
        Throw New NotImplementedException()
    End Function
End Class
