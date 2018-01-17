Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Data
Imports System.Data.SqlClient
Imports MT.SpendFramework.EnterpriseLibrary
Imports Microsoft.Practices.EnterpriseLibrary.Data.Sql
Imports Microsoft.Practices.EnterpriseLibrary.Data
Imports DataAccessLibrary
Imports OneMTDataAccess.Access

Public Class recordToDatabase
    Dim objAccess As New DataAccess()

    Public recordList As New List(Of awsProcess)()
    Public Function GetGetFilesToTransfer() As DataTable
        Try
            Dim SearchValue As String
            Dim dt As New DataTable()
            'dt = objAccess.ExcecuteDataTable("sp_GetCreativesForAws", New String() {SearchValue})
            'dt = objAccess.ExcecuteDataTable("sp_GetCreativesForAwsOnd", New String() {SearchValue})
            'dt = objAccess.ExcecuteDataTable("sp_GetCreativesForAwsPrint", New String() {SearchValue})
            'dt = objAccess.ExcecuteDataTable("sp_GetCreativesForAwsTVBBatch", New String() {SearchValue})
            dt = objAccess.ExcecuteDataTable("sp_GetCreativesForAwsThmbnlTV", New String() {SearchValue})
            'dt = objAccess.ExcecuteDataTable("sp_GetCreativesForAwsONDBBatch", New String() {SearchValue})

            Return dt
        Catch ex As Exception
            ExceptionLogging.LogErrorMessage(ex)
            Throw ex
        End Try
    End Function

    Public Function GetFilesToTransferToAws() As DataTable
        Try
            Dim SearchValue As String
            Dim dt As New DataTable()
            ' dt = objAccess.ExcecuteDataTable("sp_GetCreativesForAws", New String() {SearchValue})
            'dt = objAccess.ExcecuteDataTable("sp_GetCreativesForAwsOnd", New String() {SearchValue})
            'dt = objAccess.ExcecuteDataTable("sp_GetCreativesForAwsPrint", New String() {SearchValue})
            'dt = objAccess.ExcecuteDataTable("sp_GetCreativesForAwsTVBBatch", New String() {SearchValue})
            'dt = objAccess.ExcecuteDataTable("sp_GetCreativesForAwsONDBBatch", New String() {SearchValue})
            ' dt = objAccess.ExcecuteDataTable("sp_GetCreativesForAwsAVI", New String() {SearchValue})

            Dim cmd As System.Data.SqlClient.SqlCommand
            Dim cn As System.Data.SqlClient.SqlConnection
            Dim csting As String

            csting = My.Settings.MCAPConnectionString.ToString

            cn = New System.Data.SqlClient.SqlConnection(csting)
            cmd = New System.Data.SqlClient.SqlCommand
            cn.Open()
            cmd.Connection = cn
            cmd.CommandType = CommandType.StoredProcedure

            cmd.CommandText = "sp_GetCreativesForAwsOND"
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


    Public Function GetGetExportedFilesToTransfer() As DataTable
        Try
            Dim SearchValue As String
            Dim dt As New DataTable()
            dt = objAccess.ExcecuteDataTable("sp_GetExportCreativesForAws", New String() {SearchValue})
            Return dt
        Catch ex As Exception
            ExceptionLogging.LogErrorMessage(ex)
            Throw ex
        End Try
    End Function
    Public Function InsertImagesToAws(MediaType As String, AdID As Integer, CreativeDetailID As Integer, OccurrenceID As Integer, CreativeSignature As String, RemoteCreativeFilePath As String, RemoteCreativeThumbFilePath As String, FileType As String, creativeId As Integer, CreativeAWSID As Integer) As [Boolean]
        Try

            Dim objMCAPAws As New awsProcess()
            Dim AwsCreativeKey As String, AwsThumbnailKey As String, CreativeDomainCode As String, CreativeDomainName As String, FileFormat As String, MimeType As String, PlayerMode As String, OrderID As Integer, ViewType As String

            If objMCAPAws.TransferProcess(MediaType, AdID, CreativeDetailID, OccurrenceID, CreativeSignature, RemoteCreativeFilePath, RemoteCreativeThumbFilePath, FileType) = True Then

                CreativeDomainName = "brand-n"
                CreativeDomainCode = AdID
                objMCAPAws.Creative_Domain_Code = CreativeDomainCode
                objMCAPAws.Creative_Domain_Name = CreativeDomainName
                objMCAPAws.Creative_id = creativeId
                AwsCreativeKey = objMCAPAws.Aws_Creative_Key
                If String.IsNullOrEmpty(objMCAPAws.Aws_Thumbnail_Key) = False Then
                    AwsThumbnailKey = objMCAPAws.Aws_Thumbnail_Key
                Else
                    AwsThumbnailKey = ""
                End If
                FileFormat = objMCAPAws.file_Format
                MimeType = objMCAPAws.MimeType
                If String.IsNullOrEmpty(objMCAPAws.Player_Mode) = False Then
                    PlayerMode = objMCAPAws.Player_Mode
                Else
                    PlayerMode = ""
                End If
                OrderID = objMCAPAws.OrderID
                ViewType = objMCAPAws.ViewType

                If objMCAPAws.Tranfered() = True Then
                    'objAccess.ExcecuteDataTable("sp_CreativesSentToAWS", New String() {MediaType, AdID.ToString, CreativeDetailID.ToString, AwsCreativeKey, AwsThumbnailKey, CreativeDomainCode, CreativeDomainName, FileFormat, MimeType, PlayerMode, OrderID, OccurrenceID.ToString, CreativeSignature, ViewType})
                    LogInsertToAws(MediaType, AdID.ToString, CreativeDetailID.ToString, AwsCreativeKey.Replace("//", "/"), AwsThumbnailKey.Replace("//", "/"), CreativeDomainCode, CreativeDomainName, FileFormat, MimeType, PlayerMode, OrderID, OccurrenceID.ToString, CreativeSignature, ViewType, creativeId)
                    'LogUpdateThmbnlToAws(CreativeAWSID, AwsCreativeKey.Replace("//", "/"), AwsThumbnailKey.Replace("//", "/"), RemoteCreativeFilePath)
                    recordList.Add(objMCAPAws)
                Else
                    Return False
                End If
            Else
                Return False

            End If
            Return True
        Catch ex As Exception
            ExceptionLogging.LogErrorMessage(ex)
            Return False
            Throw ex
        End Try
    End Function

    Public Function UpdateConvertedData(MediaType As String, creativeDetailID As Integer, fileType As String, FileName As String) As Boolean
        Try
            objAccess.ExcecuteDataTable("sp_UpdateCreativeDataAWS", New String() {MediaType, fileType, creativeDetailID.ToString, FileName})
        Catch ex As Exception
            ExceptionLogging.LogErrorMessage(ex)
            Return False
            Throw ex
        End Try
        Return True
    End Function
    Public Function UpdateFileNotFound(MediaType As String, creativeDetailID As Integer) As Boolean
        Try
            objAccess.ExcecuteDataTable("sp_FileNotFoundForAWS", New String() {MediaType, creativeDetailID.ToString})
        Catch ex As Exception
            ExceptionLogging.LogErrorMessage(ex)
            Return False
            Throw ex
        End Try
        Return True
    End Function

    Public Function LogInsertToAws(MediaType As String, AdID As String, CreativeDetailID As String, AwsCreativeKey As String, AwsThumbnailKey As String, CreativeDomainCode As String, CreativeDomainName As String, FileFormat As String, MimeType As String, PlayerMode As String, OrderID As String, OccurrenceID As String, CreativeSignature As String, ViewType As String, CreativeID As Integer) As Boolean
        Dim obj As Object
        Dim val As Integer

        Dim cmd As System.Data.SqlClient.SqlCommand
        Dim cn As System.Data.SqlClient.SqlConnection
        Dim csting As String

        csting = My.Settings.MCAPConnectionString.ToString

        cn = New System.Data.SqlClient.SqlConnection(csting)
        cmd = New System.Data.SqlClient.SqlCommand
        cn.Open()
        cmd.Connection = cn
        cmd.CommandType = CommandType.StoredProcedure

        cmd.CommandText = "sp_CreativesSentToAWS"
        'cmd.Parameters.Add("@VehicleId", SqlDbType.Int).Direction = ParameterDirection.Output


        cmd.Parameters.AddWithValue("@MediaType", MediaType)
        cmd.Parameters.AddWithValue("@AdID", AdID)
        cmd.Parameters.AddWithValue("@CreativeID", CreativeID)
        cmd.Parameters.AddWithValue("@CreativeDetailID", CreativeDetailID)
        cmd.Parameters.AddWithValue("@AwsCreativeKey", AwsCreativeKey)
        cmd.Parameters.AddWithValue("@AwsThumbnailKey", AwsThumbnailKey)
        cmd.Parameters.AddWithValue("@CreativeDomainCode", CreativeDomainCode)
        cmd.Parameters.AddWithValue("@CreativeDomainName", CreativeDomainName)
        cmd.Parameters.AddWithValue("@FileFormat", FileFormat)
        cmd.Parameters.AddWithValue("@MimeType", MimeType)
        cmd.Parameters.AddWithValue("@PlayerMode", PlayerMode)
        cmd.Parameters.AddWithValue("@OrderID", OrderID)
        cmd.Parameters.AddWithValue("@OccurrenceID ", OccurrenceID)
        cmd.Parameters.AddWithValue("@CreativeSignature", CreativeSignature)
        cmd.Parameters.AddWithValue("@ViewType", ViewType)


        obj = cmd.ExecuteScalar()

        val = CType(obj, Integer)

        If cmd.Connection.State = ConnectionState.Open Then
            cmd.Connection.Close()
        End If
        cmd = Nothing

        Return val
    End Function


    Public Function LogUpdateThmbnlToAws(CreativeAWSID As String, AwsCreativeKey As String, AwsThumbnailKey As String, FilePath As String) As Boolean
        Dim obj As Object
        Dim val As Integer

        Dim cmd As System.Data.SqlClient.SqlCommand
        Dim cn As System.Data.SqlClient.SqlConnection
        Dim csting As String

        csting = My.Settings.MCAPConnectionString.ToString

        cn = New System.Data.SqlClient.SqlConnection(csting)
        cmd = New System.Data.SqlClient.SqlCommand
        cn.Open()
        cmd.Connection = cn
        cmd.CommandType = CommandType.StoredProcedure

        cmd.CommandText = "sp_CreativesThmbnlSentToAWS"
        'cmd.Parameters.Add("@VehicleId", SqlDbType.Int).Direction = ParameterDirection.Output


        cmd.Parameters.AddWithValue("@CreativeAWSID", CreativeAWSID)
        cmd.Parameters.AddWithValue("@AwsCreativeKey", AwsCreativeKey)
        cmd.Parameters.AddWithValue("@AwsThumbnailKey", AwsThumbnailKey)
        cmd.Parameters.AddWithValue("@FilePath", FilePath)

        obj = cmd.ExecuteScalar()

        val = CType(obj, Integer)

        If cmd.Connection.State = ConnectionState.Open Then
            cmd.Connection.Close()
        End If
        cmd = Nothing

        Return val
    End Function
End Class
