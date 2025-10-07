# Dokument wymagań produktu (PRD) - 10xWorkoutManager (MVP)

## 1. Przegląd produktu
10xWorkoutManager to aplikacja internetowa (MVP) zaprojektowana w celu rozwiązania problemu nieefektywnego zarządzania planami treningowymi i śledzenia postępów. Aplikacja umożliwia użytkownikom tworzenie, edytowanie i zarządzanie planami treningowymi, przeprowadzanie sesji treningowych oraz przeglądanie historii swoich wyników. Celem jest zastąpienie niewygodnych metod, takich jak notatniki papierowe czy arkusze kalkulacyjne, intuicyjnym i scentralizowanym narzędziem cyfrowym, które wspiera użytkownika w osiąganiu celów treningowych poprzez łatwy dostęp do danych o poprzednich treningach.

## 2. Problem użytkownika
Głównym problemem, który rozwiązuje aplikacja, jest nieefektywność i czasochłonność zarządzania treningami siłowymi przy użyciu metod analogowych (papier) lub niespecjalistycznych narzędzi cyfrowych (arkusze kalkulacyjne). Użytkownikom trudno jest śledzić swoje postępy, przeglądać wyniki z poprzednich sesji, pamiętać używane obciążenia czy modyfikować plany treningowe. Brak scentralizowanego i dedykowanego narzędzia utrudnia regularne monitorowanie progresu i podejmowanie świadomych decyzji dotyczących kolejnych treningów.

## 3. Wymagania funkcjonalne
### 3.1. Zarządzanie kontem użytkownika
-   Rejestracja nowego użytkownika za pomocą adresu e-mail i hasła.
-   Logowanie do systemu.
-   Funkcja resetowania zapomnianego hasła.
-   Możliwość usunięcia swojego konta i wszystkich powiązanych danych z poziomu ustawień.

### 3.2. Zarządzanie planami treningowymi
-   Tworzenie planu treningowego za pomocą kreatora krok-po-kroku (nazwa planu, dodawanie dni treningowych).
-   Przeglądanie listy wszystkich stworzonych planów treningowych.
-   Wyświetlanie szczegółów pojedynczego planu (dni i przypisane do nich ćwiczenia).
-   Edycja planu, w tym zmiana nazwy oraz zmiana kolejności dni treningowych za pomocą funkcji "przeciągnij i upuść".
-   Usuwanie planów treningowych.
-   Blokada edycji planu podczas trwania aktywnej sesji treningowej.

### 3.3. Zarządzanie ćwiczeniami
-   Dostęp do predefiniowanej bazy ćwiczeń skategoryzowanych według partii mięśniowych.
-   Wyszukiwanie ćwiczeń w bazie danych.
-   Możliwość dodawania własnych, niestandardowych ćwiczeń do bazy.
-   Dodawanie ćwiczeń z bazy do poszczególnych dni w planie treningowym.
-   Zmiana kolejności ćwiczeń w obrębie dnia treningowego za pomocą funkcji "przeciągnij i upuść".
-   Usuwanie ćwiczeń z dnia treningowego.

### 3.4. Przeprowadzanie treningu
-   Możliwość rozpoczęcia sesji treningowej poprzez wybranie planu i dnia treningowego.
-   Wyświetlanie ćwiczeń w ustalonej kolejności.
-   Przed rozpoczęciem każdego ćwiczenia, aplikacja wyświetla wyniki (ciężar, serie, powtórzenia) z ostatniego wykonania tego ćwiczenia.
-   Zapisywanie wyników dla każdej serii: ciężar (w kg), liczba powtórzeń.
-   Możliwość oznaczenia serii jako wykonanej "do upadku" za pomocą checkboxa.
-   Możliwość dodania niezaplanowanej serii do bieżącego ćwiczenia.
-   Możliwość dodania opcjonalnej notatki do ćwiczenia oraz do całej sesji treningowej.
-   Możliwość pominięcia ćwiczenia (zostanie to odnotowane w historii).
-   Nawigacja między ćwiczeniami za pomocą przycisku "Zapisz i przejdź dalej".

### 3.5. Historia treningów
-   Dostęp do prostej, chronologicznej listy wszystkich zapisanych sesji treningowych.
-   Możliwość wyświetlenia szczegółowego podsumowania każdej sesji, zawierającego wszystkie zapisane ćwiczenia, serie, powtórzenia, ciężary, notatki oraz informację o pominiętych ćwiczeniach.

