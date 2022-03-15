#Region "Microsoft.VisualBasic::18ac65adb5b104ad39321aff613fb16d, R-sharp\studio\njl\Runtime\math.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


     Code Statistics:

        Total Lines:   37
        Code Lines:    27
        Comment Lines: 5
        Blank Lines:   5
        File Size:     1.33 KB


    ' Module math
    ' 
    '     Function: zero
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("math")>
Module math

    ''' <summary>
    ''' Get the additive identity element for the type of x (x can also specify the type itself).
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    <ExportAPI("zero")>
    Public Function zero(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
        Dim vec As Array = REnv.TryCastGenericArray(REnv.asVector(Of Object)(x), env)
        Dim type As RType = RType.GetRSharpType(vec.GetType.GetElementType)
        Dim defaultVal As Object = Nothing

        Select Case type.mode
            Case TypeCodes.boolean : defaultVal = False
            Case TypeCodes.double : defaultVal = 0.0
            Case TypeCodes.integer : defaultVal = 0
            Case TypeCodes.string : defaultVal = ""
            Case Else
                defaultVal = Nothing
        End Select

        Return Enumerable _
            .Range(0, vec.Length) _
            .Select(Function(any) defaultVal) _
            .ToArray
    End Function

End Module
