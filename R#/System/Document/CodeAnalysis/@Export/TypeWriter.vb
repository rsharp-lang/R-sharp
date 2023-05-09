Imports System.IO
Imports SMRUCC.Rsharp.Runtime

Namespace Development.CodeAnalysis

    Public Class TypeWriter

        ReadOnly indent As Integer
        ReadOnly symbol As SymbolTypeDefine
        ReadOnly ts As TextWriter

        Sub New(indent As Integer, symbol As SymbolTypeDefine, ts As TextWriter)
            Me.indent = indent
            Me.symbol = symbol
            Me.ts = ts
        End Sub

        Public Sub Flush()
            Dim valueType As String = RType.GetType(Func.type).MapTypeScriptType
            Dim params = Func.parameters _
                .Select(Function(a)
                            Return $"{a.GetSymbolName}:{RType.GetType(a.type).MapTypeScriptType}"
                        End Function) _
                .ToArray
            Dim indent As String = New String(" "c, level * 3)
            Dim indent_comment As String = New String(" "c, level * 3 + 2)

            Call ts.WriteLine($"{indent}/**")

            For Each par As DeclareNewSymbol In Func.parameters
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

        Public Sub Flush(context As GlobalEnvironment)
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

        Public Overrides Function ToString() As String
            Return symbol.ToString
        End Function

    End Class
End Namespace