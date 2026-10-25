using CommunityToolkit.Mvvm.ComponentModel;

namespace GenoCare.Desktop.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty] private string _title = "جينوكير";
}
