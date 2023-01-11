using Content.Client.Message;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Monitor;
using Content.Shared.Temperature;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.Atmos.Monitor.UI.Widgets;

[GenerateTypedNameReferences]
public sealed partial class SensorInfo : BoxContainer
{
    public Action<string, AtmosMonitorThresholdType, AtmosAlarmThreshold, Gas?>? OnThresholdUpdate;
    private string _address;

    private ThresholdControl _pressureThreshold;
    private ThresholdControl _temperatureThreshold;
    private Dictionary<Gas, ThresholdControl> _gasThresholds = new();
    private Dictionary<Gas, RichTextLabel> _gasLabels = new();

    public SensorInfo(AtmosSensorData data, string address)
    {
        RobustXamlLoader.Load(this);

        _address = address;

        SensorAddress.Title = $"{address} : {data.AlarmState}";

        AlarmStateLabel.SetMarkup(Loc.GetString("air-alarm-ui-window-alarm-state-indicator",
                    ("color", AirAlarmWindow.ColorForAlarm(data.AlarmState)),
                    ("state", $"{data.AlarmState}")));
        PressureLabel.SetMarkup(Loc.GetString("air-alarm-ui-window-pressure-indicator",
                    ("color", AirAlarmWindow.ColorForThreshold(data.Pressure, data.PressureThreshold)),
                    ("pressure", $"{data.Pressure:0.##}")));
        TemperatureLabel.SetMarkup(Loc.GetString("air-alarm-ui-window-temperature-indicator",
                ("color", AirAlarmWindow.ColorForThreshold(data.Temperature, data.TemperatureThreshold)),
                ("tempC", $"{TemperatureHelpers.KelvinToCelsius(data.Temperature):0.#}"),
                ("temperature", $"{data.Temperature:0.##}")));

        foreach (var (gas, amount) in data.Gases)
        {
            var label = new RichTextLabel();

            var fractionGas = amount / data.TotalMoles;
            label.SetMarkup(Loc.GetString("air-alarm-ui-gases-indicator", ("gas", $"{gas}"),
                ("color", AirAlarmWindow.ColorForThreshold(fractionGas, data.GasThresholds[gas])),
                ("amount", $"{amount:0.####}"),
                ("percentage", $"{(100 * fractionGas):0.##}")));
            GasContainer.AddChild(label);
            _gasLabels.Add(gas, label);

            var threshold = data.GasThresholds[gas];
            var gasThresholdControl = new ThresholdControl(Loc.GetString($"air-alarm-ui-thresholds-gas-title", ("gas", $"{gas}")), threshold, AtmosMonitorThresholdType.Gas, gas, 100);
            gasThresholdControl.Margin = new Thickness(20, 2, 2, 2);
            gasThresholdControl.ThresholdDataChanged += (type, threshold, arg3) =>
            {
                OnThresholdUpdate!(_address, type, threshold, arg3);
            };

            _gasThresholds.Add(gas, gasThresholdControl);
            GasContainer.AddChild(gasThresholdControl);
        }

        _pressureThreshold = new ThresholdControl(Loc.GetString("air-alarm-ui-thresholds-pressure-title"), data.PressureThreshold, AtmosMonitorThresholdType.Pressure);
        PressureThresholdContainer.AddChild(_pressureThreshold);
        _temperatureThreshold = new ThresholdControl(Loc.GetString("air-alarm-ui-thresholds-temperature-title"), data.TemperatureThreshold,
            AtmosMonitorThresholdType.Temperature);
        TemperatureThresholdContainer.AddChild(_temperatureThreshold);

        _pressureThreshold.ThresholdDataChanged += (type, threshold, arg3) =>
        {
            OnThresholdUpdate!(_address, type, threshold, arg3);
        };

        _temperatureThreshold.ThresholdDataChanged += (type, threshold, arg3) =>
        {
            OnThresholdUpdate!(_address, type, threshold, arg3);
        };

        foreach (var (gas, threshold) in data.GasThresholds)
        {
       }
    }

    public void ChangeData(AtmosSensorData data)
    {
        SensorAddress.Title = $"{_address} : {data.AlarmState}";

        AlarmStateLabel.SetMarkup(Loc.GetString("air-alarm-ui-window-alarm-state-indicator",
                    ("color", AirAlarmWindow.ColorForAlarm(data.AlarmState)),
                    ("state", $"{data.AlarmState}")));

        PressureLabel.SetMarkup(Loc.GetString("air-alarm-ui-window-pressure-indicator",
                    ("color", AirAlarmWindow.ColorForThreshold(data.Pressure, data.PressureThreshold)),
                    ("pressure", $"{data.Pressure:0.##}")));
        TemperatureLabel.SetMarkup(Loc.GetString("air-alarm-ui-window-temperature-indicator",
                ("color", AirAlarmWindow.ColorForThreshold(data.Temperature, data.TemperatureThreshold)),
                ("tempC", $"{TemperatureHelpers.KelvinToCelsius(data.Temperature):0.#}"),
                ("temperature", $"{data.Temperature:0.##}")));

        foreach (var (gas, amount) in data.Gases)
        {
            if (!_gasLabels.TryGetValue(gas, out var label))
            {
                continue;
            }

            var fractionGas = amount / data.TotalMoles;
            label.SetMarkup(Loc.GetString("air-alarm-ui-gases-indicator", ("gas", $"{gas}"),
                ("color", AirAlarmWindow.ColorForThreshold(fractionGas, data.GasThresholds[gas])),
                ("amount", $"{amount:0.####}"),
                ("percentage", $"{(100 * fractionGas):0.##}")));
        }

        _pressureThreshold.UpdateThresholdData(data.PressureThreshold, data.Pressure);
        _temperatureThreshold.UpdateThresholdData(data.TemperatureThreshold, data.Temperature);
        foreach (var (gas, control) in _gasThresholds)
        {
            if (!data.GasThresholds.TryGetValue(gas, out var threshold))
            {
                continue;
            }

            control.UpdateThresholdData(threshold, data.Gases[gas] / data.TotalMoles);
        }
    }

 }