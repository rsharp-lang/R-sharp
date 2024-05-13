#Region "Microsoft.VisualBasic::246f3ec7e2ce82615e5ee038f4831c3a, R#\Runtime\Internal\internalInvokes\Linq\Group.vb"

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

    '   Total Lines: 115
    '    Code Lines: 83
    ' Comment Lines: 13
    '   Blank Lines: 19
    '     File Size: 4.12 KB


    '     Structure Group
    ' 
    '         Properties: length
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) getByName, getNames, InternalToString, (+2 Overloads) setByName, (+2 Overloads) ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter

Namespace Runtime.Internal.Invokes.LinqPipeline

    ''' <summary>
    ''' Elements in <see cref="group"/> have a common <see cref="key"/>
    ''' </summary>
    Friend Structure Group : Implements RNameIndex

        Public key As Object
        Public group As Array

        Public ReadOnly Property length As Integer
            Get
                Return group.Length
            End Get
        End Property

        ''' <summary>
        ''' get by 0 based index
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property Item(i As Integer) As Object
            Get
                If i >= group.Length Then
                    Return Nothing
                Else
                    Return group.GetValue(i)
                End If
            End Get
        End Property

        Shared Sub New()
            Call printer.AttachInternalConsoleFormatter(Of Group)(AddressOf InternalToString)
        End Sub

        Private Shared Function InternalToString(printContent As Boolean, env As GlobalEnvironment) As IStringBuilder
            Return Function(x) DirectCast(x, Group).ToString(env)
        End Function

        Public Overrides Function ToString() As String
            Return Scripting.ToString(key)
        End Function

        Public Overloads Function ToString(env As Environment) As String
            Dim globalEnv As GlobalEnvironment = env.globalEnvironment

            Return $" '{group.Length}' elements with key: " & printer.ValueToString(key, globalEnv) & vbCrLf &
                group.AsObjectEnumerator _
                    .Select(Function(o)
                                Return "   " & printer.ValueToString(o, globalEnv)
                            End Function) _
                    .JoinBy(vbCrLf)
        End Function

        ''' <summary>
        ''' just supports get data via 'key' or 'group'
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function getByName(name As String) As Object Implements RNameIndex.getByName
            If name = NameOf(key) Then
                Return key
            ElseIf name = NameOf(group) Then
                Return group
            Else
                Return Nothing
            End If
        End Function

        Public Function getByName(names() As String) As Object Implements RNameIndex.getByName
            Return names.Select(AddressOf getByName).ToArray
        End Function

        Public Function setByName(name As String, value As Object, envir As Environment) As Object Implements RNameIndex.setByName
            If name = NameOf(key) Then
                key = value
            ElseIf name = NameOf(group) Then
                group = Runtime.asVector(Of Object)(value)
            Else
                Return Internal.debug.stop(New InvalidOperationException, envir)
            End If

            Return value
        End Function

        Public Function setByName(names() As String, value As Array, envir As Environment) As Object Implements RNameIndex.setByName
            Dim values As New List(Of Object)
            Dim val As Object
            Dim i As i32 = Scan0

            For Each name As String In names
                val = setByName(name, value.GetValue(++i), envir)

                If Program.isException(val) Then
                    Return val
                Else
                    values.Add(val)
                End If
            Next

            Return values.ToArray
        End Function

        Public Function getNames() As String() Implements IReflector.getNames
            Return {NameOf(key), NameOf(group)}
        End Function
    End Structure
End Namespace
