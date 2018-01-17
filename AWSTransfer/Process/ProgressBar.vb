Public Class ProgressBar
    Private _lastOutputLength As Integer
    Private ReadOnly _maximumWidth As Integer

    Public Sub New(maximumWidth As Integer)
        _maximumWidth = maximumWidth
        Show(" [ ")
    End Sub

    Public Sub Update(percent As Double)
        ' Remove the last state           
        Dim clear As String = String.Empty.PadRight(_lastOutputLength, ControlChars.Back)

        Show(clear)

        ' Generate new state           
        Dim width As Integer = CInt(percent / 100 * _maximumWidth)
        Dim fill As Integer = _maximumWidth - width
        Dim output As String = String.Format("{0}{1} ] {2}%", String.Empty.PadLeft(width, "="c), String.Empty.PadLeft(fill, " "c), percent.ToString("0.0"))
        Show(output)
        _lastOutputLength = output.Length
    End Sub

    Private Sub Show(value As String)
        Console.Write(value)
    End Sub
End Class
