#Region "Microsoft.VisualBasic::3abc42e5309329bc6f79963eacf1a2e0, F:/GCModeller/src/R-sharp/R#//System/Package/PackageFile/Serialization/ExpressionTypes.vb"

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

    '   Total Lines: 74
    '    Code Lines: 52
    ' Comment Lines: 6
    '   Blank Lines: 16
    '     File Size: 1.37 KB


    '     Enum ExpressionTypes
    ' 
    '         [Break], [Continute], [Do], [Else], [ElseIf]
    '         [For], [If], [IIf], [Return], [TypeOf]
    '         [Using], [With], AcceptorDeclare, Annotation, Binary
    '         ClosureDeclare, Comment, DotNetMemberReference, ExpressionLiteral, FormulaDeclare
    '         FunctionByRef, FunctionCall, JSONLiteral, LambdaDeclare, SequenceLiteral
    '         Shell, Switch, SymbolAssign, SymbolDeclare, SymbolIndex
    '         SymbolMemberAssign, SymbolNamespaceReference, SymbolRegexp, TryCatch, UnaryNumeric
    '         VectorLiteral, VectorLoop
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Development.Package.File

    ''' <summary>
    ''' The R# expression data type
    ''' </summary>
    Public Enum ExpressionTypes As Integer
        ''' <summary>
        ''' literal value or constant value
        ''' </summary>
        Literal = 1
        VectorLiteral
        SequenceLiteral
        ExpressionLiteral
        JSONLiteral

        StringInterpolation = 10
        FunctionCall
        FunctionByRef

        FunctionDeclare = 20
        ClosureDeclare
        FormulaDeclare
        LambdaDeclare
        SymbolDeclare
        AcceptorDeclare

        SymbolReference = 30
        SymbolNamespaceReference
        SymbolIndex
        SymbolAssign
        SymbolRegexp
        SymbolMemberAssign

        DotNetMemberReference

        ''' <summary>
        ''' A linq query epxression in R# scripting 
        ''' </summary>
        LinqQuery = 40

        ''' <summary>
        ''' expression for require load package 
        ''' </summary>
        Require = 50
        [Imports] = 51

        Constructor = 60
        VectorLoop

        GetArgument = 70

        ''' <summary>
        ''' shell a commandline string
        ''' </summary>
        Shell

        [While] = 80
        [For]
        [If]
        [Else]
        [ElseIf]
        [Break]
        [Continute]
        [IIf]
        [TypeOf]
        [With]
        [Return]
        [Using]
        [Do]
        TryCatch
        Switch

        Suppress = 100
        Annotation
        Comment

        UnaryNot = 110
        Binary



        UnaryNumeric
    End Enum
End Namespace
