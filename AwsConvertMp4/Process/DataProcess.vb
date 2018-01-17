Imports System.Data.SqlClient
Imports MT.SpendFramework.EnterpriseLibrary
Public Class DataProcess
    Dim objAccess As New DataAccess()
    Public Function GetGetFilesToConvert() As DataTable
        Try
            Dim SearchValue As String
            Dim dt As New DataTable()
            dt = objAccess.ExcecuteDataTable("sp_AWSGetCreativesToConvert", New String() {SearchValue})
            Return dt
        Catch ex As Exception
            ExceptionLogging.LogErrorMessage(ex)
            Throw ex
        End Try
    End Function

    Public Function GetFilesToConvertToAws() As DataTable
        Try
            Dim SearchValue As String
            Dim dt As New DataTable()
            ' dt = objAccess.ExcecuteDataTable("sp_GetCreativesForAws", New String() {SearchValue})
            'dt = objAccess.ExcecuteDataTable("sp_GetCreativesForAwsOnd", New String() {SearchValue})
            'dt = objAccess.ExcecuteDataTable("sp_GetCreativesForAwsPrint", New String() {SearchValue})
            'dt = objAccess.ExcecuteDataTable("sp_GetCreativesForAwsTVBBatch", New String() {SearchValue})
            'dt = objAccess.ExcecuteDataTable("sp_GetCreativesForAwsONDBBatch", New String() {SearchValue})

            Dim cmd As System.Data.SqlClient.SqlCommand
            Dim cn As System.Data.SqlClient.SqlConnection
            Dim csting As String

            csting = My.Settings.MCAPConnectionString.ToString

            cn = New System.Data.SqlClient.SqlConnection(csting)
            cmd = New System.Data.SqlClient.SqlCommand
            cn.Open()
            cmd.Connection = cn
            cmd.CommandType = CommandType.StoredProcedure

            cmd.CommandText = "sp_AWSGetCreativesToConvert"
            Using cnn As New SqlConnection(csting)
                cnn.Open()
                Using dad As New SqlDataAdapter(cmd)
                    dad.SelectCommand.CommandTimeout = 3000
                    dad.Fill(dt)
                End Using
                cnn.Close()
            End Using

            Return dt
        Catch ex As Exception
            ExceptionLogging.LogErrorMessage(ex)
            Throw ex
        End Try
    End Function
    Public Function UpdateConvertedData(MediaType As String, creativeDetailID As Integer, FileName As String) As Boolean
        Try
            objAccess.ExcecuteDataTable("sp_AWSCreativesConvertedToBeTransfered", New String() {MediaType, creativeDetailID.ToString, FileName})
        Catch ex As Exception
            ExceptionLogging.LogErrorMessage(ex)
            Return False
            Throw ex
        End Try
        Return True
    End Function

    Public Function UpdateFileNotFound(MediaType As String, creativeDetailID As Integer) As Boolean
        Try
            objAccess.ExcecuteDataTable("sp_RemoveConvertedNotFound", New String() {MediaType, creativeDetailID.ToString})
        Catch ex As Exception
            ExceptionLogging.LogErrorMessage(ex)
            Return False
            Throw ex
        End Try
        Return True
    End Function
End Class
