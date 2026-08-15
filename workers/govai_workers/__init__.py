"""GOVAI worker servisleri.

Üç bağımsız worker:
    * collector — resmî kaynakları tarar, ham dokümanları API'ye bırakır
    * parser    — PDF/HTML metnini normalize eder, kural çıkarımını tetikler
    * scheduler — kaynakların cron takvimini işletir ve bildirim gönderimini tetikler

Hepsi GOVAI REST API üzerinden konuşur; veritabanına doğrudan erişmez.
Bu ayrım, iş kurallarının tek bir yerde (.NET Application katmanı) kalmasını sağlar.
"""

__version__ = "0.1.0"
