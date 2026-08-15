-- Veritabanı ilk oluşturulurken bir kez çalışır.
-- Tablolar EF Core migration'ları ile oluşturulur; burada yalnızca şema ve eklentiler hazırlanır.

CREATE SCHEMA IF NOT EXISTS govai;

-- Türkçe metinlerde aksana duyarsız arama için (ör. "İSTİHDAM" ↔ "istihdam").
CREATE EXTENSION IF NOT EXISTS unaccent;

-- Çağrı başlıklarında benzerlik araması ve yazım hatası toleransı için.
CREATE EXTENSION IF NOT EXISTS pg_trgm;

COMMENT ON SCHEMA govai IS
    'GOVAI — kurumsal teşvik, hibe ve ihale uygunluk analizi platformu';
