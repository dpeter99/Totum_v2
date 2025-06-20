# Observability Driven Development (ODD) Structure Guide

## Overview

This document describes the structure and implementation patterns for Observability Driven Development (ODD) as demonstrated in the `todo-odd` example project. ODD is a development approach where observability requirements are defined and tested first, ensuring that applications emit meaningful telemetry from the start.

## Core Principles

1. **Telemetry as First-Class Requirements**: Observability requirements are defined alongside functional requirements
2. **Test-Driven Telemetry**: Tests verify that the correct telemetry is emitted during operations
3. **Strategic Instrumentation**: Instrument critical business operations, not just infrastructure
4. **Observable by Design**: Build applications with observability built-in, not bolted-on

## Project Structure

```
todo-odd/
├── src/
│   ├── Controllers/
│   │   ├── TodoController.cs          # Business logic with instrumentation
│   │   └── TodoListController.cs      # Caching logic with instrumentation
│   ├── Database.cs                    # Entity Framework DbContext and models
│   ├── Program.cs                     # Application startup with ActivitySource
│   ├── appsettings.json              # Application configuration
│   └── todo-odd.csproj               # Main project dependencies
├── tests/
│   ├── Setup.cs                      # OpenTelemetry test framework setup
│   ├── InMemoryTestSpans.cs          # Custom span collection for testing
│   ├── CustomApplicationFactoryWithTelemetry.cs  # Test factory with telemetry
│   ├── TodoAdminBaseTest.cs          # Base test class with utilities
│   ├── TodoAdminTests.cs             # Traditional functional tests
│   ├── TodoAdminTestsWithODD.cs      # ODD-focused tests
│   ├── tests.csproj                  # Test project with OpenTelemetry packages
│   └── appsettings.Test.json         # Test configuration
└── todo-odd.sln                     # Solution file
```

## Key Components

### 1. Application Instrumentation

#### ActivitySource Declaration (`Program.cs:46-49`)
```csharp
public static class ActivityHelper
{
    public static ActivitySource Source = new ActivitySource("todo-odd");
}
```

**Purpose**: Central ActivitySource for all custom spans in the application.

#### Strategic Span Instrumentation

**TodoController.cs:24-26** - Save Operation Instrumentation:
```csharp
using var span = ActivityHelper.Source.StartActivity("save-todo");
span?.AddTag("todo.title", item.Title);
```

**TodoListController.cs:25-32** - Database Query Instrumentation:
```csharp
using (var span = ActivityHelper.Source.StartActivity("get-todo-list-from-db"))
{
    var allTodos = _context.TodoItems.ToList();
    // ... rest of operation
}
```

**Instrumentation Strategy**:
- Instrument business operations, not just technical operations
- Add meaningful tags with business context
- Use descriptive span names that reflect business intent
- Wrap critical operations that need to be observable

### 2. Test Infrastructure

#### OpenTelemetry Test Framework (`Setup.cs:10-25`)
```csharp
public class OtelTestFramework : TracedTestFramework
{
    public static readonly InMemoryTestSpans CollectedSpans = [];
    
    public OtelTestFramework(IMessageSink messageSink) : base(messageSink)
    {
        traceProviderSetup = tpb => {
            tpb
                .ConfigureResource(resource => resource.AddService("Unit-Tests"))
                .AddSource("UnitTests")
                .AddSource(ActivityHelper.Source.Name)  // Include app's ActivitySource
                .AddInMemoryExporter(CollectedSpans)    // Collect spans for testing
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter();
        };
    }
}
```

**Key Features**:
- Extends `PracticalOtel.xUnit.OpenTelemetry.TracedTestFramework`
- Captures spans in memory for test assertions
- Includes both application and ASP.NET Core instrumentation
- Provides global span collection accessible in tests

#### Custom Span Collection (`InMemoryTestSpans.cs`)

**Purpose**: Thread-safe, trace-aware span collection for test assertions.

**Key Methods**:
- `SpanExistsWithName(string name)` - Check if span exists
- `GetSpanByName(string name)` - Retrieve spans by name
- `RemoveAllSpansForTest()` - Clean up between tests
- `Add(Activity item)` - Store spans by trace ID

#### Test Application Factory (`CustomApplicationFactoryWithTelemetry.cs`)

**Purpose**: Customized WebApplicationFactory that:
- Uses in-memory database for isolation
- Includes telemetry instrumentation
- Provides clean test environment

### 3. Testing Patterns

