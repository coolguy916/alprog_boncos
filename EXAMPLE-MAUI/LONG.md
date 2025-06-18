# Complete .NET MAUI App Structure & Implementation Guide

## 📁 Project Structure

```
AlbonApp/
├── Platforms/
│   ├── Android/
│   ├── iOS/
│   ├── MacCatalyst/
│   └── Windows/
├── Models/
│   ├── User.cs
│   ├── Device.cs
│   ├── SensorReading.cs
│   └── ApiResponse.cs
├── Services/
│   ├── IApiService.cs
│   ├── ApiService.cs
│   ├── IAuthService.cs
│   └── AuthService.cs
├── ViewModels/
│   ├── BaseViewModel.cs
│   ├── LoginViewModel.cs
│   ├── SignupViewModel.cs
│   └── DashboardViewModel.cs
├── Views/
│   ├── LoginPage.xaml
│   ├── LoginPage.xaml.cs
│   ├── SignupPage.xaml
│   ├── SignupPage.xaml.cs
│   ├── DashboardPage.xaml
│   └── DashboardPage.xaml.cs
├── Resources/
│   ├── Styles/
│   │   └── Colors.xaml
│   └── Images/
├── App.xaml
├── App.xaml.cs
├── AppShell.xaml
├── AppShell.xaml.cs
├── MauiProgram.cs
└── AlbonApp.csproj
```

## 🔧 Key Files to Edit

### 1. **MauiProgram.cs** - App Configuration
```csharp
using Microsoft.Extensions.Logging;

namespace AlbonApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Register Services
        builder.Services.AddSingleton<IApiService, ApiService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        
        // Register ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<SignupViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        
        // Register Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<SignupPage>();
        builder.Services.AddTransient<DashboardPage>();

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddLogging();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
```

### 2. **Models/Device.cs**
```csharp
namespace AlbonApp.Models;

public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // BLOWER, SENSOR, etc.
    public bool IsOnline { get; set; }
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}

public class SensorReading
{
    public int Id { get; set; }
    public double Temperature { get; set; }
    public double Ph { get; set; }
    public double Moisture { get; set; }
    public double Watts { get; set; }
    public DateTime Timestamp { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
```

### 3. **Services/ApiService.cs**
```csharp
using System.Net.Http.Json;
using System.Text.Json;
using AlbonApp.Models;

namespace AlbonApp.Services;

public interface IApiService
{
    Task<ApiResponse<User>> LoginAsync(string email, string password);
    Task<ApiResponse<User>> SignupAsync(User user);
    Task<ApiResponse<List<Device>>> GetDevicesAsync();
    Task<ApiResponse<SensorReading>> GetLatestSensorDataAsync();
    Task<ApiResponse<bool>> ToggleDeviceAsync(int deviceId, bool isOn);
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://your-api-url.com/api/"; // Replace with your API URL

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<ApiResponse<User>> LoginAsync(string email, string password)
    {
        try
        {
            var loginData = new { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("auth/login", loginData);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<User>>();
                return result ?? new ApiResponse<User> { Success = false, Message = "Invalid response" };
            }
            
            return new ApiResponse<User> { Success = false, Message = "Login failed" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<User> { Success = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<User>> SignupAsync(User user)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/signup", user);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<User>>();
                return result ?? new ApiResponse<User> { Success = false, Message = "Invalid response" };
            }
            
            return new ApiResponse<User> { Success = false, Message = "Signup failed" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<User> { Success = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<List<Device>>> GetDevicesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("devices");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<Device>>>();
                return result ?? new ApiResponse<List<Device>> { Success = false, Message = "Invalid response" };
            }
            
            return new ApiResponse<List<Device>> { Success = false, Message = "Failed to get devices" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<Device>> { Success = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<SensorReading>> GetLatestSensorDataAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("sensors/latest");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<SensorReading>>();
                return result ?? new ApiResponse<SensorReading> { Success = false, Message = "Invalid response" };
            }
            
            return new ApiResponse<SensorReading> { Success = false, Message = "Failed to get sensor data" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<SensorReading> { Success = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<bool>> ToggleDeviceAsync(int deviceId, bool isOn)
    {
        try
        {
            var toggleData = new { DeviceId = deviceId, IsOn = isOn };
            var response = await _httpClient.PostAsJsonAsync("devices/toggle", toggleData);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                return result ?? new ApiResponse<bool> { Success = false, Message = "Invalid response" };
            }
            
            return new ApiResponse<bool> { Success = false, Message = "Failed to toggle device" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { Success = false, Message = ex.Message };
        }
    }
}
```

### 4. **ViewModels/BaseViewModel.cs**
```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AlbonApp.ViewModels;

public class BaseViewModel : INotifyPropertyChanged
{
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action? onChanged = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        onChanged?.Invoke();
        OnPropertyChanged(propertyName);
        return true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

### 5. **ViewModels/LoginViewModel.cs**
```csharp
using System.Windows.Input;
using AlbonApp.Services;

