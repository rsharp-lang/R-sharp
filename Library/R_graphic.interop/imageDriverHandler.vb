Imports Microsoft.VisualBasic.Imaging.Driver
Imports SMRUCC.Rsharp.Runtime

Module imageDriverHandler

    Public Function getDriver(env As Environment) As Drivers
        Dim frames = env.stackTrace

        For Each stack In frames
            If stack.Method.Namespace = "graphics" Then
                Select Case LCase(stack.Method.Method)
                    Case "wmf" : Return Drivers.WMF
                    Case "bitmap" : Return Drivers.GDI
                    Case "svg" : Return Drivers.SVG
                    Case Else
                        ' do nothing, and then test
                        ' next frame data
                End Select
            End If
        Next

        Return Drivers.Default
    End Function
End Module
