# SafeCity — Community Safety App

A .NET MAUI Android application where citizens report and view local incidents (crime, fire, accidents, road hazards, missing persons, "good vibes") in real time. Inspired by the Citizen app.

---

## Build & Run (Fedora Linux)

### Prerequisites

```bash
# .NET SDK (10.x)
dotnet --info

# Confirm MAUI Android workload
dotnet workload list   # should show maui-android

# Java 17 JDK — already at ~/android-dev/jdk-17.0.13+11
# Android SDK — already at ~/Android/Sdk (platform 36 installed)
```

### Build

```bash
JAVA_HOME=~/android-dev/jdk-17.0.13+11 \
dotnet build -f net10.0-android \
  -p:JavaSdkDirectory=~/android-dev/jdk-17.0.13+11 \
  -p:AndroidSdkDirectory=~/Android/Sdk
```

### Run on a connected device / emulator

```bash
JAVA_HOME=~/android-dev/jdk-17.0.13+11 \
dotnet run -f net10.0-android \
  -p:JavaSdkDirectory=~/android-dev/jdk-17.0.13+11 \
  -p:AndroidSdkDirectory=~/Android/Sdk
```

### Environment variables

Copy `.env.example` → `.env` and add your Gemini key:

```
GEMINI_API_KEY=your_key_here
```

The `.env` file is excluded from git (see `.gitignore`). On Android, copy the `.env` file to `FileSystem.AppDataDirectory` on first run (or handle via the Helpers/EnvHelper seeding path).

---

## 12 GoF Design Patterns

### Creational (4)

| # | Pattern | File | Justification |
|---|---------|------|---------------|
| 1 | **Singleton** | `Patterns/Creational/Singleton/SessionManager.cs` | Guarantees one shared source of truth for the active user, auth token, and last-known location across all services. Registered as `AddSingleton<SessionManager>()` in DI. |
| 2 | **Factory Method** | `Patterns/Creational/FactoryMethod/IncidentFactory.cs` | Maps `IncidentType` → correct subclass (`CrimeIncident`, `FireIncident`, …) with default icon, color, and severity. Callers never `new` a subtype directly; adding a new type is a new class + one `switch` arm (OCP). |
| 3 | **Abstract Factory** | `Patterns/Creational/AbstractFactory/CriticalAlertFactory.cs` | Produces a coherent family of alert-presentation objects (icon glyph + color + sound key + vibration flag). `CriticalAlertFactory` vs `InfoAlertFactory` swap the whole family atomically. |
| 4 | **Builder** | `Patterns/Creational/Builder/ReportBuilder.cs` | Assembles a `Report` across multiple UI steps (location → category → severity → description → media → anonymity). `Build()` validates completeness before returning; prevents half-initialised objects. |

### Structural (4)

| # | Pattern | File | Justification |
|---|---------|------|---------------|
| 5 | **Facade** | `Patterns/Structural/Facade/ReportingFacade.cs` | Single `SubmitAsync()` entry point for the Report ViewModel. Internally orchestrates ReportBuilder → ReportRepository → IncidentFactory → IncidentRepository → IncidentAlertPublisher without exposing those collaborators. |
| 6 | **Adapter** | `Patterns/Structural/Adapter/GeminiAssistantAdapter.cs` | Wraps the Gemini REST API's nested JSON response shape and adapts it to the internal `IAssistant.AskAsync(string)` contract. Swapping to a different LLM requires only a new adapter. |
| 7 | **Decorator** | `Patterns/Structural/Decorator/PriorityNotificationDecorator.cs` | Wraps `IAppNotification` to layer priority/sound/vibration behaviours at runtime without subclass explosion. Stack: `BaseNotification → VibrationDecorator → SoundDecorator → PriorityDecorator`. |
| 8 | **Proxy** | `Patterns/Structural/Proxy/CachedIncidentMediaProxy.cs` | Same `IMediaLoader` interface as the real HTTP loader; intercepts `LoadAsync()` to return a cached `MemoryStream` on hit, or delegates and caches on miss. Eliminates redundant network requests in the feed. |

### Behavioral (4)