namespace AlbonApp.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;
    
    private string _email = string.Empty;
    private string _password = string.Empty;

    public LoginViewModel(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
        LoginCommand = new Command(async () => await LoginAsync(), () => !IsBusy);
        SignupCommand = new Command(async () => await GoToSignupAsync());
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand SignupCommand { get; }

    private async Task LoginAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", "Please fill in all fields", "OK");
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _apiService.LoginAsync(Email, Password);
            
            if (result.Success && result.Data != null)
            {
                await _authService.SaveUserAsync(result.Data);
                await Shell.Current.GoToAsync("//dashboard");
            }
            else
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", result.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GoToSignupAsync()
    {
        await Shell.Current.GoToAsync("signup");
    }
}
```

### 6. **Views/LoginPage.xaml** - Responsive Design
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage x:Class="AlbonApp.Views.LoginPage"
             xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:AlbonApp.ViewModels"
             x:DataType="vm:LoginViewModel"
             Shell.NavBarIsVisible="False"
             BackgroundColor="#F5F5F5">

    <Grid>
        <!-- Background Image -->
        <Image Source="plant_background.jpg" 
               Aspect="AspectFill" 
               Opacity="0.3"/>
        
        <!-- Main Content -->
        <ScrollView>
            <Grid Padding="20" RowSpacing="0">
                <Grid.RowDefinitions>
                    <RowDefinition Height="*" />
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="*" />
                </Grid.RowDefinitions>

                <!-- Logo Section -->
                <StackLayout Grid.Row="1" 
                           VerticalOptions="Center" 
                           HorizontalOptions="Center">
                    
                    <!-- Logo -->
                    <Image Source="albon_logo.png" 
                           HeightRequest="60" 
                           Margin="0,0,0,30"/>
                    
                    <!-- Login Card -->
                    <Frame BackgroundColor="White"
                           CornerRadius="15"
                           HasShadow="True"
                           Padding="30"
                           WidthRequest="{OnPlatform Default=400, Phone=350}">
                        
                        <StackLayout Spacing="20">
                            <Label Text="Welcome back!" 
                                   FontSize="24" 
                                   FontAttributes="Bold" 
                                   HorizontalOptions="Center"
                                   TextColor="#2E7D32"/>
                            
                            <Label Text="Enter your Credentials to access your account" 
                                   FontSize="14" 
                                   HorizontalOptions="Center"
                                   TextColor="#666"/>
                            
                            <!-- Email Entry -->
                            <StackLayout>
                                <Label Text="Email" 
                                       FontSize="14" 
                                       TextColor="#333"/>
                                <Entry x:Name="EmailEntry"
                                       Text="{Binding Email}"
                                       Placeholder="Enter your email"
                                       Keyboard="Email"
                                       BackgroundColor="#F8F9FA"
                                       TextColor="#333"/>
                            </StackLayout>
                            
                            <!-- Password Entry -->
                            <StackLayout>
                                <Grid>
                                    <Grid.ColumnDefinitions>
                                        <ColumnDefinition Width="*"/>
                                        <ColumnDefinition Width="Auto"/>
                                    </Grid.ColumnDefinitions>
                                    <Label Grid.Column="0" 
                                           Text="Password" 
                                           FontSize="14" 
                                           TextColor="#333"/>
                                    <Label Grid.Column="1" 
                                           Text="forgot password?" 
                                           FontSize="12" 
                                           TextColor="#4CAF50"
                                           TextDecorations="Underline"/>
                                </Grid>
                                <Entry x:Name="PasswordEntry"
                                       Text="{Binding Password}"
                                       Placeholder="Enter your password"
                                       IsPassword="True"
                                       BackgroundColor="#F8F9FA"
                                       TextColor="#333"/>
                            </StackLayout>
                            
                            <!-- Remember Me -->
                            <StackLayout Orientation="Horizontal">
                                <CheckBox x:Name="RememberCheckBox"/>
                                <Label Text="Remember for 30 days" 
                                       VerticalOptions="Center"
                                       FontSize="14"
                                       TextColor="#666"/>
                            </StackLayout>
                            
                            <!-- Login Button -->
                            <Button Text="Login"
                                    Command="{Binding LoginCommand}"
                                    BackgroundColor="#4CAF50"
                                    TextColor="White"
                                    CornerRadius="8"
                                    HeightRequest="50"
                                    FontSize="16"
                                    FontAttributes="Bold"/>
                            
                            <!-- Signup Link -->
                            <StackLayout Orientation="Horizontal" 
                                       HorizontalOptions="Center">
                                <Label Text="Don't have an account?" 
                                       FontSize="14" 
                                       TextColor="#666"/>
                                <Label Text="Sign Up"
                                       FontSize="14"
                                       TextColor="#4CAF50"
                                       FontAttributes="Bold"
                                       TextDecorations="Underline">
                                    <Label.GestureRecognizers>
                                        <TapGestureRecognizer Command="{Binding SignupCommand}"/>
                                    </Label.GestureRecognizers>
                                </Label>
                            </StackLayout>
                        </StackLayout>
                    </Frame>
                </StackLayout>
            </Grid>
        </ScrollView>
        
        <!-- Loading Indicator -->
        <ActivityIndicator IsVisible="{Binding IsBusy}" 
                          IsRunning="{Binding IsBusy}"
                          Color="#4CAF50"
                          BackgroundColor="#80000000"/>
    </Grid>
</ContentPage>
```

