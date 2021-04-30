#Region "Microsoft.VisualBasic::9d1c32c930216c38e2aebb00281c4aa0, R#\System\Package\PackageFile\Serialization\ExpressionTypes.vb"

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

    '     Enum ExpressionTypes
    ' 
    '         [Break], [Continute], [Do], [Else], [For]
    '         [If], [IIf], [Return], [TypeOf], [Using]
    '         [While], [With], Annotation, Binary, ClosureDeclare
    '         Comment, Constructor, ExpressionLiteral, FormulaDeclare, FunctionByRef
    '         FunctionCall, FunctionDeclare, GetArgument, LambdaDeclare, LinqQuery
    '         Literal, Require, SequenceLiteral, Shell, StringInterpolation
    '         Suppress, SymbolAssign, SymbolDeclare, SymbolIndex, SymbolMemberAssign
    '         SymbolNamespaceReference, SymbolReference, SymbolRegexp, UnaryNot, VectorLiteral
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Development.Package.File

    Public Enum ExpressionTypes As Integer
        Literal = 1
        VectorLiteral
        SequenceLiteral
        ExpressionLiteral

        StringInterpolation = 10
        FunctionCall
        FunctionByRef

        FunctionDeclare = 20
        ClosureDeclare
        FormulaDeclare
        LambdaDeclare
        SymbolDeclare

        SymbolReference = 30
        SymbolNamespaceReference
        SymbolIndex
        SymbolAssign
        SymbolRegexp
        SymbolMemberAssign

        ''' <summary>
        ''' A linq query epxression in R# scripting 
        ''' </summary>
        LinqQuery = 40

        Require = 50

        Constructor = 60

        GetArgument = 70
        Shell

        [While] = 80
        [For]
        [If]
        [Else]
        [Break]
        [Continute]
        [IIf]
        [TypeOf]
        [With]
        [Return]
        [Using]
        [Do]

        Suppress = 100
        Annotation
        Comment

        UnaryNot = 110
        Binary
    End Enum
End Namespace
