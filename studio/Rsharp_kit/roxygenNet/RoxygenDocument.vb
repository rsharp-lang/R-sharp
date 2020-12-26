Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.System

Public Class RoxygenDocument

    Public Shared Iterator Function ParseDocuments(scriptText As String) As IEnumerable(Of Document)
        Dim script As Program = Program.BuildProgram(scriptText)
        Dim symbols As Expression() = script.Where(Function(line) TypeOf line Is DeclareNewFunction).ToArray
        Dim list As Dictionary(Of String, Document) = SplitBlocks(scriptText) _
            .ToDictionary(Function(a) a.Name,
                          Function(a)
                              Return a.Value
                          End Function)

        For Each item In symbols
            If TypeOf item Is DeclareNewFunction Then

            Else
                Throw New NotImplementedException(item.GetType.FullName)
            End If
        Next
    End Function

    Private Shared Iterator Function SplitBlocks(scriptText As String) As IEnumerable(Of NamedValue(Of Document))
        Dim buffer As New List(Of String)

        For Each line As String In scriptText.LineIterators
            If line.StartsWith("#'") Then
                buffer.Add(line.Substring(2).Trim)
            ElseIf buffer > 0 Then
                Dim name As String() = line.Trim.Split.Where(Function(s) Not s.StringEmpty).ToArray

                Yield New NamedValue(Of Document) With {
                    .Name = name(1),
                    .Value = buffer.PopAll _
                        .Where(Function(str) Not str.StringEmpty) _
                        .DoCall(AddressOf continuteLines) _
                        .DoCall(AddressOf ParseDocument)
                }
            End If
        Next
    End Function

    Private Shared Function continuteLines(lines As IEnumerable(Of String)) As String()
        Dim list As New List(Of String)
        Dim continuteLine As String = Nothing

        For Each line As String In lines
            If line.FirstOrDefault = "@"c Then
                If Not continuteLine Is Nothing Then
                    list.Add(continuteLine)
                    continuteLine = Nothing
                End If

                continuteLine = line
            Else
                continuteLine = continuteLine & " " & line
            End If
        Next

        If Not continuteLine Is Nothing Then
            list.Add(continuteLine)
        End If

        Return list.ToArray
    End Function

    Private Shared Function ParseDocument(lines As String()) As Document
        Dim title As String = lines(Scan0).Trim
        Dim tagsData As Dictionary(Of String, String()) = lines _
            .Skip(1) _
            .Select(Function(line)
                        Return line.GetTagValue(, trim:=True)
                    End Function) _
            .GroupBy(Function(a) a.Name) _
            .ToDictionary(Function(a) a.Key,
                          Function(g)
                              Return g.Select(Function(t) t.Value).ToArray
                          End Function)

        Return New Document With {
            .author = tagsData.TryGetValue("@author"),
            .title = title,
            .description = tagsData.TryGetValue("@description").JoinBy(vbCrLf),
            .details = tagsData.TryGetValue("@details").JoinBy(vbCrLf),
            .keywords = tagsData.TryGetValue("@keywords").SafeQuery.ToArray,
            .returns = tagsData.TryGetValue("@return").DefaultFirst,
            .parameters = tagsData.TryGetValue("@param") _
                .SafeQuery _
                .Select(Function(line)
                            Dim tag = line.GetTagValue(trim:=True)
                            Dim val As New NamedValue With {
                                .name = tag.Name,
                                .text = tag.Value
                            }

                            Return val
                        End Function) _
                .ToArray
        }
    End Function
End Class
