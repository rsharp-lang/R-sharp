#Region "Microsoft.VisualBasic::da5fd60016269958e9a78464bb87c1ce, G:/GCModeller/src/R-sharp/R#//Runtime/Interop/RsharpOperator/ROperatorInvoke.vb"

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

    '   Total Lines: 73
    '    Code Lines: 59
    ' Comment Lines: 1
    '   Blank Lines: 13
    '     File Size: 2.48 KB


    '     Class ROperatorInvoke
    ' 
    '         Properties: op
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetInvoke, Invoke2, Invoke3, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports REnv = SMRUCC.Rsharp.Runtime.Internal

Namespace Runtime.Interop.Operator

    Public Class ROperatorInvoke

        ReadOnly left, right As RType
        ReadOnly method As MethodInfo

        Public Property op As ROperatorAttribute

        Sub New(left As RType, right As RType, api As MethodInfo)
            Me.left = left
            Me.right = right
            Me.method = api
        End Sub

        Public Overrides Function ToString() As String
            Return $"{left}{op}{right} => {method.ToString}"
        End Function

        Public Function GetInvoke(argsN As Integer) As IBinaryOperator
            ' fix of System.Reflection.TargetParameterCountException: Parameter count mismatch.

            If argsN = 2 Then
                Return AddressOf Invoke2
            ElseIf argsN = 3 Then
                Return AddressOf Invoke3
            Else
                Throw New InvalidProgramException
            End If
        End Function

        Public Function Invoke2(x As Object, y As Object, internal As Environment) As Object
            x = RCType.CTypeDynamic(x, left, internal)
            y = RCType.CTypeDynamic(y, right, internal)

            If TypeOf x Is Message Then
                Return x
            ElseIf TypeOf y Is Message Then
                Return y
            Else
                Try
                    Return method.Invoke(Nothing, {x, y})
                Catch ex As Exception
                    Return REnv.debug.stop(New RuntimeError(ToString, ex), internal)
                End Try
            End If
        End Function

        Public Function Invoke3(x As Object, y As Object, internal As Environment) As Object
            x = RCType.CTypeDynamic(x, left, internal)
            y = RCType.CTypeDynamic(y, right, internal)

            If TypeOf x Is Message Then
                Return x
            ElseIf TypeOf y Is Message Then
                Return y
            Else
                Try
                    Return method.Invoke(Nothing, {x, y, internal})
                Catch ex As Exception
                    Return REnv.debug.stop(New RuntimeError(ToString, ex), internal)
                End Try
            End If
        End Function

    End Class
End Namespace
