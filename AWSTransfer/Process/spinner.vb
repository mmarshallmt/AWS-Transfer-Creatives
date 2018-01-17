
Imports System.Threading

Public Class Spinner
    Implements IDisposable
    Private Const Sequence As String = "/-\|"
    Private counter As Integer = 0
    Private ReadOnly left As Integer
    Private ReadOnly top As Integer
    Private ReadOnly delay As Integer
    Private active As Boolean
    Private ReadOnly thread As Thread

    Public Sub New(left As Integer, top As Integer, Optional delay As Integer = 100)
        Me.left = left
        Me.top = top
        Me.delay = delay
        thread = New Thread(AddressOf Spin)
    End Sub

    Public Sub Start()
        active = True
        If Not thread.IsAlive Then
            thread.Start()
        End If
    End Sub

    Public Sub [Stop]()
        active = False
        Draw(" "c)
    End Sub

    Private Sub Spin()
        While active
            Turn()
            Thread.Sleep(delay)
        End While
    End Sub

    Private Sub Draw(c As Char)
        Console.SetCursorPosition(left, top)
        'Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine(c)
    End Sub

    Private Sub Turn()
        Draw(Sequence(System.Threading.Interlocked.Increment(counter) Mod Sequence.Length))
    End Sub

    Public Sub Dispose()
        [Stop]()
    End Sub

    Private Sub IDisposable_Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub
End Class
