#Region "Microsoft.VisualBasic::aea0a9fa894659ae34238d64532f2ce5, R#\Runtime\Internal\printer\printer.vb"

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

'     Module printer
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: ToString, ValueToString
' 
'         Sub: AttachConsoleFormatter, printArray, printInternal, printList
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports SMRUCC.Rsharp.Runtime.Components.Configuration

Namespace Runtime.Internal.ConsolePrinter

    Public Delegate Function InternalToString(env As GlobalEnvironment) As IStringBuilder

    ''' <summary>
    ''' R# console nice print supports.
    ''' </summary>
    Public Module printer

        Friend ReadOnly RtoString As New Dictionary(Of Type, IStringBuilder)
        Friend ReadOnly RInternalToString As New Dictionary(Of Type, InternalToString)

        Sub New()
            RtoString(GetType(Color)) = Function(c) DirectCast(c, Color).ToHtmlColor.ToLower
            RtoString(GetType(vbObject)) = Function(o) DirectCast(o, vbObject).ToString

            RInternalToString(GetType(Double)) = AddressOf printer.f64_InternalToString
        End Sub

        Private Function f64_InternalToString(env As GlobalEnvironment) As IStringBuilder
            Dim opts As Options = env.global.options
            Dim format As String = $"{opts.f64Format}{opts.digits}"

            Return Function(d) DirectCast(d, Double).ToString(format)
        End Function

        ''' <summary>
        ''' <see cref="Object"/> -> <see cref="String"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="formatter"></param>
        Public Sub AttachConsoleFormatter(Of T)(formatter As IStringBuilder)
            RtoString(GetType(T)) = formatter
        End Sub

        ''' <summary>
        ''' <see cref="Object"/> -> <see cref="String"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="formatter"></param>
        Friend Sub AttachInternalConsoleFormatter(Of T)(formatter As InternalToString)
            RInternalToString(GetType(T)) = formatter
        End Sub

        Friend Sub printInternal(x As Object, listPrefix$, maxPrint%, env As GlobalEnvironment)
            Dim valueType As Type = x.GetType

            If valueType.IsInheritsFrom(GetType(Array)) Then
                With DirectCast(x, Array)
                    If .Length > 1 Then
                        Call .printArray(maxPrint, env)
                    ElseIf .Length = 0 Then
                        Call Console.WriteLine("NULL")
                    Else
                        x = .GetValue(Scan0)
                        ' get the first value and then print its
                        ' text value onto console
                        GoTo printSingleElement
                    End If
                End With
            ElseIf valueType Is GetType(vector) Then
                Call DirectCast(x, vector).data.printArray(maxPrint, env)
            ElseIf valueType Is GetType(Dictionary(Of String, Object)) Then
                Call DirectCast(x, Dictionary(Of String, Object)).printList(listPrefix, maxPrint, env)
            ElseIf valueType Is GetType(dataframe) Then
                Call DirectCast(x, dataframe) _
                    .GetTable _
                    .Print(addBorder:=False) _
                    .DoCall(AddressOf Console.WriteLine)
            Else
printSingleElement:
                Call Console.WriteLine("[1] " & printer.ValueToString(x, env))
            End If
        End Sub

        <Extension>
        Private Sub printList(list As Dictionary(Of String, Object), listPrefix$, maxPrint%, env As GlobalEnvironment)
            For Each slot As KeyValuePair(Of String, Object) In list
                Dim key$ = slot.Key

                If key.IsPattern("\d+") Then
                    key = $"{listPrefix}[[{slot.Key}]]"
                Else
                    key = $"{listPrefix}${slot.Key}"
                End If

                Call Console.WriteLine(key)
                Call printer.printInternal(slot.Value, key, maxPrint, env)
                Call Console.WriteLine()
            Next
        End Sub

        ''' <summary>
        ''' Debugger test api of <see cref="ToString"/>
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ValueToString(x As Object, env As GlobalEnvironment) As String
            Return printer.ToString(x.GetType, env)(x)
        End Function

        ''' <summary>
        ''' The external string formatter will overrides the internal formatter
        ''' </summary>
        ''' <param name="elementType"></param>
        ''' <returns></returns>
        <Extension>
        Private Function ToString(elementType As Type, env As GlobalEnvironment) As IStringBuilder
            If RtoString.ContainsKey(elementType) Then
                Return RtoString(elementType)
            ElseIf RInternalToString.ContainsKey(elementType) Then
                Return RInternalToString(elementType)(env)
            ElseIf elementType Is GetType(String) Then
                Return Function(o) As String
                           If o Is Nothing Then
                               Return "NULL"
                           Else
                               Return $"""{o}"""
                           End If
                       End Function
            ElseIf Not (elementType.Namespace.StartsWith("System.") OrElse elementType.Namespace = "System") Then
                Return AddressOf classPrinter.printClass
            ElseIf elementType = GetType(Boolean) Then
                Return Function(b) b.ToString.ToUpper
            Else
                Return Function(obj) Scripting.ToString(obj, "NULL", True)
            End If
        End Function

        ''' <summary>
        ''' Print vector elements
        ''' </summary>
        ''' <param name="xvec"></param>
        <Extension>
        Private Sub printArray(xvec As Array, maxPrint%, env As GlobalEnvironment)
            Dim elementType As Type = Runtime.MeasureArrayElementType(xvec)
            Dim toString As IStringBuilder = printer.ToString(elementType, env)
            Dim stringVec = From element As Object
                            In xvec.AsQueryable
                            Select toString(element)
            Dim maxColumns As Integer = Console.WindowWidth
            Dim contents As String() = stringVec.Take(maxPrint).ToArray
            ' maxsize / average size
            Dim divSize As Integer = maxColumns \ contents.Average(Function(c) c.Length + 1) - 1
            Dim i As i32 = 1 - divSize

            For Each row As String() In contents.Split(divSize)
                Call Console.WriteLine($"[{i = i + divSize}]{vbTab}" & row.JoinBy(vbTab))
            Next

            If xvec.Length > maxPrint Then
                Call Console.WriteLine($"[ reached getOption(""max.print"") -- omitted {xvec.Length - contents.Length} entries ]")
            End If
        End Sub
    End Module
End Namespace
