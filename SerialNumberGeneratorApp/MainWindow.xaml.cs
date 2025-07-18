﻿using System.Text;
using System.Windows;
using Service;

namespace SerialNumberGeneratorApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ISerialNumberGenerator _serialNumberGenerator;
    public MainWindow(ISerialNumberGenerator serialNumberGenerator)
    {
        InitializeComponent();
        this.ResizeMode = ResizeMode.NoResize;
        this.Height = 500;
        this.Width = 400;
        _serialNumberGenerator = serialNumberGenerator;
        this.GeneratorInfoBox.Text = string.Empty;
    }

    private async void GenerateButton_OnClick(object sender, RoutedEventArgs e)
    {
        var sb = new StringBuilder();
        try
        {
            sb.AppendLine("Starting generation of serial numbers...");
            this.GeneratorInfoBox.Text += sb.ToString();
            var response = await _serialNumberGenerator.GenerateSerialNumbersFileAsync(string.Empty);
            if (response.WorkOrdersCount == 0)
            {
                sb.AppendLine($"No new work orders found.");
                sb.AppendLine($"Last generated serial number: {response.LastGeneratedSerialNumber}");
                this.GeneratorInfoBox.Text = sb.ToString();
                return;
            }
            sb.AppendLine($"Serial numbers generated successfully. Saved file to {response.FilePath}");
            sb.AppendLine($"Total work orders processed: {response.WorkOrdersCount}");
            sb.AppendLine($"Total serial numbers generated: {response.SerialNumbersGeneratedCount}");
            sb.AppendLine($"Last generated serial number: {response.LastGeneratedSerialNumber}");
            this.GeneratorInfoBox.Text += sb.ToString();
        }
        catch(Exception exception)
        {
            sb.AppendLine("Failed to generate serial numbers.");
            sb.AppendLine(exception.Message);
            this.GeneratorInfoBox.Text = sb.ToString();
            Console.WriteLine(exception);
        }
    }
}