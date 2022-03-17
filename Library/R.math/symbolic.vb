#Region "Microsoft.VisualBasic::a09eedd9eb7700a5fcfcd4091ee074fb, R-sharp\Library\R.math\symbolic.vb"

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

    '   Total Lines: 48
    '    Code Lines: 33
    ' Comment Lines: 9
    '   Blank Lines: 6
    '     File Size: 1.82 KB


    ' Module symbolic
    ' 
    '     Function: ParseExpression, ParseMathML, PolynomialParse
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
Imports Microsoft.VisualBasic.MIME.application.xml.MathML
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' math symbolic expression for R#
''' </summary>
<Package("symbolic", Category:=APICategories.ResearchTools)>
Module symbolic

    ''' <summary>
    ''' Parse a math formula
    ''' </summary>
    ''' <param name="expressions">a list of character vector which are all represents some math formula expression.</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("parse")>
    <RApiReturn(GetType(Expression))>
    Public Function ParseExpression(<RRawVectorArgument> expressions As Object, Optional env As Environment = Nothing) As Object
        Dim str As pipeline = pipeline.TryCreatePipeline(Of String)(expressions, env)

        If str.isError Then
            Return str.getError
        End If

        Return str _
            .populates(Of String)(env) _
            .Select(AddressOf ScriptEngine.ParseExpression) _
            .DoCall(AddressOf Internal.Object.vector.asVector)
    End Function

    <ExportAPI("parse.mathml")>
    Public Function ParseMathML(mathml As String) As LambdaExpression
        Return LambdaExpression.FromMathML(mathml)
    End Function

    <ExportAPI("as.polynomial")>
    Public Function PolynomialParse(expression As String) As Polynomial
        Return Polynomial.Parse(expression)
    End Function
End Module
