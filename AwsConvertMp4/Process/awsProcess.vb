Imports Amazon.S3
Imports Amazon
Imports Amazon.S3.Model
Imports System.IO
Imports System.Collections.Specialized
Imports System.Configuration
Public Class awsProcess

    Private BUCKET_NAME As String = AWSTransfer.My.MySettings.Default.BucketName
    Private Const S3_KEY As String = "s3_key"
    Public Property Creative_id As Integer
    Public Property Aws_Creative_Key As String
    Public Property Aws_Thumbnail_Key As String
    Public Property Creative_Domain_Name As String
    Public Property Creative_Domain_Code As String

    Public Property file_Format As String
    Public Property MimeType As String
    Public Property ViewType As String
    Public Property Player_Mode As String
    Public Property OrderID As Integer
    Private Property DefaultAWSPath As String = AWSTransfer.My.MySettings.Default.DefaultAWSPath

    Private Sub init()
        Creative_id = 0
        Aws_Creative_Key = ""
        Aws_Thumbnail_Key = ""
        Creative_Domain_Code = ""
        Creative_Domain_Name = ""
        file_Format = ""
        MimeType = ""
        ViewType = ""
        Player_Mode = ""
        OrderID = 0

    End Sub

    Public Function GetS3Client() As AmazonS3Client

        Dim AWSAccessKey = AWSTransfer.My.MySettings.Default.AWSAccessKey
        Dim AWSSecretKey = AWSTransfer.My.MySettings.Default.AWSSecretKey
        Dim s3Client As AmazonS3Client = New AmazonS3Client(AWSAccessKey, AWSSecretKey, Amazon.RegionEndpoint.USEast1)

        Return s3Client
    End Function

    Private Function UrlAwsFormat(ByVal url As String) As String
        Return url.Replace("\", "/")
    End Function

    Private Function PathURLFormat(ByVal url As String) As String
        Return url.Replace("/", "\")
    End Function

    Public Sub CreateNewFolder(client As AmazonS3Client, Path As String)
        Dim S3_KEY As [String] = DefaultAWSPath + Path
        Dim request As New PutObjectRequest()
        request.BucketName = BUCKET_NAME
        request.Key = S3_KEY
        request.ContentBody = "CreativeAssets"
        client.PutObject(request)
    End Sub

    Public Sub CreateNewFileInFolder(client As AmazonS3Client)

        Dim S3_KEY As [String] = "Demo Create Folder/" + "Demo Create File.txt"
        Dim request As New PutObjectRequest()
        request.BucketName = BUCKET_NAME
        request.Key = S3_KEY
        request.ContentBody = "This is body of S3 object."
        client.PutObject(request)
    End Sub


    Public Sub UploadFile(client As AmazonS3Client, FromPath As String, ToPath As String)

        Dim contentType As String
        Dim objTaggList As List(Of Tag)

        objTaggList = New List(Of Tag)() From {New Tag() With {
            .Key = "cft-createdby QC UPLOADER",
            .Value = "cft-createdate " + Now().Date
        }}

        contentType = GetContentType(Path.GetExtension(FromPath))


        'S3_KEY is name of file we want upload
        Dim request As New PutObjectRequest()
        request.BucketName = BUCKET_NAME
        request.Key = DefaultAWSPath + ToPath
        request.ContentType = contentType
        request.CannedACL = S3CannedACL.Private
        request.FilePath = FromPath
        request.StorageClass = "Standard"
        request.TagSet = objTaggList

        'UPload Images 
        client.PutObject(request)

        'Process to update Logg
        file_Format = Path.GetExtension(FromPath).Replace(".", "")
        MimeType = contentType
        Aws_Creative_Key = request.Key
        ViewType = GetViewType(Path.GetExtension(FromPath.ToLower))

    End Sub

    Public Function FolderExistCheck(Path As String) As Boolean
        Using s3Client As AmazonS3Client = GetS3Client()
            Try

                Dim findFolderRequest As New ListObjectsRequest()
                findFolderRequest.BucketName = BUCKET_NAME
                'findFolderRequest.Delimiter = "/";
                findFolderRequest.Prefix = DefaultAWSPath + Path
                Dim findFolderResponse As ListObjectsResponse = s3Client.ListObjects(findFolderRequest)
                Dim commonPrefixes As List(Of [String]) = findFolderResponse.CommonPrefixes
                Dim folderExists As [Boolean] = commonPrefixes.Any()
                Return folderExists
            Catch e As AmazonS3Exception
                Return False
                Console.WriteLine("Folder existence check has failed.")
                Console.WriteLine("Amazon error code: {0}", If(String.IsNullOrEmpty(e.ErrorCode), "None", e.ErrorCode))
                Console.WriteLine("Exception message: {0}", e.Message)
            End Try
        End Using
    End Function

    Public Function FileExistCheck(Path As String) As Boolean
        Dim SKey As String = DefaultAWSPath + Path
        Using client As AmazonS3Client = GetS3Client()
            Dim request As New ListObjectsRequest()
            request.BucketName = BUCKET_NAME
            request.Prefix = SKey
            Dim response As ListObjectsResponse = client.ListObjects(request)

            For Each o As S3Object In response.S3Objects
                    If o.Key = SKey Then
                        Return True
                    End If
                Next
            Return False
        End Using
    End Function

    Public Function TransferProcess(MediaType As String, AdID As Integer, CreativeDetailID As Integer, OccurrenceID As Integer, CreativeSignature As String, RemoteCreativeFilePath As String, RemoteCreativeThumbFilePath As String) As Boolean
        Dim FilePath, toPath, toPathThumb As String
        Dim RemotePath As String = AWSTransfer.My.MySettings.Default.RemotePath
        FilePath = RemoteCreativeFilePath
        toPath = UrlAwsFormat(RemoteCreativeFilePath).Substring(26)
        If MediaType = "TV" Then
            Dim TVFolderPath As String
            TVFolderPath = "/TV/" + CreativeSignature + "/" + GetTVFolder(Path.GetExtension(RemoteCreativeFilePath.ToLower)) + "/"
            If FolderExistCheck(TVFolderPath) = False Then
                CreateNewFolder(GetS3Client(), TVFolderPath)
            End If
            Dim NewFileName = Path.GetFileName(FilePath)
            TVFolderPath = TVFolderPath + NewFileName

            If FileExistCheck(TVFolderPath) = False Then
                    UploadFile(GetS3Client(), FilePath, TVFolderPath)
                End If
            Else


                If String.IsNullOrEmpty(RemoteCreativeThumbFilePath) = False Then
                toPathThumb = UrlAwsFormat(RemoteCreativeThumbFilePath).Substring(26)
            End If

            If FolderExistCheck(UrlAwsFormat(FilePath).Substring(26)) = True Then
                UploadFile(GetS3Client(), FilePath, toPath)
                If String.IsNullOrEmpty(RemoteCreativeThumbFilePath) = False Then
                    UploadFile(GetS3Client(), RemoteCreativeThumbFilePath, toPathThumb)
                End If
            Else
                CreateNewFolder(GetS3Client(), FilePath)
                UploadFile(GetS3Client(), RemoteCreativeThumbFilePath, toPathThumb)
            End If
        End If
        Return True
    End Function

    Public Shared Function GetContentType(fileExtension As String) As String
        Dim mimeTypes = New Dictionary(Of [String], [String])() From {
            {".bmp", "image/bmp"},
            {".gif", "image/gif"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".png", "image/png"},
            {".tif", "image/tiff"},
            {".tiff", "image/tiff"},
            {".mp3", "audio/mpeg"},
            {".wma", "audio/wma"},
            {".wav", "audio/wav"},
            {".wmv", "audio/wmv"},
            {".swf", "application/x-shockwave-flash"},
            {".avi", "video/avi"},
            {".mp4", "video/mp4"},
            {".mpeg", "video/mpeg"},
            {".mpg", "video/mpeg"},
            {".qt", "video/quicktime"}
        }

        ' if the file type is not recognized, return "application/octet-stream" so the browser will simply download it
        Return If(mimeTypes.ContainsKey(fileExtension.ToLower()), mimeTypes(fileExtension.ToLower()), "application/octet-stream")
    End Function

    Public Shared Function GetViewType(fileExtension As String) As String
        Dim mimeTypes = New Dictionary(Of [String], [String])() From {
            {".bmp", "Static"},
            {".gif", "Static"},
            {".jpeg", "Static"},
            {".jpg", "Static"},
            {".png", "Static"},
            {".mp3", "Streaming"},
            {".wma", "Streaming"},
            {".wav", "Streaming"},
            {".wmv", "Streaming"},
            {".swf", "Streaming"},
            {".avi", "Streaming"},
            {".mp4", "Streaming"},
            {".mpeg", "Streaming"},
            {".mpg", "Streaming"},
            {".qt", "Streaming"}
        }

        Return If(mimeTypes.ContainsKey(fileExtension), mimeTypes(fileExtension), "application/octet-stream")
    End Function

    Public Function GetTVFolder(fileExtension) As String
        Dim folderValue As String
        Select Case fileExtension
            Case ".mp3"
                folderValue = "MP3"
            Case ".jpg"
                folderValue = "JPG"
            Case ".wav"
                folderValue = "WAV"
            Case ".mpg"
                folderValue = "HQ"
            Case ".mp4"
                folderValue = "HQ"
        End Select
        Return folderValue
    End Function

End Class