| # | Pattern | File | Justification |
|---|---------|------|---------------|
| 9 | **Observer** | `Patterns/Behavioral/Observer/IncidentAlertPublisher.cs` | `ReportingFacade` calls `Publish(incident)` after every submission. Map VM, Alerts VM, and News VM all implement `IIncidentObserver` and update their UI independently without being coupled to each other or the facade. |
| 10 | **Strategy** | `Patterns/Behavioral/Strategy/By*.cs` | Four interchangeable feed-sort algorithms (distance, recency, severity, popularity). The user switches at runtime via a `Picker`; the News VM delegates to whatever `IFeedSortStrategy` is selected. |
| 11 | **State** | `Patterns/Behavioral/State/ReportedState.cs` | Incident lifecycle: `Reported → Verified → Resolved → Archived` (+ `Disputed`). Allowed transitions live inside each state object. Calling an illegal transition (e.g. `Resolve()` on `Reported`) throws immediately, centralising invariant enforcement. |
| 12 | **Command** | `Patterns/Behavioral/Command/ReportIncidentCommand.cs` | User actions (report, upvote, share, join network) are encapsulated as `IUserCommand` objects with `Execute()/Undo()`. `CommandHistory` enables undo of the last action and decouples UI events from business logic. |

---

## SOLID Principles

| Principle | How it is applied |
|-----------|-------------------|
| **S** — Single Responsibility | `ReportingFacade` orchestrates; repositories only persist; factories only create; each ViewModel manages one screen. |
| **O** — Open / Closed | New incident types → new subclass + one factory `switch` arm, no existing classes modified. New sort orders → new `IFeedSortStrategy` implementation. |
| **L** — Liskov Substitution | Every `Incident` subtype is substitutable. Every `IFeedSortStrategy` implementation is substitutable. |
| **I** — Interface Segregation | Small focused interfaces: `IIncidentObserver` (1 method), `IFeedSortStrategy` (2 methods), `IAssistant` (2 properties + 1 method), `IMediaLoader` (2 methods). |
| **D** — Dependency Inversion | All ViewModels depend on interfaces, never concretes. The DI container in `MauiProgram.cs` owns the binding of abstractions to implementations. |

---

## Screens

| Screen | Route | Key pattern usage |
|--------|-------|-------------------|
| Onboarding | `OnboardingPage` | Entry point |
| Login | `LoginPage` | `IAuthService` (stubbed) |
| Create Profile | `CreateProfilePage` | Live username availability check |
| Location Permission | `LocationPermissionPage` | `ILocationService` |
| News Feed | `NewsPage` | **Strategy** (sort), **Observer** (live updates), **Command** (upvote/share) |
| Map / Home | `HomePage` | **Observer** (marker updates), **Factory** (hydrate markers) |
| Alerts | `AlertsPage` | **Observer** + **Abstract Factory** + **Decorator** (notification stack) |
| Profile | `ProfilePage` | **Command** (join network), **Singleton** (session) |
| Report / Go Live | `ReportPage` | **Builder** + **Facade** + **Command** |
| SafeCity Assistant | `AssistantPage` | **Adapter** (Gemini) |

---

## Backend (optional)

```
backend/
├── app.py          # Flask — /health  /incidents  /assistant
├── requirements.txt
└── .env.example
```

**Render.com start command:** `gunicorn app:app --timeout 120`

Warm up the /health endpoint before a demo to avoid cold-start latency.

---

## Project Structure

```
SafeCity/
├── Models/         Domain entities (Incident + 6 subtypes, Report, User, AlertZone)
├── Enums/          IncidentType, IncidentSeverity, IncidentStateEnum
├── Data/           SQLite repos + interfaces
├── Services/       ILocationService, IMediaService, IAuthService, IAppNotification
├── Patterns/       12 GoF implementations (see table above)
├── ViewModels/     One VM per screen (MVVM, CommunityToolkit.Mvvm)
├── Views/          XAML pages
├── Controls/       Reusable UI components
├── Converters/     XAML value converters
├── Helpers/        EnvHelper (.env loader)
├── AppConfig.cs    Base URL + feature flags
└── MauiProgram.cs  DI registration hub
```
