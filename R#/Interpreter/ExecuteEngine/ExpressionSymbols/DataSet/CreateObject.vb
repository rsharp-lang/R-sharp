#Region "Microsoft.VisualBasic::2d0ec07101c0d5a42fe91741c7dcd49c, G:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/ExpressionSymbols/DataSet/CreateObject.vb"

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

    '   Total Lines: 88
    '    Code Lines: 70
    ' Comment Lines: 4
    '   Blank Lines: 14
    '     File Size: 3.42 KB


    '     Class CreateObject
    ' 
    '         Properties: constructor, expressionName, name, stackFrame, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString, TryGetType
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Development.Package.File

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' ``new xxx(...)``
    ''' </summary>
    Public Class CreateObject : Inherits Expression
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.generic
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Constructor
            End Get
        End Property

        Public ReadOnly Property name As String
        Public ReadOnly Property constructor As Expression()
        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Sub New(name$, constructor As Expression(), stackframe As StackFrame)
            Me.name = name
            Me.stackFrame = stackframe
            Me.constructor = constructor
        End Sub

        Public Shared Function TryGetType(activator As String, env As Environment) As [Variant](Of RType, Message)
            If env.globalEnvironment.types.ContainsKey(activator) Then
                Return env.globalEnvironment.types(activator)
            Else
                Return Internal.debug.stop({"missing required type information...", "type: " & activator}, env)
            End If
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim type = TryGetType(name, envir)
            Dim obj As vbObject

            If type Like GetType(Message) Then
                Return type.TryCast(Of Message)
            Else
                obj = vbObject.CreateInstance(type.TryCast(Of RType).raw)
            End If

            Dim err As New Value(Of Object)

            ' initialize the property
            For Each prop As Expression In constructor
                If Not TypeOf prop Is ValueAssignExpression Then
                    Return Internal.debug.stop({
                         $"invalid expression: {prop} !",
                         $"require: " & GetType(ValueAssignExpression).Name,
                         $"but given: " & prop.expressionName
                    }, envir)
                Else
                    With DirectCast(prop, ValueAssignExpression)
                        Dim name = .targetSymbols(Scan0).Evaluate(envir)
                        Dim value = .value.Evaluate(envir)

                        If TypeOf (err = obj.setByName(name, value, envir)) Is Message Then
                            Return err.Value
                        End If
                    End With
                End If
            Next

            Return obj
        End Function

        Public Overrides Function ToString() As String
            Return $"new {name}({constructor.JoinBy(", ")})"
        End Function
    End Class
End Namespace
