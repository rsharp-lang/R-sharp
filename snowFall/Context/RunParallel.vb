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

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.CodeAnalysis
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports snowFall.Context.RPC
Imports Rset = SMRUCC.Rsharp.Runtime.Internal.Invokes.set
Imports Rscript = Rserver.RscriptCommandLine.Rscript

''' <summary>
''' context_analysis -> symbols -> serialization -> parallel_slave
''' </summary>
Public Class RunParallel

    Public Property [error] As Message
    Public Property master As MasterContext
    Public Property seqSet As NamedCollection(Of Object)()
    Public Property size As Integer
    Public Property worker As Rscript

    Private Sub New()
        worker = Rscript.FromEnvironment(App.HOME)
    End Sub

    ''' <summary>
    ''' run task on the remote slave node from this function
    ''' </summary>
    ''' <param name="index"></param>
    ''' <returns></returns>
    Public Function taskFactory(index As Integer) As Object
        Dim task As String = worker.GetparallelModeCommandLine(master.port, [delegate]:="Parallel::slave")
    End Function

    Public Shared Function Initialize(task As Expression, argv As list, env As Environment) As RunParallel
        Dim allSymbols = SymbolAnalysis.GetSymbolReferenceList(task).ToArray
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
        Dim seqSet As New List(Of NamedCollection(Of Object))
        Dim value As Object
        Dim parallelBase As New MasterContext(env, verbose:=argv.getValue("debug", env, [default]:=False))

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

                Return Message.SymbolNotFound(env, symbol.Name, TypeCodes.ref)
            Else
                value = argv.getByName(symbol.Name)
                seqSet.Add(New NamedCollection(Of Object)(symbol.Name, Rset.getObjectSet(value, env)))
            End If
        Next

        Dim checkSize As Integer() = seqSet _
            .Select(Function(seq) seq.Length) _
            .Where(Function(l) l <> 1) _
            .ToArray

        If checkSize.Distinct.Count <> 1 Then
            Return Internal.debug.stop("the sequence size should be equals to each other!", env)
        Else
            Return New RunParallel With {
                .master = parallelBase,
                .seqSet = seqSet.ToArray,
                .size = checkSize(Scan0)
            }
        End If
    End Function

    Public Shared Widening Operator CType(err As Message) As RunParallel
        Return New RunParallel With {.[error] = err}
    End Operator
End Class
