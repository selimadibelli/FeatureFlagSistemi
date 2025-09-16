# Feature Flag System

Bu proje, dolandırıcılık önleme sistemleri için pilot whitelist yönetimi sağlayan bir feature flag sistemidir. Vertical slice architecture kullanılarak geliştirilmiştir.

## Özellikler

- ✅ **Feature Flag Yönetimi**: Feature flag'leri oluşturma, güncelleme, silme
- ✅ **Pilot Whitelist**: Kullanıcıları pilot programına ekleme/çıkarma
- ✅ **Redis Cache**: Hızlı erişim için Redis cache desteği
- ✅ **Version Control**: Minimum uygulama versiyonu kontrolü
- ✅ **Expiration**: Pilot süresi bitiş tarihi desteği
- ✅ **RESTful API**: Swagger UI ile dokümantasyon
- ✅ **Vertical Slice Architecture**: Temiz ve sürdürülebilir kod yapısı

## Teknolojiler

- **.NET 8.0**
- **Entity Framework Core**
- **SQL Server**
- **Redis**
- **Minimal API**
- **Serilog**
- **Swagger/OpenAPI**

## Kurulum

### Gereksinimler

- .NET 8.0 SDK
- SQL Server (LocalDB yeterli)
- Redis Server

### Adımlar

1. **Projeyi klonlayın**
   ```bash
   git clone <repository-url>
   cd FeatureFlagSystem
   ```

2. **Bağımlılıkları yükleyin**
   ```bash
   dotnet restore
   ```

3. **Veritabanını oluşturun**
   ```bash
   dotnet ef database update
   ```

4. **Redis'i başlatın**
   ```bash
   # Windows için Redis'i indirip çalıştırın
   # Veya Docker kullanın:
   docker run -d -p 6379:6379 redis:alpine
   ```

5. **Uygulamayı çalıştırın**
   ```bash
   dotnet run
   ```

6. **Swagger UI'a erişin**
   ```
   http://localhost:5000
   ```

## API Endpoints

### Feature Flag Check
- `POST /api/feature-flag-check/check` - Tek feature flag kontrolü
- `POST /api/feature-flag-check/check-multiple` - Çoklu feature flag kontrolü

### Feature Flag Management
- `GET /api/feature-flags` - Tüm feature flag'leri listele
- `GET /api/feature-flags/{id}` - ID'ye göre feature flag getir
- `GET /api/feature-flags/by-name/{name}` - İsme göre feature flag getir
- `POST /api/feature-flags` - Yeni feature flag oluştur
- `PUT /api/feature-flags/{id}` - Feature flag güncelle
- `DELETE /api/feature-flags/{id}` - Feature flag sil

### Pilot Whitelist Management
- `GET /api/feature-flags/{id}/pilot-whitelist` - Pilot whitelist'i getir
- `POST /api/feature-flags/pilot-whitelist` - Pilot whitelist'e ekle
- `DELETE /api/feature-flags/pilot-whitelist/{id}` - Pilot whitelist'ten çıkar

### Cache Management
- `POST /api/feature-flags/refresh-cache` - Cache'i yenile

## Kullanım Örnekleri

### Feature Flag Kontrolü

```json
POST /api/feature-flag-check/check
{
  "featureName": "OpenBankingPilot",
  "userIdentifier": "CUST001",
  "userType": "Customer",
  "appVersion": "1.2.0"
}
```

**Yanıt:**
```json
{
  "isEnabled": true,
  "isInPilot": true,
  "reason": "Pilot kullanıcısı",
  "checkedAt": "2024-01-15T10:30:00Z"
}
```

### Yeni Feature Flag Oluşturma

```json
POST /api/feature-flags
{
  "name": "NewFeature",
  "description": "Yeni özellik açıklaması",
  "isEnabled": true,
  "createdBy": "admin"
}
```

### Pilot Whitelist'e Ekleme

```json
POST /api/feature-flags/pilot-whitelist
{
  "featureFlagId": 1,
  "userIdentifier": "CUST002",
  "userType": "Customer",
  "minVersion": "1.1.0",
  "expiresAt": "2024-02-15T00:00:00Z",
  "createdBy": "admin"
}
```

## Proje Yapısı

```
FeatureFlagSystem/
├── Domain/
│   ├── Entities/          # Veritabanı modelleri
│   └── DTOs/             # Data Transfer Objects
├── Infrastructure/
│   ├── Data/             # DbContext
│   └── Services/         # Cache servisleri
├── Application/
│   └── Services/         # Business logic servisleri
├── Features/             # Vertical slice architecture
│   ├── FeatureFlagCheck/
│   └── FeatureFlagManagement/
└── Program.cs            # Uygulama giriş noktası
```

## Cache Stratejisi

- Feature flag'ler Redis'te 30 dakika cache'lenir
- Her CRUD işleminde cache otomatik temizlenir
- Cache key pattern: `feature_flag:{name}` veya `feature_flag:id:{id}`
- Tüm feature flag'ler için: `all_feature_flags`

## Güvenlik

- CORS politikası yapılandırılmış
- Input validation
- Error handling
- Logging (Serilog)

## Geliştirme

### Yeni Feature Ekleme

1. `Features/` klasörü altında yeni klasör oluşturun
2. Endpoint sınıfını oluşturun
3. `Program.cs`'te endpoint'i map edin

### Test

```bash
# Unit testler (gelecekte eklenecek)
dotnet test

# Integration testler (gelecekte eklenecek)
dotnet test --filter Category=Integration
```

## Lisans

Bu proje staj projesi olarak geliştirilmiştir.