## 4. Granice produktu
### Co wchodzi w zakres MVP:
-   System uwierzytelniania użytkowników (rejestracja/logowanie).
-   Pełen cykl zarządzania planami treningowymi (tworzenie, edycja, usuwanie).
-   Zarządzanie ćwiczeniami w planach.
-   Predefiniowana baza ćwiczeń z możliwością dodawania własnych.
-   Interaktywny moduł przeprowadzania i zapisywania sesji treningowej.
-   Wyświetlanie wyników z ostatniego wykonania danego ćwiczenia podczas treningu.
-   Prosta historia odbytych treningów.
-   Aplikacja wyłącznie w wersji webowej.
-   Użycie kilogramów (kg) jako jedynej jednostki wagi.

### Co NIE wchodzi w zakres MVP:
-   Aplikacje mobilne (iOS, Android).
-   Zaawansowana wizualizacja postępów (wykresy, statystyki).
-   Funkcje społecznościowe (udostępnianie planów, porównywanie wyników).
-   Integracje z innymi urządzeniami (np. smartwatche).
-   Dodawanie aktywności pobocznych (np. cardio, rozciąganie) do planu.
-   Gotowe, predefiniowane plany treningowe.
-   Wyświetlanie wyników z ostatnio wykonywanego całego planu (pokazywane są tylko wyniki dla konkretnego ćwiczenia).

## 5. Historyjki użytkowników

### Zarządzanie kontem i onboarding
---
-   ID: US-001
-   Tytuł: Rejestracja nowego użytkownika
-   Opis: Jako nowy użytkownik, chcę móc założyć konto w aplikacji używając mojego adresu e-mail i hasła, aby móc zapisywać swoje plany i treningi.
-   Kryteria akceptacji:
    -   Formularz rejestracji zawiera pola na adres e-mail, hasło i potwierdzenie hasła.
    -   System waliduje poprawność formatu adresu e-mail.
    -   System sprawdza, czy hasła w obu polach są identyczne.
    -   System sprawdza, czy podany e-mail nie jest już zarejestrowany.
    -   Po pomyślnej rejestracji użytkownik jest automatycznie zalogowany i przekierowany na ekran powitalny.

---
-   ID: US-002
-   Tytuł: Logowanie użytkownika
-   Opis: Jako zarejestrowany użytkownik, chcę móc zalogować się na swoje konto podając e-mail i hasło, aby uzyskać dostęp do moich danych.
-   Kryteria akceptacji:
    -   Formularz logowania zawiera pola na adres e-mail i hasło.
    -   Po podaniu prawidłowych danych użytkownik zostaje zalogowany i przekierowany do panelu głównego.
    -   W przypadku podania błędnych danych, wyświetlany jest stosowny komunikat.

---
-   ID: US-003
-   Tytuł: Resetowanie hasła
-   Opis: Jako zarejestrowany użytkownik, który zapomniał hasła, chcę mieć możliwość jego zresetowania, aby odzyskać dostęp do konta.
-   Kryteria akceptacji:
    -   Na stronie logowania znajduje się link "Zapomniałem hasła".
    -   Po kliknięciu linku użytkownik jest proszony o podanie swojego adresu e-mail.
    -   Jeśli e-mail istnieje w bazie, na podany adres wysyłana jest wiadomość z linkiem do resetu hasła.
    -   Link jest unikalny i ma ograniczony czas ważności.
    -   Po przejściu pod link, użytkownik może ustawić nowe hasło.

---
-   ID: US-004
-   Tytuł: Usuwanie konta
-   Opis: Jako użytkownik, chcę mieć możliwość trwałego usunięcia mojego konta i wszystkich moich danych z poziomu ustawień aplikacji.
-   Kryteria akceptacji:
    -   W ustawieniach konta znajduje się opcja "Usuń konto".
    -   Przed usunięciem konta wyświetlane jest okno modalne z prośbą o potwierdzenie operacji (np. poprzez wpisanie hasła).
    -   Po potwierdzeniu, wszystkie dane użytkownika (konto, plany, historia treningów) są trwale usuwane z bazy danych.

---
-   ID: US-005
-   Tytuł: Ekran powitalny dla nowych użytkowników
-   Opis: Jako nowy, zalogowany użytkownik, chcę zobaczyć ekran powitalny, który pokieruje mnie do stworzenia mojego pierwszego planu treningowego.
-   Kryteria akceptacji:
    -   Ekran jest wyświetlany tylko po pierwszym zalogowaniu po rejestracji.
    -   Ekran zawiera wyraźny przycisk/wezwanie do działania (CTA), np. "Stwórz swój pierwszy plan treningowy".
    -   Kliknięcie przycisku przenosi użytkownika do kreatora planu treningowego.

