#Region "Microsoft.VisualBasic::357fe6d9f0604ce9dc1c2ba880336896, R#\System\Document\CodeAnalysis\@Export\TypeScriptDefine.vb"

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

    '   Total Lines: 263
    '    Code Lines: 184 (69.96%)
    ' Comment Lines: 28 (10.65%)
    '    - Xml Docs: 96.43%
    ' 
    '   Blank Lines: 51 (19.39%)
    '     File Size: 9.98 KB


    '     Module TypeScriptDefine
    ' 
    '         Function: (+2 Overloads) BuildNamespaceTree, ExtractModule, ExtractPackage, MapTypeScriptParameter, MapTypeScriptType
    ' 
    '         Sub: ExtractModule, ExtractPackage, WriteNamespaceTree, WriteSourceTree, WriteSymbol
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports pkg = SMRUCC.Rsharp.Development.Package.Package

Namespace Development.CodeAnalysis

    ''' <summary>
    ''' the generator module for the type script definition
    ''' </summary>
    Public Module TypeScriptDefine

        ''' <summary>
        ''' extract the typescript module definition 
        ''' </summary>
        ''' <param name="pkg"></param>
        ''' <returns></returns>
        Public Function ExtractModule(context As GlobalEnvironment, ParamArray pkg As pkg()) As String
            Dim ts As New StringBuilder
            Dim file As New StringWriter(ts)

            Call ExtractModule(pkg, ts:=file, context)

            Return ts.ToString
        End Function

        Public Function ExtractPackage(symbols As SymbolExpression(), [namespace] As String) As String
            Dim ts As New StringBuilder
            Dim file As New StringWriter(ts)

            Call ExtractPackage(symbols, [namespace], ts:=file)
            Call file.Flush()

            Return ts.ToString
        End Function

        Public Sub ExtractPackage(symbols As SymbolExpression(), [namespace] As String, ts As TextWriter)
            Dim tree = BuildNamespaceTree([namespace], symbols)

            Call ts.WriteLine("// export R# source type define for javascript/typescript language")
            Call ts.WriteLine("//")
            Call ts.WriteLine($"// package_source={[namespace]}")
            Call ts.WriteLine()

            Call WriteSourceTree(tree, ts, level:=0)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="pkgs">
        ''' modules with the same namespace reference
        ''' </param>
        ''' <param name="ts"></param>
        Public Sub ExtractModule(pkgs As pkg(), ts As TextWriter, context As GlobalEnvironment)
            Dim tree = BuildNamespaceTree(pkgs(0).namespace, pkgs)

            Call ts.WriteLine("// export R# package module type define for javascript/typescript language")
            Call ts.WriteLine("//")

            For Each type As pkg In pkgs
                Call ts.WriteLine($"//    imports ""{type.namespace}"" from ""{type.dllName}"";")
            Next

            Call ts.WriteLine("//")

            For Each type As pkg In pkgs
                Call ts.WriteLine($"// ref={type.package.FullName}@{type.package.Assembly.ToString}")
            Next

            Call ts.WriteLine()

            Call ts.WriteLine("/**")

            For Each type As pkg In pkgs
                Dim docs As ProjectType = context.packages.packageDocs.GetAnnotations(type.package)

                If Not docs Is Nothing Then
                    For Each line As String In docs.Summary.LineTokens
                        Call ts.WriteLine($" * {line}")
                    Next

                    Call ts.WriteLine(" * ")

                    For Each line As String In docs.Remarks.LineTokens
                        Call ts.WriteLine($" * > {line}")
                    Next
                End If
            Next

            Call ts.WriteLine("*/")

            Call WriteNamespaceTree(tree, ts, 0, context)
            Call ts.Flush()
        End Sub

        Private Sub WriteSourceTree(tree As FunctionTree, ts As TextWriter, level As Integer)
            Dim prefix As String = If(level = 0, "declare namespace", "module")
            Dim indent As String = New String(" "c, level * 3)

            If tree.IsLeaf Then
                Call New TypeWriter(level, tree.Symbol1, ts).Flush()
            Else
                Call ts.WriteLine($"{indent}{prefix} {tree.Name.Replace("+", "_")} {{")

                For Each child In tree.ChildNodes
                    Call WriteSourceTree(child, ts, level + 1)
                Next

                Call ts.WriteLine($"{indent}}}")
            End If
        End Sub

        <Extension>
        Private Sub WriteSymbol(symbol As SymbolExpression, treeName As String, ts As TextWriter, level As Integer)
            If TypeOf symbol Is DeclareNewFunction Then
                Call DirectCast(symbol, DeclareNewFunction).WriteSymbol(treeName, ts, level)
            Else
                Call $"not implements ts writer for {symbol.GetType.FullName}".Warning
            End If
        End Sub

        Private Sub WriteNamespaceTree(tree As FunctionTree, ts As TextWriter, level As Integer, context As GlobalEnvironment)
            Dim prefix As String = If(level = 0, "declare namespace", "module")
            Dim indent As String = New String(" "c, level * 3)

            If tree.IsLeaf Then
                Call New TypeWriter(level, tree.Symbol1, ts).Flush(context)
            Else
                Call ts.WriteLine($"{indent}{prefix} {tree.Name.Replace("+", "_")} {{")

                For Each child In tree.ChildNodes
                    Call WriteNamespaceTree(child, ts, level + 1, context)
                Next

                Call ts.WriteLine($"{indent}}}")
            End If
        End Sub

        Private Function BuildNamespaceTree(pkgName As String, symbols As SymbolExpression())
            Dim tree As New FunctionTree(pkgName)

            For Each symbol As SymbolExpression In symbols
                Dim t = symbol.GetSymbolName.Split("."c)
                Dim func As FunctionTree = tree

                If t(0) = "" Then
                    t(0) = "_"
                End If

                For Each ti As String In t
                    func = func.GetNode(name:=ti)
                    func.FunctionTrace.Add(symbol.GetSymbolName)
                Next

                func.Symbol1 = New SymbolTypeDefine(symbol)
            Next

            Return tree.SortName
        End Function

        ''' <summary>
        ''' due to the reason of R symbol name may contains the ``dot`` symbol
        ''' and this may confuse the javascript/python syntax, so needs the 
        ''' tree structure to solve this problem
        ''' </summary>
        ''' <param name="root"></param>
        ''' <param name="pkg"></param>
        ''' <returns></returns>
        Private Function BuildNamespaceTree(root As String, pkg As pkg()) As FunctionTree
            Dim tree As New FunctionTree(root)

            For Each api As NamedValue(Of MethodInfo) In pkg _
               .Select(AddressOf ImportsPackage.GetAllApi) _
               .IteratesALL

                Dim t = api.Name.Split("."c)
                Dim func As FunctionTree = tree

                For Each ti As String In t
                    func = func.GetNode(name:=ti)
                    func.FunctionTrace.Add(api.Name)
                Next

                func.Symbol1 = New SymbolTypeDefine(New RMethodInfo(api))
            Next

            Return tree.SortName
        End Function

        <Extension>
        Friend Function MapTypeScriptParameter(p As RMethodArgument) As NamedValue(Of String)
            Dim name As String = p.name
            Dim type As String = p.type.MapTypeScriptType
            Dim optVal As String

            If Not p.isOptional Then
                Return New NamedValue(Of String)(name, Nothing, type)
            End If

            Dim def As Object = p.default

            If def Is Nothing Then
                optVal = "null"
            Else
                Select Case RType.TypeOf(def).mode
                    Case TypeCodes.boolean
                        Dim b = CLRVector.asLogical(def)
                        optVal = If(b.Length = 1, b(0).ToString.ToLower, b.GetJson)
                    Case TypeCodes.integer
                        Dim i = CLRVector.asInteger(def)
                        optVal = If(i.Length = 1, i(0).ToString, i.GetJson)
                    Case TypeCodes.double
                        Dim d = CLRVector.asNumeric(def)
                        optVal = If(d.Length = 1, d(0).ToString, d.GetJson)
                    Case TypeCodes.string
                        Dim s = CLRVector.asCharacter(def)
                        optVal = If(s.Length = 1, $"'{s(0)}'", s.GetJson)
                    Case Else
                        optVal = "null"
                End Select
            End If

            Return New NamedValue(Of String)(name, optVal, type)
        End Function

        ''' <summary>
        ''' mapping R# clr type to typescript type mark string
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Friend Function MapTypeScriptType(type As RType) As String
            If type Is RType.any Then
                Return "any"
            ElseIf type Is RType.list Then
                Return "object"
            ElseIf type.mode = TypeCodes.boolean Then
                Return "boolean"
            ElseIf type.mode = TypeCodes.closure Then
                Return "any"
            ElseIf type.mode = TypeCodes.double Then
                Return "number"
            ElseIf type.mode = TypeCodes.string Then
                Return "string"
            Else
                Return "object"
            End If
        End Function
    End Module
End Namespace
