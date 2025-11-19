Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.CommandLine
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Internal.Object.baseOp

Public Class OptionParser

    Public Property usage As String
    Public Property option_list As OptionParserOption()
    Public Property add_help_option As Boolean = True
    Public Property description As String
    Public Property epilogue As String
    Public Property title As String
    Public Property dependency As String()

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetDocument() As CommandLineDocument
        Return New CommandLineDocument With {
            .arguments = option_list _
                .Select(Function(a) a.CreateArgumentInternal) _
                .ToArray,
            .info = description,
            .title = title,
            .sourceScript = App.CommandLine.Name,
            .dependency = dependency _
                .SafeQuery _
                .Select(Function(name) New Dependency(name)) _
                .ToArray
        }
    End Function

    Public Function getOptions(args As CommandLine,
                               env As Environment,
                               Optional convert_hyphens_to_underscores As Boolean = False) As list

        Dim opts As list = list.empty
        Dim opt_val As Object

        For Each opt As OptionParserOption In option_list.SafeQuery
            Dim value = (From name As String
                         In opt.opt_str
                         Let val = args.GetString(name)
                         Where Not val.StringEmpty
                         Select val).FirstOrDefault

            If value Is Nothing Then
                If opt.opt_str.Any(Function(o) args.HavebFlag(o)) Then
                    If opt.action = "store_true" Then
                        opt_val = True
                    ElseIf opt.action = "store_false" Then
                        opt_val = False
                    Else
                        opt_val = Nothing
                    End If
                Else
                    opt_val = opt.default
                End If
            Else
                opt_val = s4Methods.conversionValue(value, opt.type, env)
            End If

            For Each name As String In opt.opt_names(convert_hyphens_to_underscores)
                Call opts.add(name, opt_val)
            Next
        Next

        Return opts
    End Function

End Class