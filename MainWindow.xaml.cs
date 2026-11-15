// Bitcoin Widget
// Created by pgmat
// GitHub: https://github.com/pgmatheus-code

using System.Windows;
using System.Net.Http;
using System.Text.Json;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using Hardcodet.Wpf.TaskbarNotification;

namespace Bitcoin_Widget;

public partial class MainWindow : Window
{
    // especificidades topmost
    [DllImport("user32.dll")]
    static extern bool SetWindowPos(
    IntPtr hWnd,
    IntPtr hWndInsertAfter,
    int X,
    int Y,
    int cx,
    int cy,
    uint uFlags);
    static readonly IntPtr HWND_TOPMOST = new(-1);
    const uint SWP_NOMOVE = 0x0002;
    const uint SWP_NOSIZE = 0x0001;
    const uint SWP_SHOWWINDOW = 0x0040;

    // variavies comuns
    private readonly HttpClient _http = new();
    private bool _travado = true;
    private TaskbarIcon _trayIcon;

    public MainWindow()
    {
        // startup
        InitializeComponent();

        //posicionar a aplicação no canto inferior esquerdo       
        Loaded += (_, _) =>
        {
            Left = 0;
            Top = SystemParameters.PrimaryScreenHeight - Height + 5;

            var handle =
                new System.Windows.Interop.WindowInteropHelper(this).Handle;

            SetWindowPos(
                handle,
                HWND_TOPMOST,
                0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        };
        DispatcherTimer zTimer = new();

        zTimer.Interval = TimeSpan.FromSeconds(0.25);

        zTimer.Tick += (_, _) =>
        {
            var handle =
                new System.Windows.Interop.WindowInteropHelper(this).Handle;

            SetWindowPos(
                handle,
                HWND_TOPMOST,
                0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        };

        zTimer.Start();

        // esconder da taskbar
        ShowInTaskbar = false;

        StatusText.Text = "🔒";
        AtualizarCotacoes();

        // timer do loop
        DispatcherTimer timer = new();
        timer.Interval = TimeSpan.FromSeconds(30);
        timer.Tick += async (_, _) => await AtualizarCotacoes();
        timer.Start();

        // icone da bandeja
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "Bitcoin Widget",
            Icon = new System.Drawing.Icon(
            @"ico\icon.ico")
        };
    }

    private async Task AtualizarCotacoes()
    {
        try
        {
            var btcJson = await _http.GetStringAsync(
                "https://api.binance.com/api/v3/ticker/price?symbol=BTCUSDT");

            using var btcDoc = JsonDocument.Parse(btcJson);

            decimal btcUsd = decimal.Parse(
                btcDoc.RootElement.GetProperty("price").GetString()!,
                CultureInfo.InvariantCulture);

            var usdJson = await _http.GetStringAsync(
                "https://api.binance.com/api/v3/ticker/price?symbol=USDTBRL");

            using var usdDoc = JsonDocument.Parse(usdJson);

            decimal usdBrl = decimal.Parse(
                usdDoc.RootElement.GetProperty("price").GetString()!,
                CultureInfo.InvariantCulture);

            BtcText.Text = $"BTC/USD {btcUsd:N0}";
            UsdText.Text = $"USD/BRL {usdBrl:N2}";
        }
        catch (Exception ex)
        {
            BtcText.Text = ex.GetType().Name;
            UsdText.Text = ex.Message;
        }
    }

    protected override void OnMouseLeftButtonDown(
        MouseButtonEventArgs e)
    {
        if (!_travado)
        {
            DragMove();
        }

        base.OnMouseLeftButtonDown(e);
    }

    protected override void OnMouseDoubleClick(
    System.Windows.Input.MouseButtonEventArgs e)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = "https://www.tradingview.com/symbols/BTCUSD/",
                UseShellExecute = true
            });
    }

    protected override void OnMouseRightButtonDown(
    MouseButtonEventArgs e)
    {
        _travado = !_travado;

        StatusText.Text =
            _travado ? "🔒" : "🔓";

        base.OnMouseRightButtonDown(e);
    }

    
}