Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.Tree
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
                Call tree.Symbol1.WriteSymbol(tree.Name, ts, level)
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

        <Extension>
        Private Sub WriteSymbol(func As DeclareNewFunction, treeName As String, ts As TextWriter, level As Integer)
            Dim valueType As String = RType.GetType(func.type).MapTypeScriptType
            Dim params = func.parameters _
                .Select(Function(a)
                            Return $"{a.GetSymbolName}:{RType.GetType(a.type).MapTypeScriptType}"
                        End Function) _
                .ToArray
            Dim indent As String = New String(" "c, level * 3)
            Dim indent_comment As String = New String(" "c, level * 3 + 2)

            Call ts.WriteLine($"{indent}/**")

            For Each par As DeclareNewSymbol In func.parameters
                Dim pname As String = par.GetSymbolName
                Dim pdocs As String()

                If par.hasInitializeExpression Then
                    pdocs = $"default value Is ``{par.value.ToString}``.".LineTokens

                    Call ts.WriteLine($"{indent_comment}* @param {pname} {pdocs.First}")

                    For Each line As String In pdocs.Skip(1)
                        Call ts.WriteLine($"{indent_comment}* {line}")
                    Next
                End If
            Next

            Call ts.WriteLine($"{indent}*/")
            Call ts.WriteLine($"{indent}function {treeName}({params.JoinBy(", ")}): {valueType};")
        End Sub

        Private Sub WriteNamespaceTree(tree As FunctionTree, ts As TextWriter, level As Integer, context As GlobalEnvironment)
            Dim prefix As String = If(level = 0, "declare namespace", "module")
            Dim indent As String = New String(" "c, level * 3)

            If tree.IsLeaf Then
                Call tree.Method1.WriteFunction(tree.Name, ts, level, context)
            Else
                Call ts.WriteLine($"{indent}{prefix} {tree.Name.Replace("+", "_")} {{")

                For Each child In tree.ChildNodes
                    Call WriteNamespaceTree(child, ts, level + 1, context)
                Next

                Call ts.WriteLine($"{indent}}}")
            End If
        End Sub

        <Extension>
        Private Sub WriteFunction(rfunc As RMethodInfo, treeName As String, ts As TextWriter, level As Integer, context As GlobalEnvironment)
            Dim returns = rfunc.returns.MapTypeScriptType
            Dim params = rfunc.parameters _
                .Select(AddressOf MapTypeScriptParameter) _
                .ToArray
            Dim type As ProjectType = context.packages.packageDocs.GetAnnotations(rfunc.GetNetCoreCLRDeclaration.DeclaringType)
            Dim docs As ProjectMember = Nothing
            Dim unionType As String = rfunc.GetUnionTypes _
                .Select(Function(ti) RType.GetRSharpType(ti).MapTypeScriptType) _
                .JoinBy("|")
            Dim indent As String = New String(" "c, level * 3)
            Dim indent_comment As String = New String(" "c, level * 3 + 2)

            If Not type Is Nothing Then
                docs = type.GetMethods(rfunc.GetNetCoreCLRDeclaration.Name).FirstOrDefault
            End If

            Call ts.WriteLine($"{indent}/**")

            If Not docs Is Nothing Then
                For Each line As String In docs.Summary.LineTokens
                    Call ts.WriteLine($"{indent} * {line}")
                Next

                Call ts.WriteLine($"{indent} * ")

                For Each line As String In docs.Remarks.LineTokens
                    Call ts.WriteLine($"{indent} * > {line}")
                Next

                Call ts.WriteLine($"{indent} * ")

                For Each pi In params
                    Dim pname As String = pi.define.Split(":"c).First.Trim("?"c)
                    Dim pdocs = docs.GetParameterDocument(pname).LineTokens

                    If Not pi.optVal Is Nothing Then
                        pdocs = pdocs _
                            .JoinIterates({"", $"+ default value Is ``{pi.optVal}``."}) _
                            .ToArray
                    End If

                    If pdocs.Length > 0 AndAlso Not pdocs.All(Function(si) si.StringEmpty) Then
                        Call ts.WriteLine($"{indent_comment}* @param {pname} {pdocs.First}")

                        For Each line As String In pdocs.Skip(1)
                            Call ts.WriteLine($"{indent_comment}* {line}")
                        Next
                    End If
                Next

                Dim rdocs = docs.Returns.LineTokens

                If Not rdocs.IsNullOrEmpty Then
                    Call ts.WriteLine($"{indent_comment}* @return {rdocs(0)}")

                    For Each line As String In rdocs.Skip(1)
                        Call ts.WriteLine($"{indent_comment}* {line}")
                    Next
                End If
            Else
                If params.Any(Function(pi) Not pi.optVal Is Nothing) Then
                    For Each pi In params
                        If Not pi.optVal Is Nothing Then
                            Call ts.WriteLine($"{indent_comment}* @param {pi.define.Split(":"c).First.Trim("?"c)} default value Is ``{pi.optVal}``.")
                        End If
                    Next
                End If
            End If

            Call ts.WriteLine($"{indent}*/")
            Call ts.WriteLine($"{indent}function {treeName}({params.Select(Function(pi) pi.define).JoinBy(", ")}): {unionType};")
        End Sub

        Private Class FunctionTree : Inherits TreeNodeBase(Of FunctionTree)

            Public Overrides ReadOnly Property MySelf As FunctionTree
                Get
                    Return Me
                End Get
            End Property

            Public Property Method1 As RMethodInfo
            Public Property Symbol1 As SymbolExpression
            Public Property FunctionTrace As New List(Of String)

            Public Sub New(name As String)
                MyBase.New(name)
            End Sub

            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="name"></param>
            ''' <returns>
            ''' the return value always not null
            ''' </returns>
            Public Function GetNode(name As String) As FunctionTree
                Dim find = ChildNodes.Where(Function(t) t.Name = name).FirstOrDefault

                If find Is Nothing Then
                    find = New FunctionTree(name)
                    AddChild(find)
                End If

                Return find
            End Function

            Public Function SortName() As FunctionTree
                ChildNodes = ChildNodes _
                    .OrderBy(Function(a) a.Name) _
                    .Select(Function(a) a.SortName) _
                    .AsList

                Return Me
            End Function

        End Class

        Private Function BuildNamespaceTree(pkgName As String, symbols As SymbolExpression()) As FunctionTree
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

                func.Symbol1 = symbol
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

                func.Method1 = New RMethodInfo(api)
            Next

            Return tree.SortName
        End Function

        Private Function MapTypeScriptParameter(p As RMethodArgument) As (define As String, optVal As String)
            Dim name As String = p.name
            Dim type As String = p.type.MapTypeScriptType
            Dim optVal As String

            If p.isOptional Then
                Dim def = p.default

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

                Return ($"{name}?:{type}", optVal)
            Else
                Return ($"{name}:{type}", Nothing)
            End If
        End Function

        <Extension>
        Private Function MapTypeScriptType(type As RType) As String
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