# MVCBitirmeProjesi

## Git Komutları

### git clone
Uzak bir depoyu (repository) yerel bilgisayarınıza kopyalamak için kullanılır.
```bash
git clone https://github.com/kullanici-adi/repo-adi.git
```

### git add
Değişiklikleri hazırlama alanına (staging area) eklemek için kullanılır.
```bash
# Tüm değişiklikleri eklemek için
git add .

# Belirli bir dosyayı eklemek için
git add dosya_adi.uzanti
```

### git commit
Hazırlama alanındaki değişiklikleri kaydetmek için kullanılır.
```bash
git commit -m "Değişiklik açıklaması"
```

### git push
Yerel depodaki commit'leri uzak depoya göndermek için kullanılır.
```bash
git push
```

### git fetch
Uzak depodaki değişiklikleri kontrol etmek için kullanılır. Değişiklikleri indirir ancak yerel dallarınızla birleştirmez.
```bash
git fetch
```
