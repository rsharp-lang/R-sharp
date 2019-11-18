Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Scripting.MetaData

''' <summary>
''' The R# Graphics Devices and Support for Colours and Fonts
''' </summary>
<Package("grDevices", Category:=APICategories.UtilityTools)>
Public Module grDevices

    Dim devlist As New List(Of IGraphics)
    Dim curDev As IGraphics

    ''' <summary>
    ''' returns the number and name of the new active device 
    ''' (after the specified device has been shut down).
    ''' </summary>
    ''' <param name="which">An integer specifying a device number.</param>
    ''' <returns></returns>
    <ExportAPI("dev.off")>
    Public Function devOff(Optional which% = -1) As Integer

    End Function

    ''' <summary>
    ''' returns a length-one named integer vector giving the number and name of the 
    ''' active device, or 1, the null device, if none is active.
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("dev.cur")>
    Public Function devCur() As Integer

    End Function
End Module
