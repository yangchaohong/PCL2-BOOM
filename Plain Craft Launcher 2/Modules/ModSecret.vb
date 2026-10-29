'由于包含加解密等安全信息，本文件中的部分代码已被删除
Imports System.Reflection

Friend Module ModSecret

    Private AuthTokenLoaded As Boolean

    '标注 PCL 的不同分支，仅用于替换标记
    Public Const VersionBranchMain As String = "AbuzzHarbor9999_Modified"
    '在开源版的注册表与常规版的注册表隔离，以防数据冲突
    Public Const RegFolder As String = "PCLDebug"
    '用于微软登录的 ClientId
    Public OAuthClientId As String
    'CurseForge API Key
    Public CurseForgeAPIKey As String

    '用于匿名数据收集的腾讯云日志服务上报 URL，形如 https://{region}.cls.tencentcs.com/track?topic_id={topic_id}
    Public Const ClsBaseUrl As String = ""

    Public Sub ReadToken(Optional Force As Boolean = False)
        If AuthTokenLoaded AndAlso Not Force Then Return

        Dim assembly As Assembly = Assembly.GetExecutingAssembly()
        ReadTokenResource(assembly, "PCL.auth_token.ini", OAuthClientId, CurseForgeAPIKey)
        '本地配置优先于仓库中的占位模板。
        ReadTokenResource(assembly, "PCL.auth_token.local.ini", OAuthClientId, CurseForgeAPIKey)

        Dim EnvironmentClientId As String = Environment.GetEnvironmentVariable("PCL_MS_CLIENT_ID")
        Dim EnvironmentCurseForgeKey As String = Environment.GetEnvironmentVariable("PCL_CURSEFORGE_API_KEY")
        If Not String.IsNullOrWhiteSpace(EnvironmentClientId) Then OAuthClientId = EnvironmentClientId
        If Not String.IsNullOrWhiteSpace(EnvironmentCurseForgeKey) Then CurseForgeAPIKey = EnvironmentCurseForgeKey
        AuthTokenLoaded = True
    End Sub

    Private Sub ReadTokenResource(Assembly As Assembly, ResourceName As String, ByRef ClientId As String, ByRef CurseForgeKey As String)
        Using FileStream As Stream = Assembly.GetManifestResourceStream(ResourceName)
            If FileStream Is Nothing Then Return
            Using Reader As New StreamReader(FileStream)
                While Not Reader.EndOfStream
                    Dim Token As String = Reader.ReadLine().Trim()
                    If Token.Length = 0 OrElse Token.StartsWith("#") Then Continue While
                    Dim EqualPos As Integer = Token.IndexOf("="c)
                    If EqualPos <= 0 Then Continue While
                    Dim TokenName As String = Token.Substring(0, EqualPos).Trim()
                    Dim TokenValue As String = Token.Substring(EqualPos + 1).Trim()
                    Select Case TokenName
                        Case "PCL_MS_CLIENT_ID"
                            ClientId = TokenValue
                        Case "CurseForgeAPIKey"
                            CurseForgeKey = TokenValue
                    End Select
                End While
            End Using
        End Using
    End Sub
