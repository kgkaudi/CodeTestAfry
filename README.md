![here we are](https://media.giphy.com/media/FnGJfc18tDDHy/giphy.gif)

From the movie Hackers 😄

# Toll Fee Calculator

A C# / ASP.NET Core solution that calculates vehicle toll fees for a city, rebuilt from the original "production-ready" code base. It has a domain library, a small controller-based HTTP API, a unit-test suite and a Postman collection for end-to-end checks.

> **Scope: 2013 only.** Toll-free days are defined for 2013, as in the original code. Passages in any other year are rejected instead of silently being charged as ordinary days (see [Assumptions](#assumptions-and-design-decisions)).

## Contents

- [Quick start](#quick-start)
- [Requirements and how they are met](#requirements-and-how-they-are-met)
- [Fee rules](#fee-rules)
- [Project structure](#project-structure)
- [Getting started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Build](#build)
  - [Run the unit tests](#run-the-unit-tests)
  - [Run the API](#run-the-api)
  - [Test the API with Postman](#test-the-api-with-postman)
- [API](#api)
- [Assumptions and design decisions](#assumptions-and-design-decisions)
- [What was wrong with the original code](#what-was-wrong-with-the-original-code)
- [Limitations and possible next steps](#limitations-and-possible-next-steps)
- [Original assignment](#original-assignment)

## Quick start

```bash
cd TollFeeCalculator                              # the folder containing src/ and tests/
dotnet clean                                      # clean project
dotnet build                                      # build project
dotnet test                                       # unit tests
dotnet run --project src/TollFeeCalculator.Api    # start the API (leave running)
```

Then import `postman/TollFeeCalculator.postman_collection.json` into Postman and run the collection (details [below](#test-the-api-with-postman)).

## Requirements and how they are met

| Requirement | Implementation |
|---|---|
| Fees between 8 and 18 SEK depending on time of day | `FeeSchedule.Default`, a table of time bands |
| Rush hour has the highest fee | 18 SEK at 07:00-07:59 and 15:30-16:59 |
| Maximum fee per day is 60 SEK | `TollCalculator.MaxDailyFee`, applied per calendar day |
| Charged once per hour, highest fee applies | 60-minute charge windows, the highest fee in each window is charged |
| Some vehicle types are fee-free | `VehicleType.IsTollFree()` |
| Weekends and holidays are fee-free | `Holiday2013Provider` behind `IHolidayProvider` |

## Fee rules

### Fee schedule

| Time of day | Fee (SEK) |
|---|---|
| 06:00-06:29 | 8 |
| 06:30-06:59 | 13 |
| 07:00-07:59 | 18 |
| 08:00-08:29 | 13 |
| 08:30-14:59 | 8 |
| 15:00-15:29 | 13 |
| 15:30-16:59 | 18 |
| 17:00-17:59 | 13 |
| 18:00-18:29 | 8 |
| 18:30-05:59 | 0 |

### Charge window (once per hour)

A window opens at the first paid passage and lasts 60 minutes, end exclusive. Only the highest fee among the passages in the window is charged. The next paid passage after the window opens a new one.

Example (all passages on the same working day):

| Passage | Fee | Window |
|---|---|---|
| 06:00 | 8 | window 1 opens (06:00 to 06:59) |
| 06:45 | 13 | window 1 |
| 07:05 | 18 | window 2 opens (65 minutes after 06:00) |

Charged: 13 (highest in window 1) + 18 = **31 SEK**.

Passages with a fee of 0 (for example 05:50) never open a window.

### Daily cap

The charged windows for a calendar day are summed and capped at **60 SEK**. The cap applies per day, so passages spanning several days are summed after each day is capped.

### Fee-free vehicles

`Motorbike`, `Tractor`, `Emergency`, `Diplomat`, `Foreign`, `Military`. Only `Car` is charged.

### Fee-free days (2013)

- Saturdays and Sundays
- All of July
- These dates, which include the days before holidays: 1 Jan, 28-29 Mar, 1 Apr, 30 Apr, 1 May, 8-9 May, 5-6 Jun, 21 Jun, 1 Nov, 24-26 Dec, 31 Dec

## Project structure

```
<repository root>/
├── README.md
├── LICENSE
├── .gitignore
└── TollFeeCalculator/                     Solution folder: run dotnet commands from here
    ├── TollFeeCalculator.slnx
    ├── postman/
    │   └── TollFeeCalculator.postman_collection.json
    ├── src/
    │   ├── TollFeeCalculator.Core/            Domain logic, no ASP.NET dependency
    │   │   ├── TollCalculator.cs              Windowing, daily cap, per-day grouping
    │   │   ├── FeeSchedule.cs                 Time bands and fees
    │   │   ├── VehicleType.cs                 Vehicle enum and fee-free check
    │   │   ├── IHolidayProvider.cs            Abstraction for fee-free days
    │   │   ├── Holiday2013Provider.cs         2013 implementation
    │   │   └── TollResult.cs                  Result records (total + per-day breakdown)
    │   └── TollFeeCalculator.Api/             ASP.NET Core controller-based API
    │       ├── Program.cs                     Host setup and DI registrations only
    │       ├── Controllers/
    │       │   └── TollController.cs          POST /api/toll/calculate
    │       ├── Models/
    │       │   └── CalculateRequest.cs         Request body
    │       └── Validation/
    │           └── VehicleTypeParser.cs        Parses and validates vehicleType
    └── tests/
        └── TollFeeCalculator.Tests/        xUnit tests
            ├── FeeScheduleTests.cs
            ├── HolidayProviderTests.cs
            ├── TollCalculatorTests.cs
            ├── VehicleTypeParserTests.cs
            └── TollControllerTests.cs
```

Keeping the logic in `Core` makes it testable without starting a web server, and lets other front ends (a console app, a worker) reuse it. The API layer stays thin: `Program.cs` only wires up services, and request handling, the request model, and vehicle-type parsing each live in their own file.

## Getting started

All commands below are run from the **solution folder** (`TollFeeCalculator/`, the one containing `src/`, `tests/` and the `.slnx` file), not from the repository root and not from inside `tests/`.

```bash
cd TollFeeCalculator
```

### Prerequisites

- The **.NET SDK** matching the `TargetFramework` in the `.csproj` files. Check what you have:

  ```bash
  dotnet --list-sdks
  dotnet --list-runtimes
  ```

  If the projects target a version you don't have installed, either install that SDK or change `<TargetFramework>` in all three `.csproj` files to a version you do have, for example:

  ```bash
  sed -i 's/net8.0/net10.0/' src/*/*.csproj tests/*/*.csproj
  ```

  All three `.csproj` files must target the **same** framework version — if only one is changed, `dotnet build`/`dotnet test` fails with an `NU1201` "is not compatible with" error.

  If a test run fails with "You must install or update .NET to run this application", the runtime for the target framework is missing; use one of the two options above.

- The `.slnx` solution format needs .NET SDK 9.0.200 or newer. On an older SDK, use the per-project commands shown below (they don't need a solution file).

- **Postman** (desktop app or web) for the API checks. Optional: Node.js, if you prefer the command-line runner (Newman).

### Build

```bash
dotnet build
```

### Run the unit tests

```bash
dotnet test
```

Useful variations:

```bash
# Show every test name and result
dotnet test --logger "console;verbosity=normal"

# Run one test class only
dotnet test --filter "FullyQualifiedName~TollCalculatorTests"

# Without the solution file (works on any SDK)
dotnet test tests/TollFeeCalculator.Tests
```

What the tests cover:

| File | Covers |
|---|---|
| `FeeScheduleTests` | Every fee band boundary (e.g. 06:29 gives 8, 06:30 gives 13), including regression cases for the original 09:00-14:29 bug |
| `HolidayProviderTests` | Every 2013 toll-free date, ordinary working days, and rejection of other years |
| `TollCalculatorTests` | Once-per-hour rule, window anchoring, daily cap, exempt vehicles, weekends, July, multiple days, unsorted input, unsupported years |
| `VehicleTypeParserTests` | Valid names (case-insensitive) and numeric values, unknown names, out-of-range numbers, `null`, booleans, objects, arrays, a missing field |
| `TollControllerTests` | The controller action directly: a valid request returns 200 with the right fee; an unknown vehicle type, empty/`null` passages, a passage outside 2013, and both fields invalid at once each return 400 with the expected error key(s) |

All tests should pass.

### Run the API

```bash
dotnet run --project src/TollFeeCalculator.Api
```

The URL it listens on is printed at startup (typically `http://localhost:5000`). Leave it running and, in another terminal, try a request:

```bash
curl -X POST http://localhost:5000/api/toll/calculate \
  -H "Content-Type: application/json" \
  -d '{
        "vehicleType": "Car",
        "passages": [
          "2013-03-11T06:00:00",
          "2013-03-11T06:45:00",
          "2013-03-11T07:05:00"
        ]
      }'
```

Expected response: `{"totalFee":31,"days":[{"date":"2013-03-11","passages":3,"fee":31,"cappedAtMax":false}]}`

Use the port shown in the startup output if it differs. Stop the API with `Ctrl+C`.

### Test the API with Postman

The collection in `postman/TollFeeCalculator.postman_collection.json` contains **85 requests**. Each one has test scripts that assert the expected status code and fee, so a full run tells you at a glance whether the API behaves correctly.

**Steps**

1. Start the API (see [Run the API](#run-the-api)) and note the URL it prints.
2. In Postman, choose **Import** and select `postman/TollFeeCalculator.postman_collection.json`.
3. Open the collection, go to the **Variables** tab and check that `baseUrl` matches the URL printed by the API (default `http://localhost:5000`). Save if you changed it.
4. Right-click the collection (or use the **Run** button) and choose **Run collection**, then **Run**. All tests should be green.

You can also send any single request on its own and inspect the response.

**Command line alternative (Newman)**

```bash
npx newman run postman/TollFeeCalculator.postman_collection.json --env-var "baseUrl=http://localhost:5000"
```

**What the collection covers**

| Folder | Requests | What it checks |
|---|---|---|
| 1. Fee bands | 21 | Every time boundary, including 09:15 and 12:10, which the original code got wrong |
| 2. Once-per-hour rule | 5 | Highest fee wins, window anchored on the first passage (31 SEK), exactly 60 minutes starts a new window (26 SEK), a free early passage doesn't swallow a paid one |
| 3. Daily cap | 2 | 70 SEK is capped to 60 with `cappedAtMax: true`; 57 SEK is left uncapped |
| 4. Vehicle types | 7 | `Car` is charged, the six exempt types are 0 |
| 5. Weekends and holidays | 23 | Saturday, Sunday, July, every 2013 fee-free date, and ordinary days that must be charged |
| 6. Multiple days and ordering | 4 | Days are summed, the cap applies per day, unsorted input gives the same result |
| 7. Validation errors | 13 | 400 for empty or missing `passages`, passages outside 2013, unknown/missing/invalid `vehicleType`, bad dates and malformed JSON |

**Troubleshooting**

| Symptom | Likely cause |
|---|---|
| "Could not get response" / connection refused | The API isn't running, or `baseUrl` has the wrong port |
| Every test fails with 404 | `baseUrl` is missing the correct host, or the path was edited |
| A HTTPS URL fails with a certificate error | Use the `http://` URL printed at startup, or trust the dev certificate with `dotnet dev-certs https --trust` |

## API

### `POST /api/toll/calculate`

Handled by `TollController.Calculate` (`Controllers/TollController.cs`).

**Request body**

| Field | Type | Notes |
|---|---|---|
| `vehicleType` | string (or number) | Required. One of `Car`, `Motorbike`, `Tractor`, `Emergency`, `Diplomat`, `Foreign`, `Military` (case-insensitive). The enum's numeric values are also accepted. |
| `passages` | array of ISO 8601 date-times | Required, at least one. All must be in 2013. Any order, any number of days. |

**Response `200 OK`**

```json
{
  "totalFee": 31,
  "days": [
    {
      "date": "2013-03-11",
      "passages": 3,
      "fee": 31,
      "cappedAtMax": false
    }
  ]
}
```

| Field | Meaning |
|---|---|
| `totalFee` | Sum of the daily fees, in SEK |
| `days[].date` | Calendar day |
| `days[].passages` | Number of passages received for that day |
| `days[].fee` | Fee charged that day, after the 60 SEK cap |
| `days[].cappedAtMax` | `true` if the uncapped total exceeded 60 SEK |

**Response `400 Bad Request`** (validation problem details). For example, a passage outside 2013:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "passages": ["Only passages in 2013 are supported."]
  }
}
```

An invalid vehicle type (unknown name such as `"Bicycle"`, a number that isn't a vehicle, `null`, or a missing field):

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "vehicleType": ["Must be one of: Car, Motorbike, Tractor, Emergency, Diplomat, Foreign, Military."]
  }
}
```

An empty or missing `passages` list returns an error under the `passages` key. Both fields can be invalid at once, in which case both error keys are returned together. An unparseable date-time or malformed JSON also returns `400`, but with no response body.

## Assumptions and design decisions

- **Fixed 60-minute window, end exclusive.** A window starts at the first paid passage; a passage exactly 60 minutes later is charged again. The original code used `<= 60`. This is a one-line change (`TollCalculator.ChargeWindow` comparison) if the business rule turns out to be inclusive.
- **Fixed window, not sliding.** Windows are anchored on the first paid passage, not extended by later passages.
- **Scope is 2013.** Supporting other years means providing a different `IHolidayProvider`. Nothing else has to change.
- **Fee bands** follow the standard Swedish congestion-tax schedule and match the intent of the original code. They live in `FeeSchedule.Default` and can be moved into configuration.
- **Local time.** Timestamps are treated as city-local time and `DateTime.Kind` is ignored.
- **Enum instead of strings** for vehicle types in the domain, so a typo can't compile. At the API boundary, `VehicleTypeParser` validates the raw value in one place, so every invalid value — an unknown name, an out-of-range number, `null`, or a missing field — produces the same clear message.
- **Controller over a minimal API.** `TollController` keeps `Program.cs` limited to host setup, and lets the request handling be unit-tested directly (`TollControllerTests`) without going through HTTP.
- **No state, no database.** Each request is self-contained.

## What was wrong with the original code

1. **The hourly window never worked.** It subtracted `Millisecond` components (0-999) instead of full timestamps, so the difference was never over 60 minutes and every passage fell into one window. The window start was also never moved, and the "subtract the previous fee, add the new one" bookkeeping produced wrong totals.
2. **Hole in the fee table.** `hour >= 8 && hour <= 14 && minute >= 30` returned 0 for 09:00-09:29, 10:00-10:29 and so on until 14:29. It should be 8 SEK from 08:30 to 14:59.
3. **Holidays existed only for 2013**, and any other year silently had none.
4. **Weak input handling.** A `null` vehicle was charged, an empty array threw `IndexOutOfRangeException`, and passages were assumed sorted and on a single day.
5. **Stringly typed vehicles.** `GetVehicleType()` results were compared against enum `.ToString()` values, so a typo compiled fine. Only `Car` and `Motorbike` classes existed for seven vehicle types.
6. **Unpaid passages could swallow paid ones.** A passage at 05:50 would open a window and hide a paid passage at 06:20.
7. **No tests.**

## Limitations and possible next steps

- Only 2013 is supported. A computed Swedish holiday provider (Easter-based dates, Midsummer, All Saints') would remove that limit.
- Fee bands and the daily cap are constants in code. They could move to `appsettings.json`.
- Time zones and daylight-saving changes are not modelled.
- Invalid date-times and malformed JSON return an empty 400 instead of a descriptive message.
- No persistence, authentication or API documentation UI (for example OpenAPI/Swagger).
- `TollControllerTests` exercises the controller action directly; true in-process HTTP integration tests could be added with `WebApplicationFactory` alongside it.

## Original assignment

<details>
<summary>Show the original brief</summary>

### Toll fee calculator 1.0
A calculator for vehicle toll fees.

* Make sure you read these instructions carefully
* The current code base is in Java and C#, but please make sure that you do an implementation in a language **you feel comfortable** in like Javascript, Python, Assembler or [ModiScript](https://en.wikipedia.org/wiki/ModiScript) (please don't choose ModiScript).
* No requirement but bonus points if you know what movie is in the gif

#### Background
Our city has decided to implement toll fees in order to reduce traffic congestion during rush hours.
This is the current draft of requirements:

* Fees will differ between 8 SEK and 18 SEK, depending on the time of day
* Rush-hour traffic will render the highest fee
* The maximum fee for one day is 60 SEK
* A vehicle should only be charged once an hour
  * In the case of multiple fees in the same hour period, the highest one applies.
* Some vehicle types are fee-free
* Weekends and holidays are fee-free

#### Your assignment
The last city-developer quit recently, claiming that this solution is production-ready.
You are now the new developer for our city - congratulations!

Your job is to deliver the code and from now on, you are the responsible go-to-person for this solution. This is a solution you will have to put your name on.

#### Instructions
You can make any modifications or suggestions for modifications that you see fit. Fork this repository and deliver your results via a pull-request. You could also create a gist, for privacy reasons, and send us the link.

</details>