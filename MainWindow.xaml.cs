// Bitcoin Widget
// Created by pgmat
// GitHub: https://github.com/pgmatheus-code

using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Bitcoin_Widget;

public partial class MainWindow : Window
{
    private readonly HttpClient _http = new();
    private bool _travado = true;

    public MainWindow()
    {
        InitializeComponent();
        StatusText.Text = "🔒";

        //posicionar a aplicação no canto inferior esquerdo
        Loaded += (_, _) =>
        {
            Left = 0;
            Top = SystemParameters.PrimaryScreenHeight - Height + 5;
        };

        AtualizarCotacoes();

        // timer do loop
        DispatcherTimer timer = new();
        timer.Interval = TimeSpan.FromSeconds(30);
        timer.Tick += async (_, _) => await AtualizarCotacoes();
        timer.Start();
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
            UsdText.Text = $"USD/BRL$ {usdBrl:N2}";
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