Imports Microsoft.VisualBasic.CommandLine

Module Env

    ReadOnly args As CommandLine = App.CommandLine

    Public ReadOnly Property debugOpt As Boolean
        Get
            Static is_debug As Boolean? = Nothing

            If is_debug Is Nothing Then
                is_debug = args.HavebFlag("--debug") OrElse args.ContainsParameter("--debug")
            End If

            Return is_debug
        End Get
    End Property
End Module
