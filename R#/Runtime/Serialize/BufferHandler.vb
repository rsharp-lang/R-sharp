#Region "Microsoft.VisualBasic::1cffd3a4f979f83ecf9aa864333052d6, R-sharp\studio\Rstudio.common\BufferHandler.vb"

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

'   Total Lines: 41
'    Code Lines: 35
' Comment Lines: 0
'   Blank Lines: 6
'     File Size: 1.64 KB


' Module BufferHandler
' 
'     Function: getBuffer
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

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
                Dim vec As New vector With {.data = {result}}
                ' save scalar value object as vector
                Return vectorBuffer.CreateBuffer(vec, env)
            Else
                Throw New NotImplementedException(result.GetType.FullName)
            End If
        End Function
    End Module
End Namespace