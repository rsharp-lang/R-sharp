Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Development.CodeAnalysis

    ''' <summary>
    ''' analysis of the static symbol reference
    ''' </summary>
    Public Class SymbolAnalysis

        Dim envir As String
        Dim declareSymbols As New Dictionary(Of String, PropertyAccess)
        Dim parent As SymbolAnalysis

        Sub New(Optional name As String = "global")
            envir = name
        End Sub

        Sub New(parent As SymbolAnalysis, name As String)
            Me.envir = name
            Me.parent = parent
        End Sub

        Public Iterator Function GetNameEnums() As IEnumerable(Of NamedValue(Of PropertyAccess))
            For Each symbolRef In declareSymbols
                Yield New NamedValue(Of PropertyAccess)(symbolRef.Key, symbolRef.Value, envir)
            Next
        End Function

        Public Sub Push(symbolName As String)
            declareSymbols.Add(symbolName, PropertyAccess.Writeable)
        End Sub

        Public Function SymbolAccess(symbolName As String, ByRef context As SymbolAnalysis) As PropertyAccess
            If declareSymbols.ContainsKey(symbolName) Then
                context = Me
                Return declareSymbols(symbolName)
            ElseIf Not parent Is Nothing Then
                Return parent.SymbolAccess(symbolName, context)
            Else
                context = Nothing
                Return PropertyAccess.NotSure
            End If
        End Function

        Public Overrides Function ToString() As String
            Return envir
        End Function

        Public Shared Function GetSymbolReferenceList(code As Expression, Optional envir As String = "global") As IEnumerable(Of NamedValue(Of PropertyAccess))
            Dim ref As New List(Of NamedValue(Of PropertyAccess))
            Dim context As New Context With {
                .context = New SymbolAnalysis(envir),
                .ref = ref
            }

            Call GetSymbolReferenceList(code, context)

            Return ref
        End Function

        Private Shared Sub GetSymbolReferenceList(code As Expression, context As Context)
            If code Is Nothing OrElse TypeOf code Is Literal Then
                Return
            End If

            Select Case code.GetType
                Case GetType(BinaryExpression),
                     GetType(BinaryInExpression),
                     GetType(BinaryOrExpression)

                    Call GetSymbols(DirectCast(code, IBinaryExpression), context)

                Case GetType(FunctionInvoke) : Call GetSymbols(DirectCast(code, FunctionInvoke), context)
                Case GetType(DeclareNewFunction) : Call GetSymbols(DirectCast(code, DeclareNewFunction), context)
                Case GetType(DeclareNewSymbol) : Call GetSymbols(DirectCast(code, DeclareNewSymbol), context)
                Case GetType(AcceptorClosure), GetType(ClosureExpression) : Call GetSymbols(DirectCast(code, ClosureExpression), context)

                Case Else
                    Throw New NotImplementedException(code.GetType.FullName)
            End Select
        End Sub

        Private Shared Sub GetSymbols(code As ClosureExpression, context As Context)
            For Each line As Expression In code.EnumerateCodeLines
                Call GetSymbolReferenceList(line, context)
            Next
        End Sub

        Private Shared Sub GetSymbols(code As DeclareNewFunction, context As Context)
            Call context.Push(code.funcName, PropertyAccess.Writeable)

            context = New Context(context, code.funcName)

            For Each arg In code.parameters
                Call GetSymbols(arg, context)
            Next

            context.ref.AddRange(context.context.GetNameEnums)
        End Sub

        Private Shared Sub GetSymbols(code As DeclareNewSymbol, context As Context)
            For Each name As String In code.names
                Call context.Push(name, PropertyAccess.Writeable)
            Next

            If Not code.value Is Nothing Then
                Call GetSymbolReferenceList(code.value, context)
            End If
        End Sub

        Private Shared Sub GetSymbols(code As FunctionInvoke, context As Context)
            If TypeOf code.funcName Is SymbolReference Then
                Call context.Push(code.funcName, PropertyAccess.Readable)
            ElseIf TypeOf code.funcName Is Literal Then
                Call context.Push(DirectCast(code.funcName, Literal).ValueStr, PropertyAccess.Readable)
            Else
                Call GetSymbolReferenceList(code.funcName, context)
            End If

            For Each arg As Expression In code.parameters
                If TypeOf arg Is SymbolReference Then
                    Call context.Push(arg, PropertyAccess.Readable)
                Else
                    Call GetSymbolReferenceList(arg, context)
                End If
            Next
        End Sub

        Private Shared Sub GetSymbols(code As IBinaryExpression, context As Context)
            If TypeOf code.left Is SymbolReference Then
                Call context.Push(code.left, PropertyAccess.Readable)
            Else
                Call GetSymbolReferenceList(code.left, context)
            End If

            If TypeOf code.right Is SymbolReference Then
                Call context.Push(code.right, PropertyAccess.Readable)
            Else
                Call GetSymbolReferenceList(code.right, context)
            End If
        End Sub

        Private Class Context

            Public context As SymbolAnalysis
            Public ref As List(Of NamedValue(Of PropertyAccess))

            Sub New()
            End Sub

            Sub New(parent As Context, name As String)
                ref = parent.ref
                context = New SymbolAnalysis(parent.context, name)
            End Sub

            Public Sub Push(symbol As SymbolReference, access As PropertyAccess)
                Call Push(symbol.symbol, access)
            End Sub

            Public Sub Push(symbol As String, access As PropertyAccess)
                Dim context As SymbolAnalysis = Me.context
                Dim currentAccess As PropertyAccess = context.SymbolAccess(symbol, context)

                If currentAccess = PropertyAccess.NotSure Then
                    ref.Add(New NamedValue(Of PropertyAccess)(symbol, access, "global"))
                Else
                    If currentAccess <> access AndAlso currentAccess <> PropertyAccess.ReadWrite Then
                        context.declareSymbols(symbol) = PropertyAccess.ReadWrite
                    End If
                End If
            End Sub
        End Class
    End Class
End Namespace