#Region "Microsoft.VisualBasic::ab03898fd543ea016b6ca755d46bc295, R#\Runtime\Interop\RInteropAttributes\RDefaultExpressionAttribute.vb"

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

    '   Total Lines: 37
    '    Code Lines: 16
    ' Comment Lines: 17
    '   Blank Lines: 4
    '     File Size: 1.27 KB


    '     Class RDefaultExpressionAttribute
    ' 
    '         Function: ParseDefaultExpression
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Interop

    ''' <summary>
    ''' the default string literal value of the parameter 
    ''' can be parsed end evaluated as R expression 
    ''' result.
    ''' </summary>
    ''' <remarks>
    ''' for example, there is an optional argument in package api:
    ''' 
    ''' ```
    ''' &lt;<see cref="RDefaultExpressionAttribute"/>> 
    ''' Optional time As <see cref="Object"/> = "~now()"
    ''' ```
    ''' 
    ''' then the default value of the parameter time can be the 
    ''' evaluated result of the default expression ``now()``.
    ''' 
    ''' </remarks>
    Public Class RDefaultExpressionAttribute : Inherits RInteropAttribute

        Public Shared Function ParseDefaultExpression(strExp As String) As Expression
            Return Rscript _
                .FromText(strExp.Trim("~"c)) _
                .DoCall(Function(script)
                            Return Program.CreateProgram(script, debug:=False, [error]:=Nothing)
                        End Function) _
                .First
        End Function

    End Class
End Namespace
