#Region "Microsoft.VisualBasic::b2c9154c67eb284cd00cb11c5e64f6cc, F:/GCModeller/src/R-sharp/R#//System/Document/CodeAnalysis/@Export/TypeWriter.vb"

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

    '   Total Lines: 140
    '    Code Lines: 109
    ' Comment Lines: 0
    '   Blank Lines: 31
    '     File Size: 5.29 KB


    '     Class TypeWriter
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetNetCoreCLRDeclaration, ToString
    ' 
    '         Sub: (+2 Overloads) Flush, WriteFunction, WriteSymbol
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
            If symbol.isSymbol Then
                Call WriteSymbol()
            Else
                Call WriteFunction()
            End If
        End Sub

        Private Sub WriteSymbol()
            Call ts.WriteLine($"{New String(" "c, level * 3)}{symbol.GetTypeScriptDeclare};")
        End Sub

        Private Sub WriteFunction()
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
