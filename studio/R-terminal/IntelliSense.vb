Imports Microsoft.VisualBasic.ApplicationServices.Terminal.LineEdit
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime

Friend Class IntelliSense

    ReadOnly R As RInterpreter

    Sub New(R As RInterpreter)
        Me.R = R
    End Sub

    Private Function AllSymbols() As IEnumerable(Of String)
        Dim globalEnv = R.globalEnvir
        Dim globalSymbols = globalEnv.EnumerateAllSymbols.JoinIterates(globalEnv.EnumerateAllFunctions)
        ' system internal hiddens
        Dim internals = Internal.invoke.getAllInternals
        Dim symbols As IEnumerable(Of String) = globalSymbols _
            .Select(Function(s) s.name) _
            .JoinIterates(internals.Select(Function(s) s.name)) _
            .Distinct

        Return symbols
    End Function

    Private Function packageFunctions(package As String) As IEnumerable(Of String)
        Dim globalEnv = R.globalEnvir
        Dim namespace_loaded = globalEnv.attachedNamespace(package)

        If namespace_loaded Is Nothing Then
            Return {}
        Else
            Return namespace_loaded _
                .EnumerateAllSymbols _
                .JoinIterates(namespace_loaded.EnumerateAllFunctions) _
                .Select(Function(s) s.name) _
                .Distinct _
                .ToArray
        End If
    End Function

    Private Function GetFileNamesCompletion(s As String, path_str As String) As Completion
        Dim ls As String()

        If path_str.DirectoryExists Then
            Dim is_dot As Boolean = path_str.IsPattern("\.+")

            ls = path_str _
                .EnumerateFiles _
                .Select(Function(f)
                            Dim name As String = f.FileName

                            If is_dot Then
                                Return "/" & name
                            Else
                                Return name
                            End If
                        End Function) _
                .ToArray
        Else
            Dim check_name As String = path_str.BaseName(allowEmpty:=True)
            Dim pos As Integer = check_name.Length

            ls = path_str _
                .ParentPath _
                .EnumerateFiles _
                .Select(Function(f) f.FileName) _
                .Where(Function(f) f.StartsWith(check_name, StringComparison.Ordinal)) _
                .Select(Function(si) si.Substring(pos)) _
                .ToArray
        End If

        Return New Completion(s, ls)
    End Function

    Public Function AutoCompletion(s As String, pos As Integer) As Completion
        Dim prefix As String = Nothing
        Dim ls As String()

        If SMRUCC.Rsharp.Language.Syntax.IsStringOpen(s, prefix) Then
            Return GetFileNamesCompletion(s, prefix)
        Else
            prefix = s.Substring(0, pos)
        End If

        If prefix.StringEmpty Then
            ls = AllSymbols.ToArray
        ElseIf prefix.EndsWith("::") Then
            ' get package functions
            Dim packageName = prefix.Trim(":"c, " "c).Split({" "c, "+"c, "-"c, "*"c, "/"c, "\"c, "?"c, "&"c}).LastOrDefault

            If packageName.StringEmpty Then
                ls = {}
            Else
                ls = packageFunctions(packageName).ToArray
            End If
        Else
            ls = AllSymbols _
                .Where(Function(c) c.StartsWith(prefix, StringComparison.Ordinal)) _
                .Select(Function(c) c.Substring(pos)) _
                .ToArray
        End If

        Return New Completion(prefix, ls)
    End Function
End Class
