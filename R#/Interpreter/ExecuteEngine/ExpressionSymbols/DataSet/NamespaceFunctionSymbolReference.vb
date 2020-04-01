Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop
Imports RPkg = SMRUCC.Rsharp.System.Package.Package

Namespace Interpreter.ExecuteEngine

    Public Class NamespaceFunctionSymbolReference : Inherits Expression

        Public ReadOnly Property [namespace] As String
        Public ReadOnly Property symbol As Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Public Sub New(namespace$, symbol As Expression)
            Me.symbol = symbol
            Me.namespace = [namespace]
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Return getPackageApiImpl(envir, [namespace], symbol)
        End Function

        Private Shared Function getFuncNameSymbolText(funcNameSymbol As Expression, env As Environment) As Object
            Select Case funcNameSymbol.GetType
                Case GetType(Literal)
                    Return DirectCast(funcNameSymbol, Literal).value
                Case GetType(SymbolReference)
                    Return DirectCast(funcNameSymbol, SymbolReference).symbol
                Case Else
                    Return Message.InCompatibleType(GetType(Literal), funcNameSymbol.GetType, env)
            End Select
        End Function

        Friend Shared Function getPackageApiImpl(env As Environment, namespace$, funcNameSymbol As Expression) As Object
            ' find package and then load method
            Dim pkg As RPkg = env.globalEnvironment.packages.FindPackage([namespace], Nothing)
            Dim funcName As Object = getFuncNameSymbolText(funcNameSymbol, env)

            If pkg Is Nothing Then
                Return Message.SymbolNotFound(env, [namespace], TypeCodes.ref)
            ElseIf funcName.GetType Is GetType(Message) Then
                Return funcName
            Else
                Dim funcNameText As String = funcName.ToString
                Dim api As RMethodInfo = pkg.GetFunction(funcNameText)

                If api Is Nothing Then
                    Return Message.SymbolNotFound(env, $"{[namespace]}::{funcNameText}", TypeCodes.closure)
                Else
                    Return DirectCast(api, RFunction)
                End If
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"&{[namespace]}::<{symbol.ToString}>"
        End Function
    End Class
End Namespace