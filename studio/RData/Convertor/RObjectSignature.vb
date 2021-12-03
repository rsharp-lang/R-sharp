Imports SMRUCC.Rsharp.RDataSet.Flags
Imports SMRUCC.Rsharp.RDataSet.Struct.LinkedList

Namespace Convertor

    Public Class RObjectSignature

        Public Shared Function IsPairList(robj As RObject) As Boolean
            Dim attrs As RObject = robj.attributes

            If attrs Is Nothing Then
                If robj.info.type Like ConvertToR.elementVectorFlags Then
                    Return False
                Else
                    Return True
                End If
            End If

            If attrs.LinkVisitor("row.names") IsNot Nothing Then
                Return False
            End If

            Return True
        End Function

        Public Shared Function IsDataFrame(robj As RObject) As Boolean
            Dim attrs As RObject = robj.attributes

            If attrs Is Nothing Then
                Return False
            ElseIf robj.info.type Like ConvertToR.elementVectorFlags Then
                Return False
            End If

            If attrs.tag.referenced_object IsNot Nothing AndAlso attrs.tag.referenced_object.characters = "names" Then
                Return True
            End If

            Return True
        End Function

        Public Shared Function HasFactor(robj As RObject) As Boolean
            Dim attrs As RObject = robj.attributes

            If attrs Is Nothing Then
                Return False
            End If

            If attrs.tag.info.type <> RObjectType.SYM AndAlso attrs.tag.info.type <> RObjectType.REF Then
                Return False
            End If

            If attrs.tag.info.type = RObjectType.SYM AndAlso attrs.tag.characters <> "levels" Then
                Return False
            End If
            If attrs.tag.info.type = RObjectType.REF AndAlso attrs.tag.referenced_object.characters <> "levels" Then
                Return False
            End If

            Return True
        End Function

    End Class
End Namespace