### Zarządzanie planami treningowymi
---
-   ID: US-006
-   Tytuł: Tworzenie nowego planu treningowego
-   Opis: Jako użytkownik, chcę stworzyć nowy plan treningowy za pomocą kreatora, aby zorganizować swoje ćwiczenia.
-   Kryteria akceptacji:
    -   Kreator działa krok po kroku.
    -   Krok 1: Użytkownik podaje nazwę planu (np. "Mój plan siłowy").
    -   Krok 2: Użytkownik dodaje dni treningowe, nadając im nazwy (np. "Dzień A", "Dzień B").
    -   Po zakończeniu pracy kreatora, pusty plan jest zapisywany i użytkownik jest przekierowywany do widoku edycji tego planu, aby dodać ćwiczenia.

---
-   ID: US-007
-   Tytuł: Przeglądanie listy planów
-   Opis: Jako użytkownik, chcę widzieć listę wszystkich moich planów treningowych, aby móc szybko wybrać jeden z nich.
-   Kryteria akceptacji:
    -   Na głównej stronie aplikacji wyświetlana jest lista wszystkich planów użytkownika.
    -   Każdy element listy pokazuje nazwę planu.
    -   Kliknięcie na element listy przenosi do widoku szczegółów danego planu.

---
-   ID: US-008
-   Tytuł: Wyświetlanie szczegółów planu
-   Opis: Jako użytkownik, chcę móc zobaczyć szczegóły wybranego planu, w tym wszystkie dni treningowe i przypisane do nich ćwiczenia.
-   Kryteria akceptacji:
    -   Widok szczegółów planu wyświetla jego nazwę.
    -   Poniżej nazwy znajduje się lista dni treningowych (np. "Dzień A", "Dzień B").
    -   Pod nazwą każdego dnia treningowego wyświetlana jest lista przypisanych do niego ćwiczeń w odpowiedniej kolejności.

---
-   ID: US-009
-   Tytuł: Edycja planu treningowego
-   Opis: Jako użytkownik, chcę mieć możliwość edycji mojego planu, w tym zmiany jego nazwy i kolejności dni treningowych.
-   Kryteria akceptacji:
    -   W widoku edycji planu można zmienić jego nazwę.
    -   Użytkownik może zmieniać kolejność dni treningowych za pomocą funkcji "przeciągnij i upuść".
    -   Zmiany są zapisywane automatycznie lub po kliknięciu przycisku "Zapisz".
    -   Edycja planu jest niemożliwa, jeśli trwa sesja treningowa oparta na tym planie.

---
-   ID: US-010
-   Tytuł: Usuwanie planu treningowego
-   Opis: Jako użytkownik, chcę móc usunąć plan treningowy, którego już nie potrzebuję.
-   Kryteria akceptacji:
    -   W widoku listy planów lub w widoku edycji planu znajduje się opcja "Usuń".
    -   Przed usunięciem wyświetlane jest okno z prośbą o potwierdzenie.
    -   Po potwierdzeniu plan jest trwale usuwany.

### Zarządzanie ćwiczeniami w planie
---
-   ID: US-011
-   Tytuł: Dodawanie ćwiczeń do dnia treningowego
-   Opis: Jako użytkownik, chcę dodawać ćwiczenia z bazy do konkretnego dnia w moim planie treningowym.
-   Kryteria akceptacji:
    -   W widoku edycji planu, przy każdym dniu treningowym jest przycisk "Dodaj ćwiczenie".
    -   Po kliknięciu przycisku otwiera się widok/modal z listą dostępnych ćwiczeń.
    -   Użytkownik może wybrać jedno lub więcej ćwiczeń do dodania.
    -   Wybrane ćwiczenia pojawiają się na liście ćwiczeń danego dnia.

---
-   ID: US-012
-   Tytuł: Wyszukiwanie i filtrowanie ćwiczeń
-   Opis: Jako użytkownik, podczas dodawania ćwiczeń do planu, chcę móc je wyszukiwać po nazwie i filtrować po kategorii (partii mięśniowej), aby szybciej znaleźć to, czego szukam.
-   Kryteria akceptacji:
    -   W oknie dodawania ćwiczeń znajduje się pole wyszukiwania tekstowego.
    -   Wpisywanie tekstu w pole filtruje listę ćwiczeń w czasie rzeczywistym.
    -   Dostępne są filtry kategorii (np. "Klatka piersiowa", "Nogi", "Plecy").
    -   Zaznaczenie filtra ogranicza listę ćwiczeń do danej kategorii.

