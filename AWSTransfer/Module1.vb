Imports Amazon.S3
Imports Amazon
Imports Amazon.S3.Model
Imports System.IO
Imports System.Collections.Specialized
Imports System.Configuration
Imports DataAccessLibrary
Imports Newtonsoft.Json
Imports System.Threading

Module Module1
    Private objMCAPDataAccess As New recordToDatabase()
    Private objConvertFile As New ConvertVideoFiles()
    Private FileUpload As Integer = 0
    Private RemotePath As String = AWSTransfer.My.MySettings.Default.RemotePath
    Private RemotePathBarch As String = AWSTransfer.My.MySettings.Default.BatchImageLocation
    Sub Main()
        Call ProcessCreativesToAWS()
        'Call createCreativeFile()
        Console.WriteLine("{0} Files Succesfully Uploaded", FileUpload)

        Call Play()
    End Sub

    Private Sub ProcessCreativesToAWS()
        Dim dtGetAws As New DataTable()
        Dim CreativeID As Integer
        Dim CreativeAWSID As Integer
        Dim AdID As Integer
        Dim OccurrenceID As Integer
        Dim CreativeDetailID As Integer, MediaType As String, FileType As String
        Dim RemoteCreativeFilePath As String
        Dim RemoteCreativeThumbFilePath As String
        Dim CreativeSignature As String
        Dim StoredFile As String
        Dim DirectoryToStoreFile As String
        Dim IsBatchProcess As Boolean = False


        StoredFile = AWSTransfer.My.MySettings.Default.FileStored
        Dim filename As String = StoredFile + "_" + DateTime.Now.ToString(“dd-MM-yyyy”) + "_" + DateTime.Now.ToString("HHmm") + “.Json”
        DirectoryToStoreFile = Path.GetDirectoryName(filename)

        Try

            dtGetAws = objMCAPDataAccess.GetFilesToTransferToAws()
            Dim i As Integer = 0


            For Each dr As DataRow In dtGetAws.Rows

                ' Dim AwsSpinner = New ConsoleSpinner()
                CreativeID = 0
                AdID = 0
                OccurrenceID = 0
                CreativeDetailID = 0
                CreativeAWSID = 0
                MediaType = ""
                FileType = ""
                RemoteCreativeFilePath = ""
                RemoteCreativeThumbFilePath = ""
                CreativeSignature = ""

                FileType = dr("FileType")
                MediaType = dr("MediaType")
                CreativeID = Convert.ToInt32(dr("CreativeID"))
                If IsDBNull(dr("AdID")) = False Then
                    AdID = Convert.ToInt32(dr("AdID"))
                Else
                    AdID = 0
                End If
                If IsDBNull(dr("OccurrenceID")) = False Then
                    OccurrenceID = dr("OccurrenceID")
                Else
                    OccurrenceID = 0
                End If

                If IsDBNull(dr("CreativeSignature")) = False Then
                    CreativeSignature = dr("CreativeSignature")
                Else
                    CreativeSignature = ""
                End If

                CreativeDetailID = Convert.ToInt32(dr("CreativeDetailID"))

                If IsDBNull(dr("RemoteCreativeFilePath")) = False Then
                    RemoteCreativeFilePath = CleanImageUrl(dr("RemoteCreativeFilePath").tolower, MediaType)
                    'If RemoteCreativeFilePath.Contains(".mp4") = True Then
                    '    Dim NewTVPath As String
                    '    Dim FileToTransfer As String = Path.GetFileName(RemoteCreativeFilePath)
                    '    NewTVPath = RemotePath + MediaType + "\" + CreativeSignature + "\HQ\" + FileToTransfer
                    '    RemoteCreativeFilePath = NewTVPath.ToLower()
                    If RemoteCreativeFilePath.Contains(".mpg") = True Then
                        RemoteCreativeFilePath = Path.ChangeExtension(RemoteCreativeFilePath, ".MP4")
                        If IfFileExist(RemoteCreativeFilePath) = False Then
                            Dim FileToTransfer As String = RemotePathBarch + Path.GetFileName(RemoteCreativeFilePath)
                            If IfFileExist(FileToTransfer) = True Then
                                RemoteCreativeFilePath = FileToTransfer
                                IsBatchProcess = True
                            Else
                                IsBatchProcess = False
                                'RemoteCreativeFilePath = Path.ChangeExtension(RemoteCreativeFilePath, ".mpg")
                            End If

                        End If
                    End If
                End If

                    If IsDBNull(dr("RemoteCreativeThumbFilePath")) = False Then
                    RemoteCreativeThumbFilePath = CleanImageUrl(dr("RemoteCreativeThumbFilePath").tolower, MediaType)
                    If RemoteCreativeThumbFilePath.Contains(".mpg") = True Then
                        RemoteCreativeThumbFilePath = Path.ChangeExtension(RemoteCreativeThumbFilePath, ".MP4")
                        If IfFileExist(RemoteCreativeThumbFilePath) = False Then
                            Dim FileToTransfer As String = RemotePathBarch + Path.GetFileName(RemoteCreativeThumbFilePath)
                            If IfFileExist(FileToTransfer) = True Then
                                IsBatchProcess = True
                                RemoteCreativeThumbFilePath = FileToTransfer
                            Else
                                IsBatchProcess = False
                                'RemoteCreativeThumbFilePath = Path.ChangeExtension(RemoteCreativeThumbFilePath, ".mpg")
                            End If
                        End If
                    End If
                End If
                'HANDLE BATCH

                'If IsDBNull(dr("copied")) = False Then
                '    RemoteCreativeFilePath = Path.ChangeExtension(RemoteCreativeFilePath, ".MP4")
                '    IsBatchProcess = True
                '    If IfFileExist(RemoteCreativeFilePath) = False Then
                '        Dim FileToTransfer As String = RemotePathBarch + Path.GetFileName(RemoteCreativeFilePath)
                '        If IfFileExist(FileToTransfer) = True Then
                '            RemoteCreativeFilePath = FileToTransfer
                '        End If
                '    End If
                'End If



                If IfFileExist(RemoteCreativeThumbFilePath) = False Then
                    RemoteCreativeThumbFilePath = ""

                End If

                'Else
                'RemoteCreativeThumbFilePath = ""



                If IfFileExist(RemoteCreativeFilePath) = True Then
                    If objMCAPDataAccess.InsertImagesToAws(MediaType, AdID, CreativeDetailID, OccurrenceID, CreativeSignature, RemoteCreativeFilePath.ToLower(), RemoteCreativeThumbFilePath.ToLower(), FileType, CreativeID, CreativeAWSID) = True Then
                        Console.WriteLine("File Transfered {0} ", RemoteCreativeFilePath)
                        FileUpload = FileUpload + 1
                    Else

                        Console.WriteLine("File Transfered {0} Failed !", RemoteCreativeFilePath)

                    End If
                        i = i + 1
                Else

                    Console.WriteLine("{0} Not Available", RemoteCreativeFilePath)
                    'objMCAPDataAccess.UpdateFileNotFound(MediaType, CreativeDetailID)
                End If

            Next


        Catch ex As Exception
            Console.WriteLine("Error Executing program")
        End Try

    End Sub
    Public Sub createCreativeFile()
        Dim StoredFile As String
        Dim DirectoryToStoreFile
        Dim RemotePath As String = AWSTransfer.My.MySettings.Default.RemotePath

        StoredFile = AWSTransfer.My.MySettings.Default.FileStored
        Dim filename As String = StoredFile + "_TV_" + DateTime.Now.ToString(“dd-MM-yyyy”) + “.Json”
        DirectoryToStoreFile = Path.GetDirectoryName(filename)
        Dim dtGetAws As New DataTable()
        Dim jsonRecordList As New List(Of JSonRecords)()
        Dim CreativeID As String, AwsCreativeKey As String, AwsThumbnailKey As String, CreativeDomainCode As String, CreativeDomainName As String, FileFormat As String, MimeType As String, PlayerMode As String, OrderID As String, ViewType As String
        Try
            dtGetAws = objMCAPDataAccess.GetFilesToTransferToAws
            Dim i As Integer = 0


            For Each dr As DataRow In dtGetAws.Rows
                Dim objMCAPAws As New JSonRecords()
                CreativeID = ""
                AwsCreativeKey = ""
                AwsThumbnailKey = ""
                CreativeDomainCode = ""
                CreativeDomainName = ""
                FileFormat = ""
                MimeType = ""
                PlayerMode = ""
                OrderID = ""
                ViewType = ""

                If IsDBNull(dr("CreativeId")) = False Then
                    CreativeID = dr("CreativeId")
                Else
                    CreativeID = ""
                End If
                If IsDBNull(dr("AwsCreativeKey")) = False Then
                    AwsCreativeKey = dr("AwsCreativeKey")
                Else
                    AwsCreativeKey = ""
                End If
                If IsDBNull(dr("AwsThumbnailKey")) = False Then
                    AwsThumbnailKey = dr("AwsThumbnailKey")
                Else
                    AwsThumbnailKey = ""
                End If
                If IsDBNull(dr("CreativeDomainCode")) = False Then
                    CreativeDomainCode = dr("CreativeDomainCode")
                Else
                    CreativeDomainCode = ""
                End If
                If IsDBNull(dr("CreativeDomainName")) = False Then
                    CreativeDomainName = dr("CreativeDomainName")
                Else
                    CreativeDomainName = ""
                End If
                If IsDBNull(dr("FileFormat")) = False Then
                    FileFormat = dr("FileFormat")
                Else
                    FileFormat = ""
                End If
                If IsDBNull(dr("MimeType")) = False Then
                    MimeType = dr("MimeType")
                Else
                    MimeType = ""
                End If
                If IsDBNull(dr("ViewType")) = False Then
                    ViewType = dr("ViewType")
                Else
                    ViewType = ""
                End If
                If IsDBNull(dr("PlayerMode")) = False Then
                    PlayerMode = dr("PlayerMode")
                Else
                    PlayerMode = ""
                End If
                If IsDBNull(dr("OrderID")) = False Then
                    OrderID = dr("OrderID")
                Else
                    OrderID = ""
                End If

                objMCAPAws.Creative_id = CreativeID
                objMCAPAws.Aws_Creative_Key = AwsCreativeKey
                objMCAPAws.Aws_Thumbnail_Key = AwsThumbnailKey
                objMCAPAws.Creative_Domain_Code = CreativeDomainCode
                objMCAPAws.Creative_Domain_Name = CreativeDomainName
                objMCAPAws.file_Format = FileFormat
                objMCAPAws.MimeType = MimeType
                objMCAPAws.ViewType = ViewType
                objMCAPAws.Player_Mode = PlayerMode
                objMCAPAws.OrderID = OrderID

                jsonRecordList.Add(objMCAPAws)
                i = i + 1
            Next

            Dim jsonString As String = JsonConvert.SerializeObject(jsonRecordList, Formatting.Indented)
            If IfFileExist(filename) = False Then
                ' serialize JSON to a string and then write string to a file
                File.WriteAllText(filename, jsonString)
            Else
                File.AppendAllText(filename, jsonString)
            End If
        Catch ex As Exception

        End Try

    End Sub
    Private Sub WriteOnBottomLine(text As String)
        Dim x As Integer = Console.CursorLeft
        Dim y As Integer = Console.CursorTop
        Console.CursorTop = Console.WindowTop + Console.WindowHeight - 1
        Console.Write(text)
        ' Restore previous position
        Console.SetCursorPosition(x, y)
    End Sub
    Private Function CleanImageUrl(RemoteCreativeFilePath As String, MediaType As String) As String
        If String.IsNullOrEmpty(RemoteCreativeFilePath) = False Then

            If RemoteCreativeFilePath.Contains("\\") = False Then
                RemoteCreativeFilePath = RemotePath + RemoteCreativeFilePath
            ElseIf RemoteCreativeFilePath.Contains("thumb") = True Then
                RemoteCreativeFilePath = RemoteCreativeFilePath.Replace("thumb", "mid")

            End If
        Else
            RemoteCreativeFilePath = ""
        End If
        Return RemoteCreativeFilePath
    End Function
    Public Sub Play()
        Console.WriteLine(" Program  Will be Closed")
        Dim t As New Timer(AddressOf timerC, Nothing, 6000000, 6000000)
    End Sub

    Private Sub timerC(state As Object)
        Environment.Exit(0)

    End Sub

    Private Function IfFileExist(ByVal fileToCheck As String) As Boolean
        Dim fileExist As Boolean = False
        If File.Exists(fileToCheck) = True Then
            fileExist = True
        End If
        Return fileExist
    End Function

End Module
