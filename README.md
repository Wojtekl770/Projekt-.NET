# Aplikacja do Wynajmu Samochodów

## Opis
Aplikacja umożliwia wypożyczanie samochodów oraz porównywanie ofert z różnych serwisów wynajmu. Ułatwia znalezienie najlepszego pojazdu w korzystnej cenie, integrując oferty wielu firm w jednym miejscu. Dodatkowo, pozwala zarządzać rezerwacjami oraz zwrotami pojazdów w prosty i intuicyjny sposób.

Aplikacja do wynajmu samochodów składa się z dwóch głównych komponentów: aplikacji pośredniczącej oraz API do zarządzania wynajmami. Architektura systemu umożliwia łatwą skalowalność oraz asynchroniczne przetwarzanie danych w tle.

## Architektura i Infrastruktura
Infrastruktura aplikacji jest hostowana na platformie Microsoft Azure, co zapewnia łatwą konserwację, skalowalność i bezpieczeństwo.

---

## Komponenty

### Frontend: Blazor WebAssembly
Aplikacja dostępna w przeglądarce internetowej z dynamicznym interfejsem użytkownika.

**Technologia:** Blazor WebAssembly

**Cel:** Zapewnienie intuicyjnego UI dla klientów, umożliwiającego wyszukiwanie, filtrowanie i wynajmowanie samochodów.

**Kluczowe funkcje:**
- Interfejs logowania i rejestracji
- Wyszukiwanie i filtrowanie dostępnych samochodów
- Pulpit do zarządzania rezerwacjami

---

### Backend: ASP.NET Core API
Obsługuje logikę biznesową, przetwarzanie danych i integracje z usługami zewnętrznymi.

**Technologia:** ASP.NET Core API (.NET 8)

**Cel:** Dostarczanie RESTful API dla aplikacji frontendowej.

**Kluczowe funkcje:**
- Pobieranie dostępnych samochodów od dostawców przez integrację z API
- Wysyłanie zapytań o ceny do partnerów wynajmu
- Obsługa operacji wynajmu, zarządzanie rezerwacjami i zwrotami
- Przechowywanie plików (np. zdjęcia zwróconych pojazdów) na Azure Blob Storage
- Wysyłanie e-maili za pomocą SendGrid

---

### Baza danych: Azure SQL Database
Zarządza danymi strukturalnymi, zapewniając bezpieczeństwo i skalowalność.

**Technologia:** Azure SQL Database

**Cel:** Przechowywanie i zarządzanie danymi użytkowników, samochodów oraz transakcji wynajmu.

**Struktura danych:**
- Użytkownicy: dane klientów, informacje o prawie jazdy
- Samochody: marka, model, status dostępności
- Transakcje: informacje o rezerwacjach i wynajmach

---

### Autentykacja i Autoryzacja: OpenID Connect
Zarządzanie logowaniem i rejestracją użytkowników z wykorzystaniem zewnętrznych dostawców tożsamości.

**Technologia:** OpenID Connect / Azure AD B2C

**Cel:** Zapewnienie bezpiecznego logowania i ochrony danych użytkowników.

**Kluczowe funkcje:**
- Obsługa MFA (multi-factor authentication)
- Integracja z dostawcami tożsamości (Google, Microsoft, Facebook)
- Bezpieczne zarządzanie tożsamością użytkowników

---

## Wymagania
- .NET 8 SDK
- Blazor WebAssembly
- Microsoft Azure (Blob Storage, SQL Database, AD B2C)
- SendGrid API

## Uruchomienie
1. Klonowanie repozytorium
   ```sh
   git clone https://github.com/user/repo.git
   cd repo
   ```
2. Uruchomienie backendu
   ```sh
   cd backend
   dotnet run
   ```
3. Uruchomienie frontendu
   ```sh
   cd frontend
   dotnet run
   ```