---
-   ID: US-013
-   Tytuł: Dodawanie własnego ćwiczenia do bazy
-   Opis: Jako użytkownik, jeśli nie mogę znaleźć ćwiczenia w bazie, chcę mieć możliwość dodania własnego.
-   Kryteria akceptacji:
    -   W oknie dodawania ćwiczeń jest opcja "Stwórz nowe ćwiczenie".
    -   Formularz tworzenia ćwiczenia zawiera pola na nazwę i kategorię (partię mięśniową).
    -   Nowo utworzone ćwiczenie jest zapisywane w ogólnej bazie ćwiczeń i dostępne do wyboru w przyszłości.

---
-   ID: US-014
-   Tytuł: Zmiana kolejności ćwiczeń w dniu treningowym
-   Opis: Jako użytkownik, chcę mieć możliwość zmiany kolejności ćwiczeń w ramach jednego dnia treningowego, aby dopasować trening do swoich preferencji.
-   Kryteria akceptacji:
    -   W widoku edycji planu, na liście ćwiczeń danego dnia, można zmieniać ich kolejność.
    -   Zmiana kolejności odbywa się za pomocą funkcji "przeciągnij i upuść".
    -   Nowa kolejność jest zapisywana.

---
-   ID: US-015
-   Tytuł: Usuwanie ćwiczenia z dnia treningowego
-   Opis: Jako użytkownik, chcę móc usunąć ćwiczenie z dnia treningowego w moim planie.
-   Kryteria akceptacji:
    -   Przy każdym ćwiczeniu na liście w trybie edycji planu znajduje się przycisk "Usuń".
    -   Po kliknięciu przycisku ćwiczenie jest usuwane z listy dla danego dnia.
    -   Usunięcie ćwiczenia z planu nie usuwa go z ogólnej bazy ćwiczeń.

### Przeprowadzanie treningu
---
-   ID: US-016
-   Tytuł: Rozpoczynanie sesji treningowej
-   Opis: Jako użytkownik, chcę rozpocząć trening wybierając plan i konkretny dzień treningowy, aby przejść do widoku aktywnej sesji.
-   Kryteria akceptacji:
    -   W widoku szczegółów planu przy każdym dniu treningowym znajduje się przycisk "Rozpocznij trening".
    -   Kliknięcie przycisku rozpoczyna sesję i przenosi użytkownika do widoku pierwszego ćwiczenia z listy dla tego dnia.

---
-   ID: US-017
-   Tytuł: Wyświetlanie poprzednich wyników ćwiczenia
-   Opis: Jako użytkownik, podczas treningu, przed rozpoczęciem ćwiczenia, chcę widzieć swoje wyniki (ciężar, powtórzenia, serie) z ostatniego razu, gdy je wykonywałem, aby wspierać progressive overload.
-   Kryteria akceptacji:
    -   W widoku aktywnego ćwiczenia, w widocznym miejscu, wyświetlana jest informacja o wynikach z ostatniej sesji, w której to ćwiczenie było wykonywane.
    -   Informacja zawiera co najmniej ciężar i liczbę powtórzeń dla każdej serii.
    -   Jeśli ćwiczenie jest wykonywane po raz pierwszy, wyświetlany jest odpowiedni komunikat.

---
-   ID: US-018
-   Tytuł: Zapisywanie wyników serii
-   Opis: Jako użytkownik, w trakcie treningu, chcę móc zapisać dla każdej wykonanej serii liczbę powtórzeń i użyty ciężar (w kg).
-   Kryteria akceptacji:
    -   Interfejs pozwala na dodawanie kolejnych serii do bieżącego ćwiczenia.
    -   Dla każdej serii można wprowadzić dane w pola "ciężar (kg)" i "powtórzenia".
    -   Domyślnie wyświetlana jest jedna pusta seria gotowa do wypełnienia.
    -   Przycisk "Dodaj serię" dodaje kolejny wiersz do zapisu.

---
-   ID: US-019
-   Tytuł: Oznaczanie serii "do upadku"
-   Opis: Jako użytkownik, przy zapisie serii, chcę mieć możliwość oznaczenia, że została ona wykonana do upadku mięśniowego, aby mieć tę informację w historii.
-   Kryteria akceptacji:
    -   Przy każdym wierszu serii znajduje się opcjonalny checkbox z etykietą "Do upadku".
    -   Zaznaczenie checkboxa jest zapisywane wraz z resztą danych serii.
    -   Informacja ta jest widoczna w podsumowaniu treningu w historii.

