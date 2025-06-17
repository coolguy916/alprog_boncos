using System.Text;
using System.Text.Json;

namespace StorageApp;

public partial class MainPage : ContentPage
{
    // 🌐 URL ke server API (WAJIB ganti ke URL server milikmu ya!)
    private const string API_BASE_URL = "https://yourserver.com";

    // 💡 Objek HTTP Client — alat komunikasi utama dengan server
    private readonly HttpClient httpClient;

    public MainPage()
    {
        InitializeComponent();

        // ⚙️ Siapkan koneksi internet
        httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30) // ⏳ Biar gak nunggu selamanya
        };

        // 📦 Kasih tau server kalau kita bakal kirim data JSON
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    // 🛒 Fungsi yang dijalankan saat user klik tombol "Tambah Barang"
    private async void OnTambahBarangClicked(object sender, EventArgs e)
    {
        try
        {
            TampilkanLoading(true, "📤 Mengirim data ke server...");

            // 🚨 Validasi data kosong
            if (string.IsNullOrWhiteSpace(NamaBarangEntry.Text) ||
                string.IsNullOrWhiteSpace(KategoriEntry.Text) ||
                string.IsNullOrWhiteSpace(JumlahEntry.Text) ||
                string.IsNullOrWhiteSpace(HargaEntry.Text))
            {
                await DisplayAlert("❗Oops", "Semua field harus diisi ya!", "OK");
                return;
            }

            // 🔢 Validasi angka - pastikan input valid
            if (!int.TryParse(JumlahEntry.Text, out int jumlah) || jumlah <= 0)
            {
                await DisplayAlert("⚠️ Error", "Jumlah harus angka positif!", "OK");
                return;
            }

            if (!decimal.TryParse(HargaEntry.Text, out decimal harga) || harga <= 0)
            {
                await DisplayAlert("⚠️ Error", "Harga harus angka positif!", "OK");
                return;
            }

            // 🔽✨=== BAGIAN PENTING: DATA YANG DIKIRIM KE SERVER ===✨🔽
            var dataBarang = new
            {
                tableName = "storage_barang", // 🗂️ Nama tabel di database server
                records = new[]
                {
                    new
                    {
                        nama_barang = NamaBarangEntry.Text.Trim(),
                        kategori = KategoriEntry.Text.Trim(),
                        jumlah = jumlah,
                        harga = harga,
                        tanggal_input = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") // ⏰ Timestamp otomatis
                    }
                }
            };
            // 🔼✨=== SELESAI BLOK PENTING DATA BARANG ===🔼

            // 🚀 Kirim data ke server
            string jsonData = JsonSerializer.Serialize(dataBarang);
            bool berhasil = await KirimDataKeServer(jsonData);

            if (berhasil)
            {
                // 🎉 Kosongkan form dan tampilkan notifikasi
                NamaBarangEntry.Text = "";
                KategoriEntry.Text = "";
                JumlahEntry.Text = "";
                HargaEntry.Text = "";

                await DisplayAlert("✅ Sukses", "Data berhasil ditambahkan!", "OK");
                StatusLabel.Text = "✅ Data berhasil disimpan ke database";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("❌ Error", $"Terjadi kesalahan: {ex.Message}", "OK");
            StatusLabel.Text = "❌ Gagal menyimpan data";
        }
        finally
        {
            TampilkanLoading(false);
        }
    }

    // 🧲 Fungsi yang dijalankan saat tombol "Ambil Data" diklik
    private async void OnAmbilDataClicked(object sender, EventArgs e)
    {
        try
        {
            TampilkanLoading(true, "📡 Mengambil data dari server...");

            string hasilData = await AmbilDataDariServer();

            if (!string.IsNullOrEmpty(hasilData))
            {
                HasilLabel.Text = hasilData;
                HasilFrame.IsVisible = true;
                StatusLabel.Text = "📥 Data berhasil diambil dari database";
            }
            else
            {
                StatusLabel.Text = "⚠️ Tidak ada data yang ditemukan";
                HasilFrame.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("❌ Error", $"Gagal mengambil data: {ex.Message}", "OK");
            StatusLabel.Text = "❌ Gagal mengambil data";
            HasilFrame.IsVisible = false;
        }
        finally
        {
            TampilkanLoading(false);
        }
    }

    // ✨🔥=== FUNGSI INTI UNTUK MENGIRIM (UPLOAD) DATA KE SERVER ===🔥✨
    private async Task<bool> KirimDataKeServer(string jsonData)
    {
        try
        {
            string url = $"{API_BASE_URL}/api/maui-data";

            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync(url, content);

            string responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"📨 Response: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                using (JsonDocument doc = JsonDocument.Parse(responseContent))
                {
                    if (doc.RootElement.TryGetProperty("success", out JsonElement successElement))
                        return successElement.GetBoolean();
                }
                return true; // Default jika tidak ada properti "success"
            }
            else
            {
                throw new Exception($"⚠️ Server Error: {response.StatusCode} - {responseContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"📴 Koneksi internet bermasalah: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            throw new Exception("⏱️ Request timeout - Server terlalu lama merespons");
        }
    }

    // ✨📥=== FUNGSI INTI UNTUK MENGAMBIL (GET) DATA DARI SERVER ===📥✨
    private async Task<string> AmbilDataDariServer()
    {
        try
        {
            string url = $"{API_BASE_URL}/api/maui-get/storage_barang";

            if (!string.IsNullOrWhiteSpace(FilterKategoriEntry.Text))
            {
                url += $"?filters[kategori]={Uri.EscapeDataString(FilterKategoriEntry.Text.Trim())}";
            }

            HttpResponseMessage response = await httpClient.GetAsync(url);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using (JsonDocument doc = JsonDocument.Parse(responseContent))
                {
                    var root = doc.RootElement;

                    if (root.TryGetProperty("success", out JsonElement successElement) &&
                        successElement.GetBoolean())
                    {
                        if (root.TryGetProperty("data", out JsonElement dataElement) &&
                            dataElement.ValueKind == JsonValueKind.Array)
                        {
                            return FormatDataUntukTampilan(dataElement);
                        }
                    }
                    else if (root.TryGetProperty("error", out JsonElement errorElement))
                    {
                        throw new Exception(errorElement.GetString());
                    }
                }
                return "🔍 Tidak ada data ditemukan";
            }
            else
            {
                throw new Exception($"⚠️ Server error: {response.StatusCode} - {responseContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"📴 Koneksi bermasalah: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            throw new Exception("⏱️ Timeout - Server lambat merespons");
        }
    }

    // 📝 Konversi JSON data ke teks yang lebih enak dibaca di UI
    private string FormatDataUntukTampilan(JsonElement dataArray)
    {
        var result = new StringBuilder();
        result.AppendLine($"📊 Total Data: {dataArray.GetArrayLength()}\n");

        int nomor = 1;
        foreach (JsonElement item in dataArray.EnumerateArray())
        {
            result.AppendLine($"🔹 Data #{nomor++}:");

            foreach (JsonProperty property in item.EnumerateObject())
            {
                string value = property.Value.ValueKind == JsonValueKind.String
                    ? property.Value.GetString()
                    : property.Value.ToString();

                result.AppendLine($"   {property.Name}: {value}");
            }

            result.AppendLine(); // Tambah jarak antar data
        }

        return result.ToString();
    }

    // 🔄 Tampilkan atau sembunyikan animasi loading saat proses berjalan
    private void TampilkanLoading(bool tampilkan, string pesan = "")
    {
        LoadingIndicator.IsVisible = tampilkan;
        LoadingIndicator.IsRunning = tampilkan;

        if (tampilkan)
        {
            StatusLabel.Text = pesan;
        }
    }

    // 🚪 Bersihkan saat halaman ditutup
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        httpClient?.Dispose();
    }
}