#Region "网络鉴权"

    Public Const RestrictMcDownload As Boolean = False
    Friend Function SecretCdnSign(UrlWithMark As String) As String
        If Not UrlWithMark.EndsWithF("{CDN}") Then Return UrlWithMark
        Return UrlWithMark.Replace("{CDN}", "").Replace(" ", "%20")
    End Function
    ''' <summary>
    ''' 设置 Headers 的 UA、Referer。
    ''' </summary>
    Friend Sub SecretHeadersSign(Url As String, ByRef Req As HttpRequestMessage, Optional SimulateBrowserHeaders As Boolean = False)
        ReadToken()
        If Not Req.Headers.UserAgent.Any Then
            If Url.Contains("baidupcs.com") OrElse Url.Contains("baidu.com") Then
                Req.Headers.Add("User-Agent", "LogStatistic")  '#4951
            ElseIf SimulateBrowserHeaders Then
                Req.Headers.Add("User-Agent", $"PCL2/{VersionBaseName}.{CInt(BuildType)} Mozilla/5.0 AppleWebKit/537.36 Chrome/63.0.3239.132 Safari/537.36")
            Else
                Req.Headers.Add("User-Agent", $"PCL2/{VersionBaseName}.{CInt(BuildType)}")
            End If
        End If
        If Not SimulateBrowserHeaders Then Req.Headers.Add("Referer", $"http://{VersionCode}.open.pcl2.server/")
        If Url.Contains("api.curseforge.com") OrElse Url.Contains("forgecdn.net") Then Req.Headers.Add("x-api-key", CurseForgeAPIKey)
    End Sub

#End Region

#Region "主题"

    Public Color1 As New MyColor(52, 61, 74)
    Public Color2 As New MyColor(11, 91, 203)
    Public Color3 As New MyColor(19, 112, 243)
    Public Color4 As New MyColor(72, 144, 245)
    Public Color5 As New MyColor(150, 192, 249)
    Public Color6 As New MyColor(213, 230, 253)
    Public Color7 As New MyColor(222, 236, 253)
    Public Color8 As New MyColor(234, 242, 254)
    Public ColorBg0 As New MyColor(150, 192, 249)
    Public ColorBg1 As New MyColor(190, Color7)
    Public ColorGray1 As New MyColor(64, 64, 64)
    Public ColorGray2 As New MyColor(115, 115, 115)
    Public ColorGray3 As New MyColor(140, 140, 140)
    Public ColorGray4 As New MyColor(166, 166, 166)
    Public ColorGray5 As New MyColor(204, 204, 204)
    Public ColorGray6 As New MyColor(235, 235, 235)
    Public ColorGray7 As New MyColor(240, 240, 240)
    Public ColorGray8 As New MyColor(245, 245, 245)
    Public ColorSemiTransparent As New MyColor(1, Color8)

    Public ThemeNow As Integer = -1
    Public ColorHue As Integer = 210, ColorSat As Integer = 85, ColorLightAdjust As Integer = 0, ColorHueTopbarDelta As OneOf(Of Integer, Integer()) = 0
    Public ThemeDontClick As Integer = 0

    Public Sub ThemeRefresh(Optional NewTheme As Integer = -1)
        Try
            If NewTheme >= 0 Then
                ThemeNow = NewTheme
                Select Case NewTheme
                    Case 0 '龙猫蓝
                        ColorHue = 210 : ColorSat = 85 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 0
                    Case 1 '甜柠青
                        ColorHue = 174 : ColorSat = 75 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 10
                    Case 2 '小草绿
                        ColorHue = 120 : ColorSat = 70 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 10
                    Case 3 '菠萝黄
                        ColorHue = 48 : ColorSat = 78 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 12
                    Case 4 '橡木棕
                        ColorHue = 28 : ColorSat = 55 : ColorLightAdjust = -2 : ColorHueTopbarDelta = 8
                    Case 5 '玄素黑
                        ColorHue = 220 : ColorSat = 5 : ColorLightAdjust = -10 : ColorHueTopbarDelta = 0
                    Case 6 '铁杆粉
                        ColorHue = 335 : ColorSat = 65 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 10
                    Case 7 '神秘紫
                        ColorHue = 270 : ColorSat = 55 : ColorLightAdjust = -2 : ColorHueTopbarDelta = 12
                    Case 8 '秋仪金
                        ColorHue = 40 : ColorSat = 58 : ColorLightAdjust = -3 : ColorHueTopbarDelta = 10
                    Case 9 '活跃橙
                        ColorHue = 22 : ColorSat = 82 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 12
                    Case 10 '跳票红
                        ColorHue = 0 : ColorSat = 72 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 8
                    Case 11 '极客蓝
                        ColorHue = 195 : ColorSat = 80 : ColorLightAdjust = -4 : ColorHueTopbarDelta = 12
                    Case 12 '滑稽彩
                        ColorHue = 48 : ColorSat = 88 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 30
                    Case 13 '欧皇彩
                        ColorHue = 290 : ColorSat = 82 : ColorLightAdjust = 0 : ColorHueTopbarDelta = 35
                    Case Else '自定义
                        ColorHue = Settings.Get(Of Integer)("UiLauncherHue")
                        ColorSat = Settings.Get(Of Integer)("UiLauncherSat")
                        ColorLightAdjust = Settings.Get(Of Integer)("UiLauncherLight")
                        ColorHueTopbarDelta = Settings.Get(Of Integer)("UiLauncherDelta")
                End Select
            ElseIf ThemeNow = 14 Then
                '自定义主题拖动滑块时，需要按最新设置重新计算颜色。
                ColorHue = Settings.Get(Of Integer)("UiLauncherHue")
                ColorSat = Settings.Get(Of Integer)("UiLauncherSat")
                ColorLightAdjust = Settings.Get(Of Integer)("UiLauncherLight")
                ColorHueTopbarDelta = Settings.Get(Of Integer)("UiLauncherDelta")
            End If

            Color1 = New MyColor().FromHSL2(ColorHue, ColorSat * 0.2, 25 + ColorLightAdjust * 0.3)
            Color2 = New MyColor().FromHSL2(ColorHue, ColorSat, 45 + ColorLightAdjust)
            Color3 = New MyColor().FromHSL2(ColorHue, ColorSat, 55 + ColorLightAdjust)
            Color4 = New MyColor().FromHSL2(ColorHue, ColorSat, 65 + ColorLightAdjust)
            Color5 = New MyColor().FromHSL2(ColorHue, ColorSat, 80 + ColorLightAdjust * 0.4)
            Color6 = New MyColor().FromHSL2(ColorHue, ColorSat, 91 + ColorLightAdjust * 0.1)
            Color7 = New MyColor().FromHSL2(ColorHue, ColorSat, 95)
            Color8 = New MyColor().FromHSL2(ColorHue, ColorSat, 97)
            ColorBg0 = Color4 * 0.4 + Color5 * 0.4 + ColorGray4 * 0.2
            ColorBg1 = New MyColor(190, Color7)

            ColorSemiTransparent = New MyColor(1, Color8)
            Application.Current.Resources("ColorBrush1") = New SolidColorBrush(Color1)
            Application.Current.Resources("ColorBrush2") = New SolidColorBrush(Color2)
            Application.Current.Resources("ColorBrush3") = New SolidColorBrush(Color3)
            Application.Current.Resources("ColorBrush4") = New SolidColorBrush(Color4)
            Application.Current.Resources("ColorBrush5") = New SolidColorBrush(Color5)
            Application.Current.Resources("ColorBrush6") = New SolidColorBrush(Color6)
            Application.Current.Resources("ColorBrush7") = New SolidColorBrush(Color7)
            Application.Current.Resources("ColorBrush8") = New SolidColorBrush(Color8)
            Application.Current.Resources("ColorBrushBg0") = New SolidColorBrush(ColorBg0)
            Application.Current.Resources("ColorBrushBg1") = New SolidColorBrush(ColorBg1)
            Application.Current.Resources("ColorObject1") = CType(Color1, Color)
            Application.Current.Resources("ColorObject2") = CType(Color2, Color)
            Application.Current.Resources("ColorObject3") = CType(Color3, Color)
            Application.Current.Resources("ColorObject4") = CType(Color4, Color)
            Application.Current.Resources("ColorObject5") = CType(Color5, Color)
            Application.Current.Resources("ColorObject6") = CType(Color6, Color)
            Application.Current.Resources("ColorObject7") = CType(Color7, Color)
            Application.Current.Resources("ColorObject8") = CType(Color8, Color)
            Application.Current.Resources("ColorObjectBg0") = CType(ColorBg0, Color)
            Application.Current.Resources("ColorObjectBg1") = CType(ColorBg1, Color)
            ThemeRefreshMain()
        Catch ex As Exception
            Logger.Error(ex, "刷新主题颜色失败", LogBehavior.Toast)
        End Try
    End Sub
    Public Sub ThemeRefreshMain()
        RunInUi(
        Sub()
            If Not FrmMain.IsLoaded Then Return
            '顶部条背景
            Dim Brush = New LinearGradientBrush With {.EndPoint = New Point(1, 0), .StartPoint = New Point(0, 0)}
            Dim Deltas = ColorHueTopbarDelta.Switch(Function(d) New Integer() {-d, 0, d}, Function(d) d)
            Brush.GradientStops.Add(New GradientStop With {.Offset = 0, .Color = New MyColor().FromHSL2(ColorHue + Deltas(0), ColorSat, 48 + ColorLightAdjust)})
            Brush.GradientStops.Add(New GradientStop With {.Offset = 0.5, .Color = New MyColor().FromHSL2(ColorHue + Deltas(1), ColorSat, 54 + ColorLightAdjust)})
            Brush.GradientStops.Add(New GradientStop With {.Offset = 1, .Color = New MyColor().FromHSL2(ColorHue + Deltas(2), ColorSat, 48 + ColorLightAdjust)})
            FrmMain.PanTitle.Background = Brush
            FrmMain.PanTitle.Background.Freeze()
            '主页面背景
            If Settings.Get(Of Boolean)("UiBackgroundColorful") Then
                Brush = New LinearGradientBrush With {.EndPoint = New Point(0.1, 1), .StartPoint = New Point(0.9, 0)}
                Brush.GradientStops.Add(New GradientStop With {.Offset = -0.1, .Color = New MyColor().FromHSL2(ColorHue - 15, ColorSat * 0.8, 91)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0.4, .Color = New MyColor().FromHSL2(ColorHue, ColorSat * 0.8, 91)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 1.1, .Color = New MyColor().FromHSL2(ColorHue + 15, ColorSat * 0.8, 91)})
                FrmMain.PanForm.Background = Brush
            Else
                FrmMain.PanForm.Background = New MyColor(245, 245, 245)
            End If
            FrmMain.PanForm.Background.Freeze()
        End Sub)
    End Sub
    Private Function GetUnlockedHiddenThemes() As HashSet(Of Integer)
        Dim Result As New HashSet(Of Integer)
        Dim SavedValue As String = Settings.Get(Of String)("UiLauncherThemeHide2")
        If String.IsNullOrWhiteSpace(SavedValue) Then Return Result
        For Each Part In SavedValue.Split(New Char() {"|"c}, StringSplitOptions.RemoveEmptyEntries)
            Dim ThemeId As Integer
            If Integer.TryParse(Part.Trim(), ThemeId) Then Result.Add(ThemeId)
        Next
        Return Result
    End Function
    Private Sub SaveUnlockedHiddenThemes(UnlockedThemes As HashSet(Of Integer))
        Settings.Set("UiLauncherThemeHide2", UnlockedThemes.OrderBy(Function(Id) Id).Join("|"c))
    End Sub
    Friend Sub ThemeCheckAll(EffectSetup As Boolean)
        If FrmSetupUI Is Nothing Then Return
        Dim HiddenThemeIds As Integer() = {5, 6, 7, 8, 9, 10, 11, 12, 13}
        For Each ThemeId In HiddenThemeIds
            Dim Radio = TryCast(FrmSetupUI.FindName($"RadioLauncherTheme{ThemeId}"), MyRadioBox)
            If Radio IsNot Nothing Then Radio.IsEnabled = ThemeCheckOne(ThemeId)
        Next
        '移除隐藏主题上的遮罩与占位控件
        FrmSetupUI.RadioLauncherTheme5.Opacity = If(ThemeCheckOne(5), 1, 0)
        FrmSetupUI.RadioLauncherTheme5Gray.Visibility = If(ThemeCheckOne(5), Visibility.Collapsed, Visibility.Visible)
        FrmSetupUI.LabLauncherTheme5Unlock.Visibility = If(ThemeCheckOne(5), Visibility.Collapsed, Visibility.Visible)
        FrmSetupUI.LabLauncherTheme8Copy.Visibility = If(ThemeCheckOne(8), Visibility.Collapsed, Visibility.Visible)
        FrmSetupUI.LabLauncherTheme9Copy.Visibility = If(ThemeCheckOne(9), Visibility.Collapsed, Visibility.Visible)
        FrmSetupUI.LabLauncherTheme11Click.Visibility = If(ThemeCheckOne(11), Visibility.Collapsed, Visibility.Visible)
        Dim AllHiddenThemesUnlocked As Boolean = HiddenThemeIds.All(Function(Id) ThemeCheckOne(Id))
        '解锁至少五个隐藏主题后开放自定义主题
        FrmSetupUI.RadioLauncherTheme14.IsEnabled = HiddenThemeIds.Count(Function(Id) ThemeCheckOne(Id)) >= 5
        FrmSetupUI.BtnThemeUnlockAll.IsEnabled = Not AllHiddenThemesUnlocked
        FrmSetupUI.BtnThemeUnlockAll.Text = If(AllHiddenThemesUnlocked, "隐藏主题已全部解锁", "一键全部解锁（调试）")
    End Sub
    Friend Function ThemeCheckOne(Id As Integer) As Boolean
        If Id < 5 Then Return True
        If Id > 13 Then Return False
        Return GetUnlockedHiddenThemes().Contains(Id)
    End Function
    Friend Sub ThemeUnlockAll(Optional ShowHint As Boolean = True)
        Dim UnlockedThemes = GetUnlockedHiddenThemes()
        For ThemeId = 5 To 13
            UnlockedThemes.Add(ThemeId)
        Next
        SaveUnlockedHiddenThemes(UnlockedThemes)
        ThemeCheckAll(True)
        If ShowHint Then Hint("已解锁全部隐藏主题！", HintType.Green)
    End Sub
    Friend Function ThemeUnlock(Id As Integer, Optional ShowDoubleHint As Boolean = True, Optional UnlockHint As String = Nothing) As Boolean
        If Id < 5 OrElse Id > 13 Then Return False
        Dim UnlockedThemes = GetUnlockedHiddenThemes()
        Dim IsNewUnlock As Boolean = UnlockedThemes.Add(Id)
        If IsNewUnlock Then SaveUnlockedHiddenThemes(UnlockedThemes)
        ThemeCheckAll(True)
        If ShowDoubleHint AndAlso IsNewUnlock AndAlso Not String.IsNullOrWhiteSpace(UnlockHint) Then Hint(UnlockHint, HintType.Green)
        Return IsNewUnlock
    End Function

#End Region

#Region "更新"

    Friend Sub UpdateCheckByButton()
        Hint("该版本中不包含更新功能……")
    End Sub

    Friend IsUpdateWaitingRestart As Boolean = False
    Public Sub UpdateRestart(TriggerRestartAndByEnd As Boolean)
    End Sub
    Public Sub UpdateReplace(ProcessId As Integer, OldFileName As String, NewFileName As String, TriggerRestart As Boolean)
    End Sub

    ''' <summary>
    ''' 确保 PathTemp/Latest.exe 是最新正式版的 PCL，它会被用于整合包打包。
    ''' 如果不是，则下载一个。
    ''' </summary>
    Friend Sub DownloadLatestPCL(Optional LoaderToSyncProgress As LoaderBase = Nothing)
        '注意：如果要自行实现这个功能，请换用另一个文件路径，以免与官方版本冲突
    End Sub

#End Region

#Region "联网配置"

    ''' <summary>
    ''' 联网获取的配置信息。
    ''' 若获取失败或仍在获取中，可能为 Nothing。
    ''' </summary>
    Public ServerConfig As JObject

    Public ServerLoader As New LoaderTask(Of Integer, Integer)("PCL 配置更新", Sub() Logger.Info("该版本中不包含更新通知功能……"), Priority:=ThreadPriority.BelowNormal) With
        {.ReloadTimeout = 1000 * 60 * 60} '超时 1 小时

#End Region

#Region "赞助等级"

    Public ReadOnly Property CurrentRank As DonationRank
        Get
            Return DonationRank.None
        End Get
    End Property

    Public Sub InputPotatoCode(IsUpdating As Boolean)
    End Sub
    Friend Sub GeneratePotatoCode()
    End Sub

    ''' <summary>
    ''' 获取设备识别码。
    ''' </summary>
    Friend Function GetIdentify() As String
        Return "0000-0000-0000-0000"
    End Function

#End Region

End Module