---
-   ID: US-020
-   Tytuł: Nawigacja między ćwiczeniami i zapis treningu
-   Opis: Jako użytkownik, po zakończeniu ćwiczenia, chcę kliknąć przycisk "Zapisz i przejdź dalej", aby zapisać wyniki i przejść do następnego ćwiczenia z planu.
-   Kryteria akceptacji:
    -   Na dole widoku aktywnego ćwiczenia znajduje się przycisk "Zapisz i przejdź dalej".
    -   Kliknięcie przycisku zapisuje wszystkie wprowadzone dane dla bieżącego ćwiczenia.
    -   Użytkownik jest automatycznie przenoszony do widoku następnego ćwiczenia.
    -   Jeśli to było ostatnie ćwiczenie, kliknięcie przycisku (zmienionego na "Zakończ trening") kończy sesję i przenosi do podsumowania.

---
-   ID: US-021
-   Tytuł: Pomijanie ćwiczenia
-   Opis: Jako użytkownik, w trakcie treningu, chcę mieć możliwość pominięcia ćwiczenia, jeśli nie mogę lub nie chcę go wykonać.
-   Kryteria akceptacji:
    -   W widoku aktywnego ćwiczenia znajduje się przycisk lub link "Pomiń ćwiczenie".
    -   Po kliknięciu, użytkownik jest przenoszony do następnego ćwiczenia z planu.
    -   W historii treningu pominięte ćwiczenie jest oznaczone jako "pominięte".

---
-   ID: US-022
-   Tytuł: Dodawanie notatek do ćwiczenia i sesji
-   Opis: Jako użytkownik, chcę mieć możliwość dodania opcjonalnych notatek do konkretnego ćwiczenia lub do całej sesji treningowej, aby zapisać dodatkowe informacje.
-   Kryteria akceptacji:
    -   W widoku aktywnego ćwiczenia znajduje się pole tekstowe na notatki do tego ćwiczenia.
    -   W głównym widoku sesji treningowej (lub w podsumowaniu) znajduje się pole na notatki do całej sesji.
    -   Notatki są zapisywane i widoczne w historii treningu.

### Historia treningów
---
-   ID: US-023
-   Tytuł: Przeglądanie historii treningów
-   Opis: Jako użytkownik, chcę mieć dostęp do chronologicznej listy moich odbytych treningów, aby móc śledzić swoją aktywność.
-   Kryteria akceptacji:
    -   W aplikacji jest sekcja "Historia".
    -   Wyświetla ona listę sesji treningowych w porządku od najnowszej do najstarszej.
    -   Każdy element listy zawiera co najmniej datę treningu i nazwę wykonanego planu/dnia.

---
-   ID: US-024
-   Tytuł: Wyświetlanie szczegółów historycznego treningu
-   Opis: Jako użytkownik, chcę móc kliknąć na trening w historii, aby zobaczyć jego szczegółowe podsumowanie.
-   Kryteria akceptacji:
    -   Po kliknięciu na sesję w historii, wyświetlane jest jej pełne podsumowanie.
    -   Podsumowanie zawiera: datę, nazwę planu/dnia, notatki do całej sesji.
    -   Dla każdego wykonanego ćwiczenia widoczne są: zapisane serie (ciężar, powtórzenia), oznaczenia "do upadku" oraz notatki do ćwiczenia.
    -   Ćwiczenia pominięte są wyraźnie oznaczone.

## 6. Metryki sukcesu
Głównym celem MVP jest zbudowanie bazy zaangażowanych użytkowników, którzy regularnie korzystają z kluczowej funkcjonalności aplikacji.

-   Główne kryterium sukcesu: Aktywne zaangażowanie użytkowników w podstawową pętlę działania aplikacji (planowanie -> trenowanie -> analiza).
-   Kluczowy wskaźnik wydajności (KPI): Średnia liczba zapisanych (odbytych) sesji treningowych na aktywnego użytkownika miesięcznie.
-   Cel dla KPI (na 3 miesiące po starcie): Osiągnięcie średniej co najmniej 4 zapisanych treningów na aktywnego użytkownika miesięcznie, co wskazywałoby na regularne korzystanie z aplikacji przynajmniej raz w tygodniu.
-   Dodatkowe metryki do monitorowania:
    -   Współczynnik retencji użytkowników (procent użytkowników wracających do aplikacji tydzień po tygodniu).
    -   Liczba stworzonych planów treningowych na użytkownika.
    -   Współczynnik ukończenia sesji (procent rozpoczętych sesji, które zostały pomyślnie zakończone i zapisane).
