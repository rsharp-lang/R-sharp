#Region "Microsoft.VisualBasic::7e132528dc65a3a50ce82b21a184ae31, R-sharp\snowFall\Context\RunParallel.vb"

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


' Code Statistics:

'   Total Lines: 158
'    Code Lines: 131
' Comment Lines: 8
'   Blank Lines: 19
'     File Size: 6.71 KB


' Class RunParallel
' 
'     Properties: [error], debug, debugPort, master, seqSet
'                 size, task, worker
' 
'     Constructor: (+1 Overloads) Sub New
' 
'     Function: getSymbol, Initialize, readSymbolSet, taskFactory
' 
'     Sub: getResult
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine.InteropService.Pipeline
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.CodeAnalysis
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
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
    Public Property seqSet As Dictionary(Of String, Array)
    Public Property size As Integer
    Public ReadOnly Property worker As R
    Public Property task As Byte()
    ''' <summary>
    ''' set port number for slave node bootstrap socket
    ''' </summary>
    ''' <returns></returns>
    Public Property debugPort As Integer = -1
    Public Property debug As Boolean = False
    Public Property slaveDebug As Boolean = False

    Private Sub New()
        worker = R.FromEnvironment(App.HOME)

#If NETCOREAPP Then
        worker.SetDotNetCoreDll()
        worker.dotnetcoreApp = True
#End If
    End Sub

    Public Function getSymbol(symbol As GetSymbol) As (hit As Boolean, val As Object)
        If Not seqSet.ContainsKey(symbol.name) Then
            Return Nothing
        Else
            Return (True, seqSet(symbol.name)(symbol.uuid))
        End If
    End Function

    ''' <summary>
    ''' run task on the remote slave node from this function
    ''' </summary>
    ''' <param name="index"></param>
    ''' <returns></returns>
    Public Function taskFactory(index As Integer) As Object
        Dim result As Object = Nothing
        Dim bootstrap As New BootstrapSocket(index, master.port, Me.task, debugPort, debug:=debug, slave_debug:=slaveDebug)
        Dim tempfile As String = If(debug, TempFileSystem.GetAppSysTempFile(".log", $"{App.PID}.{bootstrap.GetHashCode}.{index}", $"task_{index}___-"), Nothing)
        Dim task As String = worker.GetparallelModeCommandLine(bootstrap.port, [delegate]:="Parallel::slave", redirect_stdout:=tempfile)
        Dim SetDllDirectory As String = master.env.globalEnvironment.options.getOption("SetDllDirectory") Or App.HOME.AsDefault
        Dim process As RunSlavePipeline = worker.CreateSlave($"{task} --SetDllDirectory {SetDllDirectory.CLIPath}")

        If debug Then
            Call base.print(process.ToString,, master.env)
        End If

        Call bootstrap.Run(process)
        Call getResult(uuid:=index, result)

        Return result
    End Function

    Private Sub getResult(uuid As Integer, <Out> ByRef result As Object)
        result = master.pop(uuid)
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="argv">
    ''' there are some additional parameter in this object list that can be config:
    ''' 
    ''' 1. ``debug``: set true for open debug mode
    ''' 2. ``master``: set the tcp port of the master node
    ''' 3. ``bootstrap``: set the bootstrap tcp port of the slave node
    ''' 4. ``slaveDebug``: set this option to pause will make the master node pause when run a new salve node for run debug
    ''' </param>
    ''' <param name="debug"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Public Shared Function Initialize(task As Expression, argv As list, debug As Boolean, env As Environment) As RunParallel
        Dim parallelBase As New MasterContext(
            env:=env,
            verbose:=argv.getValue("debug", env, [default]:=False),
            port:=argv.getValue("master", env, -1)
        )
        Dim seqSet As NamedCollection(Of Object)() = readSymbolSet(task, parallelBase, argv, env).ToArray
        Dim checkSize As Integer() = seqSet _
            .Select(Function(seq) seq.Length) _
            .Where(Function(l) l <> 1) _
            .ToArray

        For Each var As NamedCollection(Of Object) In seqSet
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
                .seqSet = seqSet _
                    .ToDictionary(Function(i) i.name,
                                  Function(i)
                                      Return DirectCast(i.value, Array)
                                  End Function),
                .size = checkSize(Scan0),
                .task = taskPayload.ToArray,
                .debugPort = argv.getValue("bootstrap", env, -1),
                .debug = debug,
                .slaveDebug = argv.getValue("slaveDebug", env, False)
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