#### ODD Test Structure (`TodoAdminTestsWithODD.cs`)

**Test Categories**:

1. **Telemetry Verification Tests**:
```csharp
[Fact]
public async Task AddTodo_WithValidRequest_ProducesSaveTelemetry()
{
    var addResponse = await CreateValidToDoItem(_api);
    
    var saveActivity = OtelTestFramework.CollectedSpans.FirstOrDefault(
        a => a.DisplayName.Contains("save-todo"));
    Assert.NotNull(saveActivity);
    
    var titleTag = saveActivity.Tags.FirstOrDefault(t => t.Key == "todo.title");
    Assert.Equal("New Todo", titleTag.Value);
}
```

2. **Performance Behavior Tests**:
```csharp
[Fact]
public async Task GetTodos_WithCachedData_DoesNotCallDatabase()
{
    // First call to populate cache
    var getAll = await _api.GetFromJsonAsync<List<TodoItem>>("todo-list");
    OtelTestFramework.CollectedSpans.RemoveAllSpansForTest();
    
    // Second call should use cache
    var getAllCached = await _api.GetFromJsonAsync<List<TodoItem>>("todo-list");
    
    var cacheActivity = OtelTestFramework.CollectedSpans.FirstOrDefault(
        a => a.DisplayName.Contains("get-todo-list-from-db"));
    Assert.Null(cacheActivity); // Should not call database
}
```

3. **Parallel Processing Tests**:
```csharp
[Fact]
public async Task GetTodos_AsyncBatching_IsParallel()
{
    var getAll = await _api.GetAsync("todo-list");
    
    var rootSpan = OtelTestFramework.CollectedSpans.GetSpanByName("start-get")
        .FirstOrDefault();
    var processingSpans = OtelTestFramework.CollectedSpans.GetSpanByName("get-from-db");
    
    foreach (var span in processingSpans)
        Assert.Equal(span.ParentId, rootSpan.Id); // Verify parallel structure
}
```

#### Traditional Tests vs ODD Tests

**Traditional Tests** (`TodoAdminTests.cs`):
- Focus on functional correctness
- Use `SuppressInstrumentationScope.Begin()` to avoid telemetry
- Test HTTP status codes and response data

**ODD Tests** (`TodoAdminTestsWithODD.cs`):
- Focus on observability requirements
- Verify telemetry is emitted correctly
- Test performance characteristics through spans
- Ensure business context is captured in telemetry

### 4. Dependencies and Packages

#### Main Application (`todo-odd.csproj`)
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SQLite" Version="6.0.7" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.2.3" />
```

#### Test Project (`tests.csproj`)
```xml
<!-- Testing Framework -->
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.10" />
<PackageReference Include="xunit" Version="2.9.2" />