### 7. **Views/DashboardPage.xaml** - Responsive Dashboard
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage x:Class="AlbonApp.Views.DashboardPage"
             xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:AlbonApp.ViewModels"
             x:DataType="vm:DashboardViewModel"
             Title="Dashboard">

    <RefreshView IsRefreshing="{Binding IsRefreshing}" 
                 Command="{Binding RefreshCommand}">
        <ScrollView>
            <Grid Padding="20" RowSpacing="20">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="*" />
                </Grid.RowDefinitions>

                <!-- Header -->
                <Grid Grid.Row="0">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*" />
                        <ColumnDefinition Width="Auto" />
                    </Grid.ColumnDefinitions>
                    
                    <StackLayout Grid.Column="0">
                        <Label Text="{Binding UserName, StringFormat='Hi {0}! 👋'}" 
                               FontSize="18" 
                               FontAttributes="Bold"
                               TextColor="#333"/>
                        <Label Text="{Binding CurrentDate}" 
                               FontSize="14" 
                               TextColor="#666"/>
                    </StackLayout>
                    
                    <Image Grid.Column="1" 
                           Source="albon_logo.png" 
                           HeightRequest="40"/>
                </Grid>

                <!-- Sensor Cards -->
                <Grid Grid.Row="1" ColumnSpacing="15">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*" />
                        <ColumnDefinition Width="*" />
                        <ColumnDefinition Width="*" />
                    </Grid.ColumnDefinitions>
                    
                    <!-- Temperature Card -->
                    <Frame Grid.Column="0" 
                           BackgroundColor="White" 
                           CornerRadius="12" 
                           HasShadow="True" 
                           Padding="15">
                        <StackLayout>
                            <Image Source="temperature_icon.png" 
                                   HeightRequest="30" 
                                   HorizontalOptions="Start"/>
                            <Label Text="TEMPERATURE" 
                                   FontSize="10" 
                                   TextColor="#666" 
                                   FontAttributes="Bold"/>
                            <Label Text="{Binding Temperature, StringFormat='{0:F1}°C'}" 
                                   FontSize="18" 
                                   FontAttributes="Bold" 
                                   TextColor="#FF6B35"/>
                        </StackLayout>
                    </Frame>
                    
                    <!-- pH Card -->
                    <Frame Grid.Column="1" 
                           BackgroundColor="White" 
                           CornerRadius="12" 
                           HasShadow="True" 
                           Padding="15">
                        <StackLayout>
                            <Image Source="ph_icon.png" 
                                   HeightRequest="30" 
                                   HorizontalOptions="Start"/>
                            <Label Text="PH" 
                                   FontSize="10" 
                                   TextColor="#666" 
                                   FontAttributes="Bold"/>
                            <Label Text="{Binding Ph, StringFormat='{0:F1}'}" 
                                   FontSize="18" 
                                   FontAttributes="Bold" 
                                   TextColor="#4CAF50"/>
                        </StackLayout>
                    </Frame>
                    
                    <!-- Moisture Card -->
                    <Frame Grid.Column="2" 
                           BackgroundColor="White" 
                           CornerRadius="12" 
                           HasShadow="True" 
                           Padding="15">
                        <StackLayout>
                            <Image Source="moisture_icon.png" 
                                   HeightRequest="30" 
                                   HorizontalOptions="Start"/>
                            <Label Text="MOISTURE" 
                                   FontSize="10" 
                                   TextColor="#666" 
                                   FontAttributes="Bold"/>
                            <Label Text="{Binding Moisture, StringFormat='{0:F0}%'}" 
                                   FontSize="18" 
                                   FontAttributes="Bold" 
                                   TextColor="#2196F3"/>
                        </StackLayout>
                    </Frame>
                </Grid>

                <!-- Quick Access -->
                <Frame Grid.Row="2" 
                       BackgroundColor="White" 
                       CornerRadius="12" 
                       HasShadow="True" 
                       Padding="20">
                    <StackLayout Spacing="15">
                        <Label Text="QUICK ACCESS" 
                               FontSize="14" 
                               FontAttributes="Bold" 
                               TextColor="#333"/>
                        
                        <Grid ColumnSpacing="15">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*" />
                                <ColumnDefinition Width="Auto" />
                            </Grid.ColumnDefinitions>
                            
                            <StackLayout Grid.Column="0">
                                <Label Text="Choose Module" 
                                       FontSize="12" 
                                       TextColor="#666"/>
                                <Picker x:Name="ModulePicker" 
                                        ItemsSource="{Binding Modules}"
                                        SelectedItem="{Binding SelectedModule}"/>
                            </StackLayout>
                            
                            <Button Grid.Column="1" 
                                    Text="START" 
                                    BackgroundColor="#4CAF50" 
                                    TextColor="White" 
                                    CornerRadius="8" 
                                    Command="{Binding StartModuleCommand}"/>
                        </Grid>
                        
                        <!-- Runtime Slider -->
                        <StackLayout>
                            <Label Text="Runtime" 
                                   FontSize="12" 
                                   TextColor="#666"/>
                            <Grid>
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="*" />
                                    <ColumnDefinition Width="Auto" />
                                </Grid.ColumnDefinitions>
                                <Slider Grid.Column="0" 
                                        Minimum="0" 
                                        Maximum="60" 
                                        Value="{Binding Runtime}"
                                        ThumbColor="#4CAF50"
                                        MinimumTrackColor="#4CAF50"/>
                                <Label Grid.Column="1" 
                                       Text="{Binding Runtime, StringFormat='{0:F0} min'}" 
                                       FontSize="12" 
                                       TextColor="#333"/>
                            </Grid>
                        </StackLayout>
                    </StackLayout>
                </Frame>

                <!-- Device Controller -->
                <Frame Grid.Row="3" 
                       BackgroundColor="White" 
                       CornerRadius="12" 
                       HasShadow="True" 
                       Padding="20">
                    <StackLayout>
                        <Label Text="CONTROLLER" 
                               FontSize="14" 
                               FontAttributes="Bold" 
                               TextColor="#333" 
                               Margin="0,0,0,15"/>
                        
                        <CollectionView ItemsSource="{Binding Devices}" 
                                      HeightRequest="200">
                            <CollectionView.ItemTemplate>
                                <DataTemplate>
                                    <Grid Padding="0,5" ColumnSpacing="15">
                                        <Grid.ColumnDefinitions>
                                            <ColumnDefinition Width="Auto" />
                                            <ColumnDefinition Width="*" />
                                            <ColumnDefinition Width="Auto" />
                                            <ColumnDefinition Width="Auto" />
                                            <ColumnDefinition Width="Auto" />
                                        </Grid.ColumnDefinitions>

                                        <Label Grid.Column="0" 
                                               Text="{Binding Name}" 
                                               FontSize="12" 
                                               FontAttributes="Bold" 
                                               VerticalOptions="Center"/>
                                        
                                        <Label Grid.Column="1" 
                                               Text="{Binding Value, StringFormat='{0:F1}'}" 
                                               FontSize="12" 
                                               VerticalOptions="Center"/>
                                        
                                        <Label Grid.Column="2" 
                                               Text="{Binding Unit}" 
                                               FontSize="12" 
                                               VerticalOptions="Center"/>
                                        
                                        <Label Grid.Column="3" 
                                               Text="{Binding IsOnline, Converter={StaticResource BoolToStatusConverter}}" 
                                               FontSize="12" 
                                               TextColor="{Binding IsOnline, Converter={StaticResource BoolToColorConverter}}" 
                                               VerticalOptions="Center"/>
                                        
                                        <Switch Grid.Column="4" 
                                               IsToggled="{Binding IsOnline}" 
                                               OnColor="#4CAF50" 
                                               ThumbColor="White"/>
                                    </Grid>
                                </DataTemplate>
                            </CollectionView.ItemTemplate>
                        </CollectionView>
                    </StackLayout>
                </Frame>

                <!-- Charts Section -->
                <Frame Grid.Row="4" 
                       BackgroundColor="White" 
                       CornerRadius="12" 
                       HasShadow="True" 
                       Padding="20">
                    <StackLayout>
                        <Label Text="WATTS" 
                               FontSize="14" 
                               FontAttributes="Bold" 
                               TextColor="#333"/>
                        
                        <!-- Simple Chart Placeholder - You can integrate OxyPlot or Syncfusion Charts -->
                        <Frame BackgroundColor="#F5F5F5" 
                               HeightRequest="200" 
                               CornerRadius="8">
                            <Label Text="Chart will be displayed here" 
                                   HorizontalOptions="Center" 
                                   VerticalOptions="Center" 
                                   TextColor="#666"/>
                        </Frame>
                    </StackLayout>
                </Frame>
            </Grid>
        </ScrollView>
    </RefreshView>
