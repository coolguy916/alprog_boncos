# 📚 Dokumentasi API Backend - Panduan Ngoding

## 🎯 Pengenalan
API ini memungkinkan aplikasi mobile (MAUI) untuk berkomunikasi dengan database MySQL melalui internet. Seperti jembatan yang menghubungkan aplikasi dengan database.

## 🌐 URL Dasar API
```
https://yourserver.com/api/
```
*Ganti `yourserver.com` dengan domain hosting Anda*

---

## 📤 1. MENGIRIM DATA KE DATABASE (POST)

### 🔗 Endpoint
```
POST /api/maui-data
```

### 📝 Penjelasan
Digunakan untuk **menambahkan data baru** ke dalam database. Seperti mengisi formulir dan mengirimkannya ke server.

### 📊 Format Data yang Dikirim
```json
{
    "tableName": "nama_tabel_database",
    "records": [
        {
            "kolom1": "nilai1",
            "kolom2": "nilai2",
            "kolom3": "nilai3"
        }
    ]
}
```

### 🎯 Contoh Penggunaan untuk Tabel `storage_barang`
```json
{
    "tableName": "storage_barang",
    "records": [
        {
            "nama_barang": "Laptop Asus",
            "kategori": "Elektronik",
            "jumlah": 5,
            "harga": 8500000,
            "tanggal_input": "2025-06-17 14:30:00"
        }
    ]
}
```

### ✅ Response Sukses
```json
{
    "success": true,
    "message": "Successfully inserted 1 records into 'storage_barang'.",
    "insertedIds": [123]
}
```

### ❌ Response Error
```json
{
    "success": false,
    "error": "A 'tableName' string is required in the request body."
}
```

---

## 📥 2. MENGAMBIL DATA DARI DATABASE (GET)

### 🔗 Endpoint
```
GET /api/maui-get/{nama_tabel}
```

### 📝 Penjelasan
Digunakan untuk **mengambil/membaca data** dari database. Seperti membuka file dan membaca isinya.

### 🎯 Contoh URL Lengkap
```
GET /api/maui-get/storage_barang
```

### 🔍 Filter Data (Opsional)
Untuk mengambil data dengan kondisi tertentu:
```
GET /api/maui-get/storage_barang?filters[kategori]=Elektronik
```

### 📊 Parameter Query yang Tersedia
| Parameter | Keterangan | Contoh |
|-----------|------------|---------|
| `filters[kolom]` | Filter berdasarkan nilai kolom | `filters[kategori]=Elektronik` |
| `orderBy[column]` | Urutkan berdasarkan kolom | `orderBy[column]=nama_barang` |
| `orderBy[direction]` | Arah urutan (ASC/DESC) | `orderBy[direction]=ASC` |
| `limit` | Batasi jumlah data | `limit=10` |

### 🎯 Contoh URL dengan Filter Lengkap
```
GET /api/maui-get/storage_barang?filters[kategori]=Elektronik&orderBy[column]=harga&orderBy[direction]=DESC&limit=5
```

### ✅ Response Sukses
```json
{
    "success": true,
    "message": "Successfully retrieved 2 records from 'storage_barang'.",
    "data": [
        {
            "id": "1",
            "nama_barang": "Laptop Asus",
            "kategori": "Elektronik",
            "jumlah": "5",
            "harga": "8500000",
            "tanggal_input": "2025-06-17 14:30:00"
        },
        {
            "id": "2",
            "nama_barang": "Mouse Logitech",
            "kategori": "Elektronik",
            "jumlah": "10",
            "harga": "250000",
            "tanggal_input": "2025-06-17 15:00:00"
        }
    ],
    "count": 2
}
```

### ❌ Response Error
```json
{
    "success": false,
    "error": "Table name is required in URL."
}
```

---

## 🔧 3. CEK STATUS API (Health Check)

### 🔗 Endpoint
```
GET /api/health
```

### 📝 Penjelasan
Digunakan untuk **mengecek apakah API berjalan normal**. Seperti mengecek apakah server masih hidup.

### ✅ Response Sukses
```json
{
    "success": true,
    "message": "API is running",
    "timestamp": "2025-06-17 14:30:00",
    "server_info": {
        "php_version": "8.1.0",
        "request_method": "GET",
        "request_uri": "/api/health",
        "parsed_uri": "/api/health"
    }
}
```

