#Region "Microsoft.VisualBasic::3a0fe71b2fd8966d65582b22f69244c5, snowFall\Context\RunParallel.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:

' Class RunParallel
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.CommandLine.InteropService.Pipeline
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.CodeAnalysis
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports snowFall.Context.RPC
Imports R = Rserver.RscriptCommandLine.Rscript
Imports Rset = SMRUCC.Rsharp.Runtime.Internal.Invokes.set

''' <summary>
''' context_analysis -> symbols -> serialization -> parallel_slave
''' </summary>
Public Class RunParallel

    Public Property [error] As Message
    Public Property master As MasterContext
    Public Property seqSet As NamedCollection(Of Object)()
    Public Property size As Integer
    Public ReadOnly Property worker As R
    Public Property task As Byte()

    Private Sub New()
        worker = R.FromEnvironment(App.HOME)
    End Sub

    ''' <summary>
    ''' run task on the remote slave node from this function
    ''' </summary>
    ''' <param name="index"></param>
    ''' <returns></returns>
    Public Function taskFactory(index As Integer) As Object
        Dim task As String = worker.GetparallelModeCommandLine(master.port, [delegate]:="Parallel::slave")
        Dim process As RunSlavePipeline = worker.CreateSlave(task)
        Dim result As Object = Nothing
        Dim bootstrap As New BootstrapSocket(index, master.port, Me.task)

        Call bootstrap.Run(process)
        Call getResult(uuid:=index, result)

        Return result
    End Function

    Private Sub getResult(uuid As Integer, <Out> ByRef result As Object)
        result = master.pop(uuid)
    End Sub

    Public Shared Function Initialize(task As Expression, argv As list, env As Environment) As RunParallel
        Dim parallelBase As New MasterContext(
            env:=env,
            verbose:=argv.getValue("debug", env, [default]:=False)
        )
        Dim seqSet As NamedCollection(Of Object)() = readSymbolSet(task, parallelBase, argv, env).ToArray
        Dim checkSize As Integer() = seqSet _
            .Select(Function(seq) seq.Length) _
            .Where(Function(l) l <> 1) _
            .ToArray

        For Each var In seqSet
            If var.name.StringEmpty AndAlso var.value.Length = 1 AndAlso TypeOf var(Scan0) Is Message Then
                Return DirectCast(var(Scan0), Message)
            End If
        Next

        Dim taskPayload As New MemoryStream

        Call New Writer(taskPayload).Write(task)
        Call taskPayload.Flush()
        Call taskPayload.Seek(Scan0, SeekOrigin.Begin)

        If checkSize.Distinct.Count <> 1 Then
            Return Internal.debug.stop("the sequence size should be equals to each other!", env)
        Else
            Return New RunParallel With {
                .master = parallelBase,
                .seqSet = seqSet,
                .size = checkSize(Scan0),
                .task = taskPayload.ToArray
            }
        End If
    End Function

    Private Shared Iterator Function readSymbolSet(task As Expression,
                                                   parallelBase As MasterContext,
                                                   argv As list,
                                                   env As Environment) As IEnumerable(Of NamedCollection(Of Object))

        Dim allSymbols = SymbolAnalysis.GetSymbolReferenceList(task).ToArray
        Dim value As Object
        Dim locals As Index(Of String) = allSymbols _
            .Where(Function(x) x.Description <> "global") _
            .Select(Function(x) x.Name) _
            .Distinct _
            .Indexing
        Dim required = allSymbols _
            .Where(Function(v) v.Description = "global") _
            .Where(Function(v)
                       Return v.Value = PropertyAccess.Readable OrElse v.Value = PropertyAccess.ReadWrite
                   End Function) _
            .GroupBy(Function(v) v.Name) _
            .Select(Function(v) v.First) _
            .ToArray
        Dim seq As IEnumerable(Of Object)

        For Each symbol As NamedValue(Of PropertyAccess) In required
            If Not argv.hasName(symbol.Name) Then
                If Not env.FindFunction(symbol.Name) Is Nothing Then
                    parallelBase.push(symbol.Name, env.FindFunction(symbol.Name).value)
                    Continue For
                ElseIf Not env.FindSymbol(symbol.Name) Is Nothing Then
                    parallelBase.push(symbol.Name, env.FindSymbol(symbol.Name).value)
                    Continue For
                ElseIf Internal.invoke.getFunction(symbol.Name) IsNot Nothing Then
                    Continue For
                ElseIf symbol.Name Like locals Then
                    Continue For
                End If

                Yield New NamedCollection(Of Object)(Nothing, {Message.SymbolNotFound(env, symbol.Name, TypeCodes.ref)})
            Else
                value = argv.getByName(symbol.Name)
                seq = Rset.getObjectSet(value, env)

                Yield New NamedCollection(Of Object)(symbol.Name, seq)
            End If
        Next
    End Function

    Public Shared Widening Operator CType(err As Message) As RunParallel
        Return New RunParallel With {.[error] = err}
    End Operator
End Class