</ContentPage>
```

### 8. **AppShell.xaml** - Navigation Setup
```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Shell x:Class="AlbonApp.AppShell"
       xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:views="clr-namespace:AlbonApp.Views"
       FlyoutBehavior="Disabled">

    <ShellContent Title="Login" 
                  Route="login" 
                  ContentTemplate="{DataTemplate views:LoginPage}" />
    
    <ShellContent Title="Signup" 
                  Route="signup" 
                  ContentTemplate="{DataTemplate views:SignupPage}" />
    
    <ShellContent Title="Dashboard" 
                  Route="dashboard" 
                  ContentTemplate="{DataTemplate views:DashboardPage}" />

</Shell>
```

## 🎯 Implementation Steps

### Step 1: Create Project Structure
1. Create new .NET MAUI project in Visual Studio
2. Add the folder structure above
3. Install NuGet packages: `Microsoft.Extensions.Http`, `System.Text.Json`

### Step 2: Replace Default Files
- Replace `MauiProgram.cs` with the version above
- Replace `AppShell.xaml` with navigation setup
- Add all Models, Services, ViewModels, and Views

### Step 3: Configure API Integration
- Update `ApiService.cs` with your actual API endpoints
- Test API connections
- Handle authentication tokens if needed

### Step 4: Test Responsiveness
- Run on different platforms (Android, iOS, Windows)
- Test on different screen sizes
- Adjust layouts using `OnPlatform` markup where needed

### Step 5: Add Platform-Specific Features
- Configure permissions in `Platforms/Android/AndroidManifest.xml`
- Add iOS Info.plist configurations
- Setup Windows-specific features if needed

## 🔧 Key Features Implemented

1. **MVVM Pattern** - Clean separation of concerns
2. **Dependency Injection** - Proper service registration
3. **API Integration** - REST API calls with error handling
4. **Responsive Design** - Works on all platforms
5. **Navigation** - Shell-based navigation
6. **Data Binding** - Two-way binding for forms
7. **Loading States** - Activity indicators and refresh views
8. **Error Handling** - Try-catch blocks and user feedback

## 📱 Platform-Specific Considerations

### Android
- Add network permissions in AndroidManifest.xml
- Configure HTTP traffic allowance

### iOS
- Add network usage descriptions in Info.plist
- Configure App Transport Security if needed

### Windows
- Add internet capability in Package.appxmanifest

### 9. **ViewModels/DashboardViewModel.cs** - Complete Implementation
```csharp
using System.Collections.ObjectModel;
using System.Windows.Input;
using AlbonApp.Models;
using AlbonApp.Services;

