Imports System.ComponentModel

Namespace Runtime.Internal.Invokes

    Public Enum varUseMethods

        everything

        <Description("all.obs")> all_obs
        <Description("complete.obs")> complete_obs
        <Description("na.or.complete")> na_or_complete
        <Description("pairwise.complete.obs")> pairwise_complete_obs

    End Enum
End Namespace