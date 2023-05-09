Imports System.IO
Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Development.CodeAnalysis

    Public Class TypeWriter

        ReadOnly level As Integer
        ReadOnly symbol As SymbolTypeDefine
        ReadOnly ts As TextWriter

        Sub New(indent As Integer, symbol As SymbolTypeDefine, ts As TextWriter)
            Me.level = indent
            Me.symbol = symbol
            Me.ts = ts
        End Sub

        Public Sub Flush()
            Dim indent As String = New String(" "c, level * 3)
            Dim indent_comment As String = New String(" "c, level * 3 + 2)

            Call ts.WriteLine($"{indent}/**")

            For Each par As NamedValue(Of String) In symbol.parameters
                Dim pname As String = par.Name
                Dim pdocs As String()

                If par.Value IsNot Nothing Then
                    pdocs = $"default value Is ``{par.Value}``.".LineTokens

                    Call ts.WriteLine($"{indent_comment}* @param {pname} {pdocs.First}")

                    For Each line As String In pdocs.Skip(1)
                        Call ts.WriteLine($"{indent_comment}* {line}")
                    Next
                End If
            Next

            Call ts.WriteLine($"{indent}*/")
            Call ts.WriteLine($"{indent}{symbol.GetTypeScriptDeclare};")
        End Sub

        Private Function GetNetCoreCLRDeclaration() As MethodInfo
            If TypeOf symbol.source Is MethodInfo Then
                Return symbol.source
            Else
                Return DirectCast(symbol.source, RMethodInfo).GetNetCoreCLRDeclaration
            End If
        End Function

        Public Sub Flush(context As GlobalEnvironment)
            Dim type As ProjectType = context.packages.packageDocs.GetAnnotations(GetNetCoreCLRDeclaration.DeclaringType)
            Dim docs As ProjectMember = Nothing
            Dim indent As String = New String(" "c, level * 3)
            Dim indent_comment As String = New String(" "c, level * 3 + 2)

            If Not type Is Nothing Then
                docs = type.GetMethods(GetNetCoreCLRDeclaration.Name).FirstOrDefault
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

                For Each pi As NamedValue(Of String) In symbol.parameters
                    Dim pname As String = pi.Name
                    Dim pdocs = docs.GetParameterDocument(pname).LineTokens

                    If Not pi.Value Is Nothing Then
                        pdocs = pdocs _
                            .JoinIterates({"", $"+ default value Is ``{pi.Value}``."}) _
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
                If symbol.parameters.Any(Function(pi) Not pi.Value Is Nothing) Then
                    For Each pi As NamedValue(Of String) In symbol.parameters
                        If Not pi.Value Is Nothing Then
                            Call ts.WriteLine($"{indent_comment}* @param {pi.Name} default value Is ``{pi.Value}``.")
                        End If
                    Next
                End If
            End If

            Call ts.WriteLine($"{indent}*/")
            Call ts.WriteLine($"{indent}{symbol.GetTypeScriptDeclare};")
        End Sub

        Public Overrides Function ToString() As String
            Return symbol.ToString
        End Function

    End Class
End Namespace