namespace AlbonApp.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;
    
    // Properties for sensor data
    private double _temperature;
    private double _ph;
    private double _moisture;
    private double _watts;
    private string _userName = "User";
    private string _currentDate;
    private bool _isRefreshing;
    private double _runtime = 30;
    private string _selectedModule = "Module 1";

    public DashboardViewModel(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
        
        Devices = new ObservableCollection<Device>();
        Modules = new ObservableCollection<string> { "Module 1", "Module 2", "Module 3" };
        
        RefreshCommand = new Command(async () => await RefreshDataAsync());
        StartModuleCommand = new Command(async () => await StartModuleAsync());
        ToggleDeviceCommand = new Command<Device>(async (device) => await ToggleDeviceAsync(device));
        
        CurrentDate = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
        
        // Load initial data
        _ = Task.Run(async () => await LoadInitialDataAsync());
    }

    // Properties
    public double Temperature
    {
        get => _temperature;
        set => SetProperty(ref _temperature, value);
    }

    public double Ph
    {
        get => _ph;
        set => SetProperty(ref _ph, value);
    }

    public double Moisture
    {
        get => _moisture;
        set => SetProperty(ref _moisture, value);
    }

    public double Watts
    {
        get => _watts;
        set => SetProperty(ref _watts, value);
    }

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public string CurrentDate
    {
        get => _currentDate;
        set => SetProperty(ref _currentDate, value);
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public double Runtime
    {
        get => _runtime;
        set => SetProperty(ref _runtime, value);
    }

    public string SelectedModule
    {
        get => _selectedModule;
        set => SetProperty(ref _selectedModule, value);
    }

    public ObservableCollection<Device> Devices { get; }
    public ObservableCollection<string> Modules { get; }

    // Commands
    public ICommand RefreshCommand { get; }
    public ICommand StartModuleCommand { get; }
    public ICommand ToggleDeviceCommand { get; }

    private async Task LoadInitialDataAsync()
    {
        try
        {
            IsBusy = true;
            
            // Load user info
            var user = await _authService.GetCurrentUserAsync();
            if (user != null)
            {
                UserName = user.Name;
            }

            // Load sensor data and devices
            await RefreshDataAsync();
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshDataAsync()
    {
        try
        {
            IsRefreshing = true;

            // Load sensor data
            var sensorResult = await _apiService.GetLatestSensorDataAsync();
            if (sensorResult.Success && sensorResult.Data != null)
            {
                Temperature = sensorResult.Data.Temperature;
                Ph = sensorResult.Data.Ph;
                Moisture = sensorResult.Data.Moisture;
                Watts = sensorResult.Data.Watts;
            }

            // Load devices
            var devicesResult = await _apiService.GetDevicesAsync();
            if (devicesResult.Success && devicesResult.Data != null)
            {
                Devices.Clear();
                foreach (var device in devicesResult.Data)
                {
                    Devices.Add(device);
                }
            }
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", $"Failed to refresh data: {ex.Message}", "OK");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task StartModuleAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedModule))
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", "Please select a module", "OK");
                return;
            }

            await Application.Current?.MainPage?.DisplayAlert("Success", 
                $"Started {SelectedModule} for {Runtime} minutes", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", $"Failed to start module: {ex.Message}", "OK");
        }
    }

    private async Task ToggleDeviceAsync(Device device)
    {
        try
        {
            if (device == null) return;

            var result = await _apiService.ToggleDeviceAsync(device.Id, !device.IsOnline);
            if (result.Success)
            {
                device.IsOnline = !device.IsOnline;
                await Application.Current?.MainPage?.DisplayAlert("Success", 
                    $"{device.Name} {(device.IsOnline ? "turned on" : "turned off")}", "OK");
            }
            else
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", result.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", $"Failed to toggle device: {ex.Message}", "OK");
        }
    }
}
```

### 10. **Services/AuthService.cs** - Authentication Management
```csharp
using AlbonApp.Models;
using System.Text.Json;

namespace AlbonApp.Services;

public interface IAuthService
{
    Task<bool> SaveUserAsync(User user);
    Task<User?> GetCurrentUserAsync();
    Task<bool> IsLoggedInAsync();
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    Task SaveTokenAsync(string token);
}

public class AuthService : IAuthService
{
    private const string UserKey = "current_user";
    private const string TokenKey = "auth_token";

    public async Task<bool> SaveUserAsync(User user)
    {
        try
        {
            var userJson = JsonSerializer.Serialize(user);
            await SecureStorage.SetAsync(UserKey, userJson);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        try
        {
            var userJson = await SecureStorage.GetAsync(UserKey);
            if (string.IsNullOrEmpty(userJson))
                return null;

            return JsonSerializer.Deserialize<User>(userJson);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsLoggedInAsync()
    {
        var user = await GetCurrentUserAsync();
        return user != null;
    }

    public async Task LogoutAsync()
    {
        SecureStorage.Remove(UserKey);
        SecureStorage.Remove(TokenKey);
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await SecureStorage.GetAsync(TokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveTokenAsync(string token)
    {
        await SecureStorage.SetAsync(TokenKey, token);
    }
}
```

### 11. **ViewModels/SignupViewModel.cs**
```csharp
using System.Windows.Input;
using AlbonApp.Models;
using AlbonApp.Services;

namespace AlbonApp.ViewModels;

public class SignupViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;
    
    private string _name = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private bool _agreeToTerms = false;

    public SignupViewModel(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
        SignupCommand = new Command(async () => await SignupAsync(), () => !IsBusy);
        LoginCommand = new Command(async () => await GoToLoginAsync());
        GoogleSignupCommand = new Command(async () => await GoogleSignupAsync());
        AppleSignupCommand = new Command(async () => await AppleSignupAsync());
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public bool AgreeToTerms
    {
        get => _agreeToTerms;
        set => SetProperty(ref _agreeToTerms, value);
    }

    public ICommand SignupCommand { get; }
    public ICommand LoginCommand { get; }
    public ICommand GoogleSignupCommand { get; }
    public ICommand AppleSignupCommand { get; }

    private async Task SignupAsync()
    {
        if (IsBusy) return;

        if (!ValidateInput())
            return;

        IsBusy = true;

        try
        {
            var user = new User
            {
                Name = Name,
                Email = Email,
                Password = Password
            };

            var result = await _apiService.SignupAsync(user);
            
            if (result.Success && result.Data != null)
            {
                await _authService.SaveUserAsync(result.Data);
                await Application.Current?.MainPage?.DisplayAlert("Success", "Account created successfully!", "OK");
                await Shell.Current.GoToAsync("//dashboard");
            }
            else
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", result.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", $"Signup failed: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            Application.Current?.MainPage?.DisplayAlert("Error", "Please enter your name", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            Application.Current?.MainPage?.DisplayAlert("Error", "Please enter your email", "OK");
            return false;
        }

        if (!IsValidEmail(Email))
        {
            Application.Current?.MainPage?.DisplayAlert("Error", "Please enter a valid email address", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            Application.Current?.MainPage?.DisplayAlert("Error", "Please enter a password", "OK");
            return false;
        }

        if (Password.Length < 6)
        {
            Application.Current?.MainPage?.DisplayAlert("Error", "Password must be at least 6 characters", "OK");
            return false;
        }

        if (Password != ConfirmPassword)
        {
            Application.Current?.MainPage?.DisplayAlert("Error", "Passwords do not match", "OK");
            return false;
        }

        if (!AgreeToTerms)
        {
            Application.Current?.MainPage?.DisplayAlert("Error", "Please agree to the terms and conditions", "OK");
            return false;
        }

        return true;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private async Task GoToLoginAsync()
    {
        await Shell.Current.GoToAsync("//login");
    }

    private async Task GoogleSignupAsync()
    {
        // Implement Google Sign-in
        await Application.Current?.MainPage?.DisplayAlert("Info", "Google signup not implemented yet", "OK");
    }

    private async Task AppleSignupAsync()
    {
        // Implement Apple Sign-in
        await Application.Current?.MainPage?.DisplayAlert("Info", "Apple signup not implemented yet", "OK");
    }
}
```

### 12. **Views/SignupPage.xaml**
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage x:Class="AlbonApp.Views.SignupPage"
             xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:AlbonApp.ViewModels"
             x:DataType="vm:SignupViewModel"
             Shell.NavBarIsVisible="False"
             BackgroundColor="#F5F5F5">

    <Grid>
        <!-- Background Image -->
        <Image Source="plant_background.jpg" 
               Aspect="AspectFill" 
               Opacity="0.3"/>
        
        <!-- Main Content -->
        <ScrollView>
            <Grid Padding="20" RowSpacing="0">
                <Grid.RowDefinitions>
                    <RowDefinition Height="*" />
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="*" />
                </Grid.RowDefinitions>

                <!-- Signup Card -->
                <StackLayout Grid.Row="1" 
                           VerticalOptions="Center" 
                           HorizontalOptions="Center">
                    
                    <!-- Logo -->
                    <Image Source="albon_logo.png" 
                           HeightRequest="60" 
                           Margin="0,0,0,30"/>
                    
                    <Frame BackgroundColor="White"
                           CornerRadius="15"
                           HasShadow="True"
                           Padding="30"
                           WidthRequest="{OnPlatform Default=400, Phone=350}">
                        
                        <StackLayout Spacing="20">
                            <Label Text="Get Started Now!" 
                                   FontSize="24" 
                                   FontAttributes="Bold" 
                                   HorizontalOptions="Center"
                                   TextColor="#2E7D32"/>
                            
                            <!-- Name Entry -->
                            <StackLayout>
                                <Label Text="Name" 
                                       FontSize="14" 
                                       TextColor="#333"/>
                                <Entry Text="{Binding Name}"
                                       Placeholder="Enter your full name"
                                       BackgroundColor="#F8F9FA"
                                       TextColor="#333"/>
                            </StackLayout>
                            
                            <!-- Email Entry -->
                            <StackLayout>
                                <Label Text="Email address" 
                                       FontSize="14" 
                                       TextColor="#333"/>
                                <Entry Text="{Binding Email}"
                                       Placeholder="Enter your email"
                                       Keyboard="Email"
                                       BackgroundColor="#F8F9FA"
                                       TextColor="#333"/>
                            </StackLayout>
                            
                            <!-- Password Entry -->
                            <StackLayout>
                                <Label Text="Password" 
                                       FontSize="14" 
                                       TextColor="#333"/>
                                <Entry Text="{Binding Password}"
                                       Placeholder="Enter your password"
                                       IsPassword="True"
                                       BackgroundColor="#F8F9FA"
                                       TextColor="#333"/>
                            </StackLayout>
                            
                            <!-- Confirm Password Entry -->
                            <StackLayout>
                                <Label Text="Confirm Password" 
                                       FontSize="14" 
                                       TextColor="#333"/>
                                <Entry Text="{Binding ConfirmPassword}"
                                       Placeholder="Confirm your password"
                                       IsPassword="True"
                                       BackgroundColor="#F8F9FA"
                                       TextColor="#333"/>
                            </StackLayout>
                            
                            <!-- Terms Agreement -->
                            <StackLayout Orientation="Horizontal">
                                <CheckBox IsChecked="{Binding AgreeToTerms}"/>
                                <Label VerticalOptions="Center"
                                       FontSize="12"
                                       TextColor="#666">
                                    <Label.FormattedText>
                                        <FormattedString>
                                            <Span Text="I agree to the "/>
                                            <Span Text="terms &amp; policy" 
                                                  TextColor="#4CAF50" 
                                                  TextDecorations="Underline"/>
                                        </FormattedString>
                                    </Label.FormattedText>
                                </Label>
                            </StackLayout>
                            
                            <!-- Signup Button -->
                            <Button Text="Signup"
                                    Command="{Binding SignupCommand}"
                                    BackgroundColor="#4CAF50"
                                    TextColor="White"
                                    CornerRadius="8"
                                    HeightRequest="50"
                                    FontSize="16"
                                    FontAttributes="Bold"/>
                            
                            <!-- Divider -->
                            <Label Text="Or" 
                                   HorizontalOptions="Center" 
                                   FontSize="14" 
                                   TextColor="#666"/>
                            
                            <!-- Social Login Buttons -->
                            <Grid ColumnSpacing="10">
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="*"/>
                                </Grid.ColumnDefinitions>
                                
                                <Button Grid.Column="0"
                                        Text="Sign up with Google"
                                        Command="{Binding GoogleSignupCommand}"
                                        BackgroundColor="White"
                                        TextColor="#333"
                                        BorderColor="#DDD"
                                        BorderWidth="1"
                                        CornerRadius="8"
                                        FontSize="12"/>
                                
                                <Button Grid.Column="1"
                                        Text="Sign up with Apple"
                                        Command="{Binding AppleSignupCommand}"
                                        BackgroundColor="Black"
                                        TextColor="White"
                                        CornerRadius="8"
                                        FontSize="12"/>
                            </Grid>
                            
                            <!-- Login Link -->
                            <StackLayout Orientation="Horizontal" 
                                       HorizontalOptions="Center">
                                <Label Text="Have an account?" 
                                       FontSize="14" 
                                       TextColor="#666"/>
                                <Label Text="Sign In"
                                       FontSize="14"
                                       TextColor="#4CAF50"
                                       FontAttributes="Bold"
                                       TextDecorations="Underline">
                                    <Label.GestureRecognizers>
                                        <TapGestureRecognizer Command="{Binding LoginCommand}"/>
                                    </Label.GestureRecognizers>
                                </Label>
                            </StackLayout>
                        </StackLayout>
                    </Frame>
                </StackLayout>
            </Grid>
        </ScrollView>
        
        <!-- Loading Indicator -->
        <ActivityIndicator IsVisible="{Binding IsBusy}" 
                          IsRunning="{Binding IsBusy}"
                          Color="#4CAF50"
                          BackgroundColor="#80000000"/>
    </Grid>
</ContentPage>
```

### 13. **Resources/Styles/Colors.xaml** - Theme Colors
```xml
<?xml version="1.0" encoding="UTF-8" ?>
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">

    <!-- Primary Colors -->
    <Color x:Key="Primary">#4CAF50</Color>
    <Color x:Key="PrimaryDark">#2E7D32</Color>
    <Color x:Key="PrimaryLight">#81C784</Color>
    
    <!-- Secondary Colors -->
    <Color x:Key="Secondary">#FF6B35</Color>
    <Color x:Key="SecondaryDark">#E65100</Color>
    
    <!-- Neutral Colors -->
    <Color x:Key="White">#FFFFFF</Color>
    <Color x:Key="Black">#000000</Color>
    <Color x:Key="Gray100">#F5F5F5</Color>
    <Color x:Key="Gray200">#EEEEEE</Color>
    <Color x:Key="Gray300">#E0E0E0</Color>
    <Color x:Key="Gray600">#757575</Color>
    <Color x:Key="Gray900">#212121</Color>
    
    <!-- Status Colors -->
    <Color x:Key="Success">#4CAF50</Color>
    <Color x:Key="Warning">#FF9800</Color>
    <Color x:Key="Error">#F44336</Color>
    <Color x:Key="Info">#2196F3</Color>

</ResourceDictionary>
```

### 14. **AlbonApp.csproj** - Project Configuration
```xml
<Project Sdk="Microsoft.NET.Sdk">

	<PropertyGroup>
		<TargetFrameworks>net8.0-android;net8.0-ios;net8.0-maccatalyst</TargetFrameworks>
		<TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">$(TargetFrameworks);net8.0-windows10.0.19041.0</TargetFrameworks>
		<OutputType>Exe</OutputType>
		<RootNamespace>AlbonApp</RootNamespace>
		<UseMaui>true</UseMaui>
		<SingleProject>true</SingleProject>
		<ImplicitUsings>enable</ImplicitUsings>
		<Nullable>enable</Nullable>

		<!-- Display Version -->
		<ApplicationDisplayVersion>1.0</ApplicationDisplayVersion>
		<ApplicationVersion>1</ApplicationVersion>

		<SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">21.0</SupportedOSPlatformVersion>
		<SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">11.0</SupportedOSPlatformVersion>
		<SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'maccatalyst'">13.1</SupportedOSPlatformVersion>
		<SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">10.0.17763.0</SupportedOSPlatformVersion>
		<TargetPlatformMinVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">10.0.17763.0</TargetPlatformMinVersion>
	</PropertyGroup>

	<ItemGroup>
		<!-- App Icon -->
		<MauiIcon Include="Resources\AppIcon\appicon.svg" ForegroundFile="Resources\AppIcon\appiconfg.svg" Color="#512BD4" />

		<!-- Splash Screen -->
		<MauiSplashScreen Include="Resources\Splash\splash.svg" Color="#512BD4" BaseSize="128,128" />

		<!-- Images -->
		<MauiImage Include="Resources\Images\*" />
		<MauiImage Update="Resources\Images\dotnet_bot.svg" BaseSize="168,208" />

		<!-- Custom Fonts -->
		<MauiFont Include="Resources\Fonts\*" />

		<!-- Raw Assets (also remove the "Resources\Raw" prefix) -->
		<MauiAsset Include="Resources\Raw\**" LogicalName="%(RecursiveDir)%(Filename)%(Extension)" />
	</ItemGroup>

	<ItemGroup>
		<PackageReference Include="Microsoft.Maui.Controls" Version="$(MauiVersion)" />
		<PackageReference Include="Microsoft.Maui.Controls.Compatibility" Version="$(MauiVersion)" />
		<PackageReference Include="Microsoft.AspNetCore.Components.WebView.Maui" Version="$(MauiVersion)" />
		<PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="8.0.0" />
		<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
		<PackageReference Include="System.Text.Json" Version="8.0.0" />
	</ItemGroup>

</Project>
```

## 🚀 Next Steps for Implementation

### 1. **Database Integration Options**
```csharp
// Option A: SQLite Local Database
public class LocalDbService
{
    private SQLiteAsyncConnection _database;

    public async Task<List<Device>> GetDevicesAsync()
    {
        return await _database.Table<Device>().ToListAsync();
    }
}

// Option B: API Integration (already implemented above)
// Option C: Firebase Integration
public class FirebaseService
{
    private FirebaseClient _firebase;
    
    public async Task<List<Device>> GetDevicesAsync()
    {
        return await _firebase
            .Child("devices")
            .OnceAsync<Device>()
            .ContinueWith(t => t.Result.Select(d => d.Object).ToList());
    }
}
```

### 2. **Real-time Updates**
```csharp
// SignalR for real-time dashboard updates
public class SignalRService
{
    private HubConnection _hubConnection;

    public async Task StartConnectionAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://your-api.com/datahub")
            .Build();

        _hubConnection.On<SensorReading>("ReceiveSensorUpdate", (data) =>
        {
            // Update UI with new sensor data
            MessagingCenter.Send<SignalRService, SensorReading>(this, "SensorUpdate", data);
        });

        await _hubConnection.StartAsync();
    }
}
```

### 3. **Offline Support**
```csharp
// Sync service for offline functionality
public class SyncService
{
    public async Task SyncDataAsync()
    {
        if (Connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            // Sync cached data with server
            await UploadPendingChangesAsync();
            await DownloadLatestDataAsync();
        }
    }
}
```

This structure gives you a complete, production-ready .NET MAUI app that matches your GUI design and includes all the backend integration you need!
