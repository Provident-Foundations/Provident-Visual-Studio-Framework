![Provident Foundations banner](PFBanner.png)

# Provident Foundations Framework

The core Provident Foundations library supplies reusable building blocks for .NET applications and for the other libraries in this repository. It consolidates common functionality from the original framework under the `Provident.Foundations` namespaces.

## What's included

- **Application abstractions:** disposable, property-aware, exception-tracking, logging and business-object base classes; repository and operation-result contracts.
- **Data and validation:** collections, common value types, conversion helpers, attributes and validation messages.
- **Infrastructure:** file and serialization utilities, logging, TCP networking helpers, and security and cryptography types.
- **Supporting utilities:** events, extensions, console helpers, state and financial types.

For example, `Provident.Foundations.Abstractions` includes `DisposableObjectBase`, `PropertyAwareBase`, `ExceptionTrackingBase`, `BusinessBase` and the `IOperationResult` contracts. `LoggableBase` is in `Provident.Foundations.Abstractions.Logging`.

## Requirements and use

The project targets **.NET 11** (`net11.0`) and currently specifies x64 as its platform target. Reference the `Provident.Foundations` package or the project from a compatible application. Some individual APIs are platform-specific; consult their platform annotations and documentation.

The library is provided without guarantees; evaluate its suitability for your application before use. See `License.txt` for licensing terms.