---

## 🗄️ 4. STRUKTUR TABEL DATABASE

### Tabel `storage_barang`
```sql
CREATE TABLE storage_barang (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nama_barang VARCHAR(255) NOT NULL,
    kategori VARCHAR(100) NOT NULL,
    jumlah INT NOT NULL,
    harga DECIMAL(15,2) NOT NULL,
    tanggal_input DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

### 📝 Penjelasan Kolom
- `id`: Nomor unik otomatis untuk setiap barang
- `nama_barang`: Nama barang (contoh: "Laptop Asus")
- `kategori`: Kategori barang (contoh: "Elektronik")
- `jumlah`: Jumlah stok barang (angka)
- `harga`: Harga barang dalam Rupiah (angka)
- `tanggal_input`: Waktu data dimasukkan (otomatis)

---

## 🛠️ 5. CARA PENGGUNAAN DI APLIKASI C#

### 📤 Mengirim Data (POST)
```csharp
// 1. Buat data JSON
var dataBarang = new
{
    tableName = "storage_barang",
    records = new[]
    {
        new
        {
            nama_barang = "Laptop Asus",
            kategori = "Elektronik",
            jumlah = 5,
            harga = 8500000,
            tanggal_input = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        }
    }
};

// 2. Konversi ke JSON string
string jsonData = JsonSerializer.Serialize(dataBarang);

// 3. Kirim ke server
var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
HttpResponseMessage response = await httpClient.PostAsync("https://yourserver.com/api/maui-data", content);
```

### 📥 Mengambil Data (GET)
```csharp
// 1. Buat URL dengan filter (opsional)
string url = "https://yourserver.com/api/maui-get/storage_barang?filters[kategori]=Elektronik";

// 2. Kirim request GET
HttpResponseMessage response = await httpClient.GetAsync(url);

// 3. Baca response
string responseContent = await response.Content.ReadAsStringAsync();
```

---

## ⚠️ 6. ERROR YANG SERING TERJADI

### 1. **Connection Error**
```
Koneksi internet bermasalah
```
**Solusi**: Cek koneksi internet dan pastikan URL server benar

### 2. **Timeout Error**
```
Request timeout - server terlalu lama merespons
```
**Solusi**: Server lambat atau down, coba lagi nanti

### 3. **Invalid JSON**
```
Invalid JSON input
```
**Solusi**: Pastikan format JSON benar sesuai contoh

### 4. **Table Not Found**
```
Table 'nama_tabel' doesn't exist
```
**Solusi**: Pastikan nama tabel database benar

### 5. **Missing Required Fields**
```
A 'tableName' string is required in the request body
```
**Solusi**: Pastikan semua field wajib diisi

---

## 🚀 7. TIPS UNTUK PEMULA

### ✅ Do's (Lakukan)
- Selalu cek koneksi internet sebelum kirim request
- Gunakan try-catch untuk menangani error
- Validasi input sebelum kirim ke server
- Tampilkan loading indicator saat proses berlangsung
- Log response untuk debugging

### ❌ Don'ts (Jangan)
- Jangan hardcode URL server di kode (gunakan config)
- Jangan kirim request tanpa timeout
- Jangan abaikan response error dari server
- Jangan lupa dispose HttpClient
- Jangan tampilkan error teknis ke user

### 🔧 Contoh Error Handling
```csharp
try
{
    // Kirim request
    var response = await httpClient.PostAsync(url, content);
    
    if (response.IsSuccessStatusCode)
    {
        // Berhasil
        await DisplayAlert("Sukses", "Data berhasil disimpan!", "OK");
    }
    else
    {
        // Server error
        await DisplayAlert("Error", "Server bermasalah, coba lagi nanti", "OK");
    }
}
catch (HttpRequestException)
{
    // Koneksi bermasalah
    await DisplayAlert("Error", "Cek koneksi internet Anda", "OK");
}
catch (TaskCanceledException)
{
    // Timeout
    await DisplayAlert("Error", "Server terlalu lama merespons", "OK");
}
```

---

## 📞 8. BANTUAN LEBIH LANJUT

Jika mengalami kesulitan:
