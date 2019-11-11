Imports SMRUCC.Rsharp.Runtime.Components.Configuration
Imports SMRUCC.Rsharp.Runtime.Package

Namespace Runtime

    Public Class GlobalEnvironment : Inherits Environment

        Public ReadOnly Property options As Options
        Public ReadOnly Property packages As LocalPackageDatabase

        Sub New(packages As LocalPackageDatabase, options As Options)
            Me.packages = packages
            Me.options = options
        End Sub

    End Class
End Namespace