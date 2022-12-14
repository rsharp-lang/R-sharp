Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports REnv = SMRUCC.Rsharp.Runtime
Imports any = Microsoft.VisualBasic.Scripting

Friend Module ThemeTools

    <Extension>
    Public Function getXAxisLayout(args As list,
                                   Optional name As String = "x.axis.layout",
                                   Optional [default] As XAxisLayoutStyles = XAxisLayoutStyles.Bottom) As XAxisLayoutStyles

        Dim val As Object = REnv.single(args.getByName(name), forceSingle:=True)

        Static enumParser As Dictionary(Of String, XAxisLayoutStyles) =
            Enums(Of XAxisLayoutStyles)() _
            .ToDictionary(Function(a)
                              Return a.ToString.ToLower
                          End Function)

        If TypeOf val Is Integer OrElse TypeOf val Is Byte OrElse TypeOf val Is Long Then
            Return CType(CByte(val), XAxisLayoutStyles)
        ElseIf TypeOf val Is String Then
            Dim valStr As String = any.ToString(val).ToLower

            If enumParser.ContainsKey(valStr) Then
                Return enumParser(valStr)
            Else
                Return [default]
            End If
        ElseIf TypeOf val Is XAxisLayoutStyles Then
            Return val
        Else
            Return [default]
        End If
    End Function

    <Extension>
    Public Function getYAxisLayout(args As list,
                                   Optional name As String = "y.axis.layout",
                                   Optional [default] As YAxisLayoutStyles = YAxisLayoutStyles.Left) As YAxisLayoutStyles

        Dim val As Object = REnv.single(args.getByName(name), forceSingle:=True)

        Static enumParser As Dictionary(Of String, YAxisLayoutStyles) =
            Enums(Of YAxisLayoutStyles)() _
            .ToDictionary(Function(a)
                              Return a.ToString.ToLower
                          End Function)

        If TypeOf val Is Integer OrElse TypeOf val Is Byte OrElse TypeOf val Is Long Then
            Return CType(CByte(val), YAxisLayoutStyles)
        ElseIf TypeOf val Is String Then
            Dim valStr As String = any.ToString(val).ToLower

            If enumParser.ContainsKey(valStr) Then
                Return enumParser(valStr)
            Else
                Return [default]
            End If
        ElseIf TypeOf val Is YAxisLayoutStyles Then
            Return val
        Else
            Return [default]
        End If
    End Function
End Module