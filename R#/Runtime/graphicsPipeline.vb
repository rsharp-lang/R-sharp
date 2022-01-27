Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime

    Public Module graphicsPipeline

        <Extension>
        Public Function CheckDpiArgument(args As Dictionary(Of String, Object)) As Boolean
            Return {"dpi", "Dpi", "DPI", "PPI", "ppi"}.Any(AddressOf args.ContainsKey)
        End Function

        Public Function getDpi(args As Dictionary(Of String, Object), env As Environment, [default] As Integer) As Integer
            Dim raw As Object

            If args.ContainsKey("dpi") Then
                raw = args("dpi")
            ElseIf args.ContainsKey("Dpi") Then
                raw = args("Dpi")
            ElseIf args.ContainsKey("DPI") Then
                raw = args("DPI")
            ElseIf args.ContainsKey("PPI") Then
                raw = args("PPI")
            ElseIf args.ContainsKey("ppi") Then
                raw = args("ppi")
            Else
                Return [default]
            End If

            If TypeOf raw Is InvokeParameter Then
                Return REnv.single(REnv.asVector(Of Integer)(eval(raw, env)))
            Else
                Return REnv.single(REnv.asVector(Of Integer)(raw))
            End If
        End Function

        Private Sub getSize(width As Object, height As Object, env As Environment, ByRef w As Double, ByRef h As Double)
            If TypeOf width Is Expression Then
                width = DirectCast(width, Expression).Evaluate(env)
            ElseIf TypeOf width Is InvokeParameter Then
                width = eval(width, env)
            End If
            If TypeOf height Is Expression Then
                height = DirectCast(height, Expression).Evaluate(env)
            ElseIf TypeOf height Is InvokeParameter Then
                height = eval(height, env)
            End If

            w = REnv.single(REnv.asVector(Of Double)(width))
            h = REnv.single(REnv.asVector(Of Double)(height))
        End Sub

        <Extension>
        Public Function CheckSizeArgument(args As Dictionary(Of String, Object)) As Boolean
            Return {"w", "h"}.All(AddressOf args.ContainsKey) OrElse
                {"width", "height"}.All(AddressOf args.ContainsKey) OrElse
                args.ContainsKey("size")
        End Function

        <Extension>
        Public Function getSize(args As Dictionary(Of String, Object), env As Environment, [default] As SizeF) As SizeF
            Dim w# = 1920
            Dim h# = 1600

            If {"w", "h"}.All(AddressOf args.ContainsKey) Then
                Call getSize(args("width"), args("height"), env, w, h)
            ElseIf {"width", "height"}.All(AddressOf args.ContainsKey) Then
                Call getSize(args("width"), args("height"), env, w, h)
            ElseIf args.ContainsKey("size") Then
                Return getSize(args("size"), env, $"{[default].Width},{[default].Height}").FloatSizeParser
            Else
                Throw New MissingPrimaryKeyException
            End If

            Return New SizeF(w, h)
        End Function

        Private Function eval(x As Object, env As Environment) As Object
            If TypeOf x Is InvokeParameter Then
                x = DirectCast(x, InvokeParameter).value
            End If

            If TypeOf x Is ValueAssignExpression Then
                x = DirectCast(x, ValueAssignExpression).value
            End If

            Return DirectCast(x, Expression).Evaluate(env)
        End Function

        Public Function getSize(size As Object, env As Environment, Optional default$ = "2700,2000") As String
            If size Is Nothing Then
                Return [default]
            ElseIf TypeOf size Is vector Then
                size = DirectCast(size, vector).data
            ElseIf TypeOf size Is InvokeParameter OrElse TypeOf size Is Expression Then
                Return getSize(eval(size, env), env, [default])
            End If

            If size.GetType.IsArray Then
                ' cast object() to integer()/string(), etc
                size = REnv.TryCastGenericArray(size, env)
            End If

            Dim sizeType As Type = size.GetType

            Select Case sizeType
                Case GetType(String)
                    Return size
                Case GetType(String())
                    Dim strs As String() = DirectCast(size, String())

                    If strs.Length = 1 Then
                        Return strs(Scan0)
                    ElseIf strs(Scan0).IsNumeric AndAlso strs(1).IsNumeric Then
                        Return $"{strs(Scan0)},{strs(1)}"
                    Else
                        Return [default]
                    End If
                Case GetType(Size)
                    With DirectCast(size, Size)
                        Return $"{ .Width},{ .Height}"
                    End With
                Case GetType(SizeF)
                    With DirectCast(size, SizeF)
                        Return $"{ .Width},{ .Height}"
                    End With
                Case GetType(Integer()), GetType(Long()), GetType(Single()), GetType(Double()), GetType(Short())
                    With DirectCast(size, Array)
                        Return $"{ .GetValue(0)},{ .GetValue(1)}"
                    End With
                Case Else
                    Call $"invalid data type for get [width,height]: {sizeType.FullName}".Warning

                    Return [default]
            End Select
        End Function
    End Module
End Namespace