<!-- OpenTelemetry Testing -->
<PackageReference Include="OpenTelemetry.Exporter.InMemory" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.9.0" />
<PackageReference Include="PracticalOtel.xUnit.OpenTelemetry" Version="0.9.1-alpha" />
```

## Implementation Guide

### Step 1: Define Observability Requirements

Before writing code, define what telemetry you need:
- What operations should be traced?
- What business context should be captured?
- What performance characteristics need monitoring?

### Step 2: Set Up Test Infrastructure

1. **Install Required Packages**:
   - `PracticalOtel.xUnit.OpenTelemetry` for test framework
   - `OpenTelemetry.Exporter.InMemory` for span collection
   - `OpenTelemetry.Instrumentation.AspNetCore` for web instrumentation

2. **Create Test Framework**:
   - Extend `TracedTestFramework`
   - Configure OpenTelemetry with in-memory exporter
   - Register your application's ActivitySource

3. **Create Custom Span Collection**:
   - Implement thread-safe span storage
   - Organize spans by trace ID
   - Provide utility methods for span querying

### Step 3: Write ODD Tests

1. **Test Telemetry Emission**:
   ```csharp
   [Fact]
   public async Task Operation_EmitsExpectedSpan()
   {
       // Arrange & Act
       await PerformOperation();
       
       // Assert
       var span = CollectedSpans.FirstOrDefault(s => s.DisplayName == "expected-span");
       Assert.NotNull(span);
   }
   ```

2. **Test Business Context Capture**:
   ```csharp
   [Fact]
   public async Task Operation_CapturesBusinessContext()
   {
       // Act
       await PerformOperation(businessId: "123");
       
       // Assert
       var span = CollectedSpans.GetSpanByName("operation-span").First();
       var businessTag = span.Tags.FirstOrDefault(t => t.Key == "business.id");
       Assert.Equal("123", businessTag.Value);
   }
   ```

3. **Test Performance Characteristics**:
   ```csharp
   [Fact]
   public async Task CachedOperation_DoesNotCallExpensiveService()
   {
       // Arrange - warm cache
       await CallOperation();
       CollectedSpans.RemoveAllSpansForTest();
       
       // Act - call again
       await CallOperation();
       
       // Assert - should not see expensive operation span
       var expensiveSpan = CollectedSpans.FirstOrDefault(s => s.DisplayName == "expensive-operation");
       Assert.Null(expensiveSpan);
   }
   ```

### Step 4: Implement Application Instrumentation

1. **Create ActivitySource**:
   ```csharp
   public static class ActivityHelper
   {
       public static ActivitySource Source = new ActivitySource("your-app-name");
   }
   ```

2. **Instrument Critical Operations**:
   ```csharp
   public async Task<Result> CriticalOperation(string businessId)
   {
       using var span = ActivityHelper.Source.StartActivity("critical-operation");
       span?.AddTag("business.id", businessId);
       span?.AddTag("operation.type", "critical");
       
       // Perform operation
       var result = await DoWork();
       
       span?.AddTag("operation.result", result.Status);
       return result;
   }
   ```

3. **Follow Naming Conventions**:
   - Use kebab-case for span names
   - Include business context, not just technical details
   - Use hierarchical naming (e.g., "user-registration", "user-registration.validation")

### Step 5: Run Tests First

1. **Red Phase**: Write failing ODD tests
2. **Green Phase**: Implement minimal instrumentation to pass tests
3. **Refactor Phase**: Improve instrumentation while keeping tests passing

## Best Practices

### Span Naming
- Use business-meaningful names: `save-todo`, not `database-insert`
- Be consistent with naming conventions
- Include operation context: `user-registration.email-validation`

### Tag Strategy
- Include business identifiers: `user.id`, `order.id`
- Add operation context: `operation.type`, `cache.hit`
- Avoid sensitive data in tags
- Use consistent tag naming conventions

### Test Organization
- Separate traditional tests from ODD tests
- Use descriptive test names that indicate telemetry expectations
- Clean up spans between tests
- Use shared utilities for common operations

### Error Handling
- Instrument error paths with appropriate tags
- Use span status to indicate success/failure
- Capture error context without exposing sensitive information

## Common Patterns

### 1. Cache Behavior Testing
```csharp
// Test cache miss
var result1 = await CallCachedOperation();
var dbSpan = CollectedSpans.GetSpanByName("database-call");
Assert.NotEmpty(dbSpan);

// Clean spans and test cache hit
CollectedSpans.RemoveAllSpansForTest();
var result2 = await CallCachedOperation();
var dbSpan2 = CollectedSpans.GetSpanByName("database-call");
Assert.Empty(dbSpan2);
```

### 2. Parallel Processing Verification
```csharp
var result = await ProcessBatch(items);

var rootSpan = CollectedSpans.GetSpanByName("batch-process").First();
var childSpans = CollectedSpans.GetSpanByName("process-item");

// Verify all items processed in parallel
Assert.All(childSpans, span => Assert.Equal(rootSpan.Id, span.ParentId));
```

### 3. Business Context Capture
```csharp
await ProcessOrder(orderId: "ORDER-123", customerId: "CUST-456");

var span = CollectedSpans.GetSpanByName("process-order").First();
var orderTag = span.Tags.FirstOrDefault(t => t.Key == "order.id");
var customerTag = span.Tags.FirstOrDefault(t => t.Key == "customer.id");

Assert.Equal("ORDER-123", orderTag.Value);
Assert.Equal("CUST-456", customerTag.Value);
```

## Benefits of ODD

1. **Observability by Design**: Ensures telemetry is built into the application from the start
2. **Testable Observability**: Verifies that observability requirements are met
3. **Business Context Capture**: Ensures meaningful business context is captured in telemetry
4. **Performance Insights**: Tests verify performance characteristics through telemetry
5. **Regression Prevention**: Prevents observability regressions through automated testing
6. **Documentation**: Tests serve as living documentation of observability requirements

## Conclusion

ODD transforms observability from an afterthought into a first-class development practice. By writing tests for telemetry emission, developers ensure that their applications provide the visibility needed for production operation and debugging.

This structure provides a complete framework for implementing ODD in any .NET application, with patterns that can be adapted to other technologies and frameworks.