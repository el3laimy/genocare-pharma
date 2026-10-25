using CommunityToolkit.Mvvm.ComponentModel;

namespace GenoCare.Desktop.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty] public partial string Title { get; set; } = "جينوكير";
}
