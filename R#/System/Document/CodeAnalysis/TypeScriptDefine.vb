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

        Private Sub WriteNamespaceTree(tree As FunctionTree, ts As TextWriter, level As Integer, context As GlobalEnvironment)
            Dim prefix As String = If(level = 0, "declare namespace", "module")

            If tree.IsLeaf Then
                Dim rfunc As RMethodInfo = tree.Method
                Dim returns = rfunc.returns.MapTypeScriptType
                Dim params = rfunc.parameters _
                    .Select(AddressOf MapTypeScriptParameter) _
                    .ToArray
                Dim type As ProjectType = context.packages.packageDocs.GetAnnotations(rfunc.GetNetCoreCLRDeclaration.DeclaringType)
                Dim docs As ProjectMember = Nothing

                If Not type Is Nothing Then
                    docs = type.GetMethods(rfunc.GetNetCoreCLRDeclaration.Name).FirstOrDefault
                End If

                Call ts.WriteLine($"{New String(" "c, level * 3)}/**")

                If Not docs Is Nothing Then
                    For Each line As String In docs.Summary.LineTokens
                        Call ts.WriteLine($"{New String(" "c, level * 3)} * {line}")
                    Next

                    Call ts.WriteLine($"{New String(" "c, level * 3)} * ")

                    For Each line As String In docs.Remarks.LineTokens
                        Call ts.WriteLine($"{New String(" "c, level * 3)} * > {line}")
                    Next

                    Call ts.WriteLine($"{New String(" "c, level * 3)} * ")

                    For Each pi In params
                        Dim pname As String = pi.define.Split(":"c).First.Trim("?"c)
                        Dim pdocs = docs.GetParameterDocument(pname).LineTokens

                        If Not pi.optVal Is Nothing Then
                            pdocs = pdocs _
                                .JoinIterates({"", $"default value Is ``{pi.optVal}``."}) _
                                .ToArray
                        End If

                        If pdocs.Length > 0 AndAlso Not pdocs.All(Function(si) si.StringEmpty) Then
                            Call ts.WriteLine($"{New String(" "c, level * 3 + 2)}* @param {pname} {pdocs.First}")

                            For Each line As String In pdocs.Skip(1)
                                Call ts.WriteLine($"{New String(" "c, level * 3 + 2)}* {line}")
                            Next
                        End If
                    Next
                Else
                    If params.Any(Function(pi) Not pi.optVal Is Nothing) Then
                        For Each pi In params
                            If Not pi.optVal Is Nothing Then
                                Call ts.WriteLine($"{New String(" "c, level * 3 + 2)}* @param {pi.define.Split(":"c).First.Trim("?"c)} default value Is ``{pi.optVal}``.")
                            End If
                        Next
                    End If
                End If

                Call ts.WriteLine($"{New String(" "c, level * 3)}*/")

                Call ts.WriteLine($"{New String(" "c, level * 3)}function {tree.Name}({params.Select(Function(pi) pi.define).JoinBy(", ")}): any;")
            Else
                Call ts.WriteLine($"{New String(" "c, level * 3)}{prefix} {tree.Name.Replace("+", "_")} {{")

                For Each child In tree.ChildNodes
                    Call WriteNamespaceTree(child, ts, level + 1, context)
                Next

                Call ts.WriteLine($"{New String(" "c, level * 3)}}}")
            End If
        End Sub

        Private Class FunctionTree : Inherits TreeNodeBase(Of FunctionTree)

            Public Overrides ReadOnly Property MySelf As FunctionTree
                Get
                    Return Me
                End Get
            End Property

            Public Property Method As RMethodInfo

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

        End Class

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
                Next

                func.Method = New RMethodInfo(api)
            Next

            Return tree
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