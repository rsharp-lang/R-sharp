﻿#Region "Microsoft.VisualBasic::8847aad2083cb762499eadf45191219c, R#\Runtime\Serialize\BufferHandler.vb"

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

    '   Total Lines: 67
    '    Code Lines: 57 (85.07%)
    ' Comment Lines: 1 (1.49%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (13.43%)
    '     File Size: 3.12 KB


    '     Module BufferHandler
    ' 
    '         Function: getBuffer, getBufferObject
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting

Namespace Runtime.Serialize

    Public Module BufferHandler

        Public Function getBuffer(result As Object, env As Environment) As Buffer
            Dim buffer As New Buffer
            buffer.data = getBufferObject(result, env)
            Return buffer
        End Function

        Public Function getBufferObject(result As Object, env As Environment) As BufferObject
            If TypeOf result Is RReturn Then
                result = DirectCast(result, RReturn).Value
            ElseIf TypeOf result Is ReturnValue Then
                result = DirectCast(result, ReturnValue).Evaluate(env)
            End If

            If result Is Nothing Then
                Return rawBuffer.getEmptyBuffer
            ElseIf TypeOf result Is dataframe Then
                Return New dataframeBuffer(result, env)
            ElseIf TypeOf result Is vector Then
                Return vectorBuffer.CreateBuffer(DirectCast(result, vector), env)
            ElseIf TypeOf result Is list Then
                Return New listBuffer(result, env)
            ElseIf TypeOf result Is Message Then
                If env.globalEnvironment.Rscript.debug Then
                    Call Internal.debug.PrintMessageInternal(DirectCast(result, Message), env.globalEnvironment)
                End If

                Return New messageBuffer(DirectCast(result, Message))
            ElseIf TypeOf result Is BufferObject Then
                Return DirectCast(result, BufferObject)
            ElseIf TypeOf result Is Expression Then
                Return New rscriptBuffer With {.target = result}
            ElseIf DataFramework.IsPrimitive(result.GetType) Then
                ' save scalar value object as vector
                Return vectorBuffer.CreateBuffer(vector.fromScalar(result), env)
            ElseIf result.GetType.IsArray Then
                Dim generic As Array = TryCastGenericArray(result, env)
                Dim vec As New vector(generic, RType.GetRSharpType(generic.GetType.GetElementType))

                Return vectorBuffer.CreateBuffer(vec, env)
            ElseIf result.GetType.ImplementInterface(Of IDictionary) Then
                Dim wrapper As New list With {.slots = New Dictionary(Of String, Object)}
                Dim tuples As IDictionary = result

                For Each key As Object In tuples.Keys
                    Call wrapper.slots.Add(any.ToString(key), tuples.Item(key))
                Next

                Return New listBuffer(wrapper, env)
            Else
                Throw New NotImplementedException(result.GetType.FullName)
            End If
        End Function
    End Module
End Namespace
