Imports Microsoft.VisualBasic.Net
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Context.RPC

    ''' <summary>
    ''' a slow accessed R# runtime environment based on the tcp networking
    ''' </summary>
    Public Class RemoteEnvironment : Inherits Environment

        ReadOnly master As IPEndPoint

        Sub New(master As IPEndPoint, parent As Environment)
            Call MyBase.New(parent, stackName:=master.ToString, isInherits:=False)
        End Sub

        ''' <summary>
        ''' find symbol at local first and then find symbol 
        ''' via tcp connection from remote
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="[inherits]"></param>
        ''' <returns></returns>
        Public Overrides Function FindSymbol(name As String, Optional [inherits] As Boolean = True) As Symbol
            Return MyBase.FindSymbol(name, [inherits])
        End Function
    End Class
End Namespace