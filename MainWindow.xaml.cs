// Bitcoin Widget
// Created by pgmat
// GitHub: https://github.com/pgmatheus-code

using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;

namespace Bitcoin_Widget;

public partial class MainWindow : Window
{
    #region Win32

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint flags);

    private static readonly IntPtr HWND_TOPMOST = new(-1);

    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_SHOWWINDOW = 0x0040;

    #endregion

    #region Fields

    private readonly HttpClient _http = new();

    private bool _travado = true;

    private TaskbarIcon _trayIcon;

    #endregion

    public MainWindow()
    {
        InitializeComponent();

        ConfigureWindow();
        ConfigurePosition();
        ConfigureTrayIcon();
        ConfigurePriceUpdater();
        ConfigureTopMostKeeper();

        StatusText.Text = "🔒";

        _ = AtualizarCotacoes();
    }

    #region Initialization
    private void ConfigureWindow()
    {
        ShowInTaskbar = false;
    }

    private void ConfigurePosition()
    {
        Loaded += (_, _) =>
        {
            Left = 0;
            Top = SystemParameters.PrimaryScreenHeight - Height + 5;

            ForceTopMost();
        };
    }

    private void ConfigurePriceUpdater()
    {
        DispatcherTimer timer = new();

        timer.Interval = TimeSpan.FromSeconds(30);

        timer.Tick += async (_, _) =>
        {
            await AtualizarCotacoes();
        };

        timer.Start();
    }

    private void ConfigureTopMostKeeper()
    {
        DispatcherTimer zTimer = new();

        zTimer.Interval = TimeSpan.FromSeconds(5);

        zTimer.Tick += (_, _) =>
        {
            ForceTopMost();
        };

        zTimer.Start();
    }

    private void ConfigureTrayIcon()
    {
        var trayMenu = new ContextMenu();

        var exitItem = new MenuItem
        {
            Header = "Exit"
        };

        exitItem.Click += (_, _) =>
        {
            Application.Current.Shutdown();
        };

        trayMenu.Items.Add(exitItem);

        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "Bitcoin Widget",
            ContextMenu = trayMenu,
            Icon = new System.Drawing.Icon(@"ico\icon.ico")
        };
    }

    #endregion

    #region Market Data
    private async Task AtualizarCotacoes()
    {
        try
        {
            decimal btcUsd = await ObterCotacao(
                "https://api.binance.com/api/v3/ticker/price?symbol=BTCUSDT");

            decimal usdBrl = await ObterCotacao(
                "https://api.binance.com/api/v3/ticker/price?symbol=USDTBRL");

            BtcText.Text = $"BTC/USD {btcUsd:N0}";
            UsdText.Text = $"USD/BRL {usdBrl:N2}";
        }
        catch (Exception ex)
        {
            BtcText.Text = ex.GetType().Name;
            UsdText.Text = ex.Message;
        }
    }

    private async Task<decimal> ObterCotacao(string url)
    {
        var json = await _http.GetStringAsync(url);

        using JsonDocument doc = JsonDocument.Parse(json);

        return decimal.Parse(
            doc.RootElement.GetProperty("price").GetString()!,
            CultureInfo.InvariantCulture);
    }

    #endregion

    #region Window Control
    private void ForceTopMost()
    {
        IntPtr handle =
            new System.Windows.Interop.WindowInteropHelper(this).Handle;

        SetWindowPos(
            handle,
            HWND_TOPMOST,
            0,
            0,
            0,
            0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
    }

    #endregion

    #region Mouse Events
    protected override void OnMouseLeftButtonDown(
        MouseButtonEventArgs e)
    {
        if (!_travado)
        {
            DragMove();
        }

        base.OnMouseLeftButtonDown(e);
    }

    protected override void OnMouseRightButtonDown(
        MouseButtonEventArgs e)
    {
        _travado = !_travado;

        StatusText.Text = _travado
            ? "🔒"
            : "🔓";

        base.OnMouseRightButtonDown(e);
    }

    protected override void OnMouseDoubleClick(
        MouseButtonEventArgs e)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = "https://www.tradingview.com/symbols/BTCUSD/",
                UseShellExecute = true
            });

        base.OnMouseDoubleClick(e);
    }

    #endregion
}