# MarktGuru Interview Prep

## How We Will Use This

- I will ask one interview question at a time.
- You answer in your own words first.
- Then we refine it into a stronger interview-ready answer.
- I will keep both your raw answer and the polished answer here.
- Some questions are conceptual, and some are code-reading or debugging questions.

## Interview Strategy

The strongest story for this code test is:

1. I first stabilized existing behavior and fixed correctness bugs.
2. I added the retailer data model and kept the migration reversible.
3. I built the new API endpoints with validation, clear status codes, and DTO projections.
4. I paid special attention to query count: no N+1 behavior, and the required endpoints use a single database round-trip where specified.
5. I added the Hangfire expiry job as an idempotent bulk update.
6. I backed the changes with focused unit and integration tests.
7. I used AI assistance, but I reviewed and understood the implementation and documented the trade-offs.

## Question 1 - Walk Me Through Your Solution

### Interviewer Question

Can you walk me through what you changed in this code test and why?

### Why They Ask

They want to know whether you understand the whole submission, not just individual lines of code. This is also where they check honesty, prioritization, and whether you can communicate backend trade-offs clearly.

### Your Raw Answer

Okay. If I remember correctly, this code changes about the implementing some tests and also refactoring some issues about performance issue. As an example, it's about the N plus one query and was about some null issue, if I remember correctly. That's about that.

### Strong Answer

I started by stabilizing the existing service and fixing correctness issues before adding the new features. The original code had a few reliability problems, including async flow issues in the import endpoint, validation gaps that could lead to bad data or server errors, and some query patterns that could cause unnecessary database work such as N+1 behavior.

After that, I extended the data model with retailers. I added the `Retailers` table, a nullable `RetailerId` relationship on offers, seeded initial retailers, and kept the migration reversible. Then I implemented the required endpoints: a paginated and filterable offer list, retailer statistics, and retailer creation with proper validation and status codes.

The main technical focus was query efficiency and predictable API behavior. For example, the offer list returns the page rows and total count in one database execution, and retailer stats are projected through EF Core instead of loading all offers into memory. I also added the Hangfire `OfferExpiryJob` using a bulk update so expired offers are updated in one statement and the job stays idempotent.

Finally, I expanded the tests around both behavior and edge cases: response bodies, validation errors, duplicate retailers, offers without retailers, query-count expectations, and the background job. So the solution was not only adding endpoints, but also making the service more correct, observable, and safer under real usage.

### Key Points To Mention

- Fixed existing async and correctness issues first, including replacing `async void` import behavior with an awaited `Task`.
- Added validation so bad import/list/retailer inputs return `400 Bad Request` instead of leaking into persistence or causing `500`.
- Added the `Retailers` table, nullable `Offer.RetailerId`, EF navigation, seeded retailers, and reversible migration.
- Implemented `GET /api/offers` with pagination, filtering, retailer/product projection, and a single query execution for total plus page rows.
- Implemented `GET /api/retailers/{id}/stats` with active-retailer behavior and aggregate projection.
- Implemented `POST /api/retailers` with trimming, country normalization, duplicate-name conflict handling, and `201 Created`.
- Added `OfferExpiryJob` using `ExecuteUpdateAsync` so expiration is one bulk update and idempotent.
- Expanded tests to cover response bodies, validation, duplicate conflicts, query counts, and job behavior.

### Follow-Up Questions They May Ask

- Which bug was the most important and why?
- Why did you care about `async void`?
- How did you prove the list endpoint does not issue N+1 queries?
- What would you change if this became a production service?

## Question 2 - Code Reading: Offer List Query

### Interviewer Question

Look at `OfferService.GetOffersAsync`. Why is `pageQuery` built before `ToListAsync`, and when is the database actually hit?

### Code Reference

`src/OffersService/Services/OfferService.cs`

### Your Raw Answer

because I need apply filter and thats why i need to return Iquaryable and then in neccessary time run .tolistasync

### Strong Answer

`pageQuery` is built as an `IQueryable`, so at that point it is only describing the SQL query; it has not gone to the database yet. This lets me keep composing the query: first apply the filters, then ordering, pagination, projection, and finally combine it with the total count requirement.

The database is actually hit when EF Core reaches the terminal operation, which is the awaited `ToListAsync()`. After that, the result is already materialized in memory, so the later LINQ `.Where(...)`, `.Select(...)`, and `.ToList()` only map query rows into the public DTO and do not cause another database call.

That distinction matters here because the exercise required the list endpoint to return both the total count and the paged rows without issuing multiple database round-trips.

### Key Points To Mention

- `pageQuery` is still an `IQueryable`; it describes SQL but does not execute yet.
- EF Core executes when the terminal operation `ToListAsync()` is awaited.
- The method composes the filtered query, total count, and paged rows into one query execution.
- The later `.Select(...).ToList()` maps in memory after the database result is already materialized.
- `AsNoTracking()` is appropriate because the endpoint is read-only.

## Question 3 - Code Reading: Retailer Stats

### Interviewer Question

In `RetailerService.GetStatsAsync`, why are `Average` and `Min` cast to nullable values?

### Code Reference

`src/OffersService/Services/RetailerService.cs`

### Your Raw Answer

چون که price decimal هستش و ما میخواستیم قیمت رو برگردونیم

### Strong Answer

The main reason is not just that `Price` is numeric; it is to handle retailers with no active offers. If we run `Average` or `Min` over an empty sequence with a non-nullable numeric type, it can fail because there is no value to return.

By casting `offer.Price` to a nullable value, EF can translate the aggregate so the database returns `NULL` when there are no matching active offers. Then the DTO can safely expose `AvgPrice` and `LowestPrice` as `null` instead of throwing an exception or returning a misleading value like zero.

The code converts the nullable aggregate result back to `decimal?` after materialization, so the API response represents the absence of active-offer stats explicitly.

### Key Points To Mention

- A retailer may have zero active offers.
- Non-nullable `Average` or `Min` over an empty sequence would fail.
- Casting to nullable lets SQL return `NULL` for empty aggregates.
- The DTO can then represent missing aggregate values as `null`.

## Question 4 - Code Reading: Hangfire Expiry Job

### Interviewer Question

Why did you use `ExecuteUpdateAsync` in `OfferExpiryJob` instead of loading offers and updating them in a loop?

### Code Reference

`src/OffersService/Jobs/OfferExpiryJob.cs`

### Your Raw Answer

because i would like to run this method as background job in different thread as parallel

### Strong Answer

`ExecuteUpdateAsync` is not mainly about running the job in a different thread or in parallel. Hangfire already handles running the job in the background. The reason for `ExecuteUpdateAsync` is that the database can update all matching rows with one bulk `UPDATE` statement.

If I loaded the matching offers into memory and updated them in a loop, EF would need to materialize entities, track changes, and then send updates through `SaveChangesAsync`. That uses more memory and can produce more database work. With `ExecuteUpdateAsync`, the condition and update are pushed directly to SQL:

`ValidTo < timestamp && Status != Expired`

That also makes the job idempotent. If it runs twice, the second run finds no matching non-expired rows and returns zero. I also pass the `CancellationToken` into the database operation and log the affected-row count.

### Key Points To Mention

- `ExecuteUpdateAsync` sends one bulk `UPDATE` statement to the database.
- Loading entities would use more memory and can create one or more unnecessary round-trips.
- The job is idempotent because already-expired offers are excluded.
- The cancellation token is propagated to the database operation.
- The returned affected-row count is used for logging and tests.

## Question 5 - Debugging Scenario: async void

### Interviewer Question

The original import flow used `async void`. What problem can that cause in an ASP.NET Core API?

### Your Raw Answer

i dont have idea about that

### Strong Answer

`async void` is dangerous in an ASP.NET Core API because the caller cannot await it. In a controller flow, that means the request can return a response before the asynchronous import work has actually finished.

It also makes exception handling unreliable. If an exception happens inside an `async void` method, it cannot be observed through the normal `await` chain, so the controller cannot catch it and the centralized exception handling pipeline may not turn it into a proper API response.

The correct shape is to return `Task` or `Task<T>`. Then the controller can `await` the operation, return the real import result, and let exceptions flow through normal ASP.NET Core error handling.

### Key Points To Mention

- `async void` cannot be awaited by the controller.
- Exceptions may bypass normal request handling and become unobserved or crash-prone.
- The HTTP response can be returned before work finishes.
- Returning `Task` lets the controller await completion, handle errors, and return accurate results.

## Question 6 - Design Trade-Off: Single Round-Trip Pagination

### Interviewer Question

Would you always combine total count and page rows into one query like this?

### Your Raw Answer

because everything is stateless i think it s better to comibne in one query 
in each query that we want to see the result total count and page rows

### Strong Answer

I would not say this is always the best approach. In many normal APIs, using two simpler queries can be easier to read and maintain: one query for `CountAsync()` and one query for the paged rows. That is often acceptable if the database latency is low and the generated SQL is straightforward.

In this exercise, though, the requirement explicitly said the endpoint must not issue more than one database round-trip. That is why I combined the unpaged total and page rows into a single EF query execution.

So my answer is: I used one query here because it matched the requirement and I added tests around the command count. In production, I would check the generated SQL, indexes, execution plan, data size, and readability before deciding whether one complex query is better than two simpler ones.

### Key Points To Mention

- Not always; two simpler queries are often easier to read and maintain.
- In this exercise, the requirement explicitly asked for one database round-trip.
- The chosen query favors the requirement and has tests around command count.
- In production, I would compare readability, generated SQL, database plan, and performance under realistic data.

## Question 7 - Validation And Status Codes

### Interviewer Question

How did you decide what should return `400`, `404`, and `409`?

### Your Raw Answer

because 400 is bad request and if invalid country must be return as 400
404 not found it must resut when resuorce or is not exist 
and 409 is conflict 
that there is existing data 
because we have a lot http status that reciever have more better view for response 
thats why we need to follow this exact http status code for responses

### Strong Answer

I chose the status codes based on the type of problem the client needs to understand.

`400 Bad Request` is for invalid input. For example, if the client sends an invalid country code, an invalid page size, or an unsupported status query value, the request itself is not valid and should be rejected before business logic continues.

`404 Not Found` is for a resource that cannot be returned. For retailer stats, if the retailer does not exist, the API returns `404`. The exercise also says inactive retailers should return `404`, so I followed that contract and treated inactive retailers as unavailable for that endpoint.

`409 Conflict` is different from validation. The create-retailer request can be structurally valid, but if another retailer already has the same name, it conflicts with the current database state. That is why duplicate retailer names return `409`.

Using the right status codes helps API consumers understand whether they should fix their request, look for another resource, or handle a business conflict.

### Key Points To Mention

- `400 Bad Request`: input is syntactically or semantically invalid, such as bad query values or invalid country code.
- `404 Not Found`: requested retailer does not exist or is inactive, matching the exercise contract.
- `409 Conflict`: the create request is valid, but conflicts with existing state, such as duplicate retailer name.
- FluentValidation centralizes validation rules and message constants keep tests and validators aligned.

## Question 8 - Tests

### Interviewer Question

Which tests give you the most confidence in this solution?

### Your Raw Answer

I use unit test for each method that i have written 
but in production level i use integration test for testing api functionallity and database funcatiolity and i try use test database into integration test with small database such as sqlite instead of using in memory database because i can easily test some constraint in the database and also i can check better some performance issue if there is 
and also i use end to end test for testing some features in one workflow 
this test give me more confidence for production 
and also i always ask from other teammate such as QA to write some business test and i also follow BDD in our project with gherkin language to reach better maintainable application 
with these test actually i have confidence for refactoring and also releasing a new version  

### Strong Answer

The tests that give me the most confidence are the ones that verify behavior through the API and the database, not only individual methods.

For this project, the integration tests are important because they check the real HTTP behavior: status codes, response bodies, validation errors, duplicate retailer handling, empty pages, and offers without retailers. I also prefer using SQLite-backed integration tests instead of EF InMemory because SQLite exercises relational behavior like constraints more realistically.

I also trust the query-count tests because performance was part of the requirement. For example, the offer list and retailer stats should not accidentally turn into N+1 queries or multiple round-trips after a future refactor.

For the background job, the unit tests are valuable because they verify the exact business rule: only offers where `ValidTo < now` and `Status != Expired` are updated, the update is bulk, and running the job on a clean database returns zero.

In a production project, I would add end-to-end workflow tests as well, and I like involving QA or business stakeholders through BDD-style scenarios when the business rules are complex. That gives more confidence when refactoring or releasing.

### Key Points To Mention

- Integration tests for endpoint status codes and response bodies.
- Query-count tests for endpoints with one-round-trip requirements.
- Unit tests for service aggregation behavior.
- Job tests verifying only matching rows are expired, the operation is bulk, and clean runs return zero.
- Tests for edge cases: empty pages, null retailer fields, invalid status values, duplicates, inactive retailers.

## Question 9 - AI Honesty

### Interviewer Question

The README says AI use is allowed but you should understand every line. How did you use AI?

### Your Raw Answer

i prefer to use Ai as asistent becuase this project dosent have instruction for ai awerness 
and as you know ai responses have a lot of hallucination 
thats why we need to make some harness for ai in our project 
but in this specefic case I mean in this project 
I prefer use ai beside of myself and also i using in all project ai with rtk and also superpowers and codebase memory to reach better performance and increase my productivity 
but i Think we need some harness for our project to deceresase the hallucination in our project 
and also after a while we need to modify this .md file continusely 

### Strong Answer

I used AI as an assistant, not as something I trusted blindly. It helped me move faster with implementation ideas, refactoring suggestions, test coverage ideas, and reviewing edge cases. But I still treated the AI output as something that needed verification, because AI can hallucinate or suggest code that looks correct but does not match the project requirements.

For this exercise, I used the project README, tests, documentation, and the actual code as the source of truth. After AI-assisted changes, I reviewed the code, ran tests, checked behavior, and documented the design decisions in `Solution.md`.

One lesson I would take into a real team is that AI works much better when the project has clear guidance: architecture rules, coding conventions, test expectations, and documentation routing. Those instructions reduce hallucination and make AI-assisted work more consistent. I would keep those Markdown guidance files updated as the project evolves.

So my answer is: AI helped me be faster, but I was responsible for understanding the code, validating it, and being able to explain the trade-offs.

### Key Points To Mention

- Be honest and calm.
- Say AI helped with implementation support, feedback, and iteration.
- Emphasize that you reviewed the code, ran tests, documented decisions, and can explain the trade-offs.
- Avoid pretending everything was written manually.

## Question 10 - With More Time

### Interviewer Question

What would you improve with more time?

### Your Raw Answer

actually this project folder and structure not clear 
I prefer follow clean architetcutre and also apply some abstraction level in this rpoject 
this is first step 
and also 
apply metric for understanding if there is any performance issue in controller
or apply logging in this project for reach better logging if there is any issue in this context

### Strong Answer

With more time, I would first improve the architecture gradually, not rewrite everything at once. The current project works for an exercise, but if it grows, I would separate API, Application, Domain, and Infrastructure concerns more clearly. That would make business rules easier to test and reduce framework coupling in the core logic.

I would also improve observability. For example, I would add more structured logging around important workflows like offer import, retailer creation, and the expiry job. I would also add metrics around endpoint latency, database query count, failed validations, and job execution results so performance issues are easier to detect.

For testing, I would add end-to-end tests against the real MariaDB provider through Docker, because SQLite-backed tests are useful but may miss provider-specific behavior. I would also add contract tests for the API response shapes so future refactors do not accidentally break clients.

So my priorities would be: clearer architecture, better observability, and more production-like testing.

### Key Points To Mention

- Run end-to-end tests against MariaDB, not only SQLite-backed tests.
- Add contract tests for exact API response shapes.
- Improve OpenAPI examples and operational Hangfire configuration.
- Consider clearer separation between API, Application, Domain, and Infrastructure if the service grows.
- Add architecture tests to protect boundaries.

## Question 11 - Offers Without Retailers

### Interviewer Question

In `GET /api/offers`, why was it important to include offers without a retailer, and how does the code avoid filtering them out?

### Your Raw Answer

I think it depends on business

### Strong Answer

In general, whether offers without retailers should be returned is a business decision. In this exercise, the requirement and project notes say those offers must be included, with `retailerId` and `retailerName` returned as `null`.

That matters because `Offer.RetailerId` is nullable. If the query used an inner join or required `offer.Retailer` to exist, it would accidentally drop valid offer rows. The code avoids that by projecting from `Offers` and reading nullable retailer fields:

`RetailerId = offer.RetailerId`

`RetailerName = offer.Retailer == null ? null : offer.Retailer.Name`

So the offer row stays in the result even when there is no related retailer. The endpoint only filters by retailer when the client explicitly sends a `retailerId` filter.

### Key Points To Mention

- Business rules decide whether null-retailer offers should be included.
- In this exercise, the contract explicitly says to include them.
- `RetailerId` is nullable, so the API must handle that relationship safely.
- Avoid accidental inner-join behavior that removes rows.
- Return `null` retailer fields rather than dropping the offer.

## Question 12 - Duplicate Retailer Conflict

### Interviewer Question

در `POST /api/retailers`، چرا duplicate retailer name را فقط validation error حساب نکردیم و به‌جایش `409 Conflict` برگرداندیم؟

### Your Raw Answer

نمیدونم

### Strong Answer

Duplicate retailer name is different from a normal validation error. If the client sends an empty name or an invalid country code, the request itself is invalid, so `400 Bad Request` makes sense.

But with duplicate name, the request can be structurally valid: the name is present, length is okay, and the country code is valid. The problem is that the request conflicts with the current database state because another retailer already has that name.

That is why `409 Conflict` is a better status code. It tells the API client: your request is understandable, but it cannot be applied because of an existing resource.

### Key Points To Mention

- `400` means invalid input shape or invalid field value.
- `409` means valid request, but conflicting with current server state.
- Duplicate name is enforced by business/database uniqueness.
- The client can resolve it by choosing another name or handling the existing retailer.

## Question 13 - Retailer Input Normalization

### Interviewer Question

در `CreateRetailerAsync`، چرا `Name` را `Trim()` کردیم و `Country` را `ToUpperInvariant()`؟

### Your Raw Answer

تریم کردیم اگه اسپیس خورد اینطرف اونطرفش درست سیوش کنیم 
اونجا هم upper کردیم که ملاک مقایسه امون همیشه با یک مقدار ولید باشه حتما

### Strong Answer

We trim the retailer name so accidental leading or trailing spaces do not become part of the stored business value. For example, `"Billa"` and `" Billa "` should not behave like different retailer names.

We uppercase the country code to store it in a consistent format. ISO country codes are commonly represented as uppercase two-letter codes, so `"at"` becomes `"AT"`.

Using `ToUpperInvariant()` is also intentional because it avoids culture-specific casing behavior. The normalization makes duplicate checks, persistence, and API responses more predictable.

### Key Points To Mention

- Trim removes accidental whitespace from business values.
- Normalization prevents visually identical values from being stored differently.
- Uppercase country codes match ISO-style API expectations.
- `Invariant` avoids culture-specific casing surprises.

## Question 14 - Concurrent Duplicate Creation

### Interviewer Question

در `CreateRetailerAsync`، اگر دو request همزمان با یک name بیایند، آیا `AnyAsync` برای جلوگیری از duplicate کافی است؟

### Your Raw Answer

نمیدونم اصلا

### Strong Answer

No, `AnyAsync` alone is not enough in a concurrent system. It is useful for returning a clean `409 Conflict` in the common case, but it has a race condition.

For example, two requests can both run `AnyAsync` at almost the same time. Both see that the retailer name does not exist yet. Then both try to insert. Without a database unique constraint, duplicates could be created.

The final protection should be a unique index or unique constraint in the database. Then the application should ideally catch the duplicate-key exception and map it to `409 Conflict` as well.

So the best production answer is: use `AnyAsync` for a friendly pre-check, but rely on the database uniqueness constraint as the source of truth.

### Key Points To Mention

- `AnyAsync` pre-check can race under concurrent requests.
- Database unique constraint is the real guarantee.
- Catch duplicate-key exceptions and return `409`.
- Pre-check is still useful for the normal path and clearer code.

## Question 15 - Empty Filtered Offer Query

### Interviewer Question

در `GetOffersAsync`، اگر `filteredOffers` هیچ رکوردی نداشته باشد، این query با `GroupBy(_ => 1)` چه رفتاری دارد؟ آیا هنوز یک row با `Total = 0` برمی‌گرداند؟

### Your Raw Answer

بهتر بود شرط رو بزاریم که خروجی reliable بشه 
ولی بله total رو صفر بر میگردونه ولی بهتر بود شرط رو میزاشتیم اگه صفر هست total رو صفر برگردونه

### Strong Answer

If `filteredOffers` has no rows at all, `GroupBy(_ => 1)` does not create a group. So the database result is an empty list; it does not return one row with `Total = 0`.

The code handles that with:

`var total = rows.FirstOrDefault()?.Total ?? 0;`

So when no rows come back, `FirstOrDefault()` is `null` and the total becomes zero in application code.

`DefaultIfEmpty()` helps with a different case: when there are matching filtered offers, so a total exists, but the requested page is empty, for example because the client requested a page number beyond the available data. In that case it can still preserve the total while returning no items.

### Key Points To Mention

- Empty `filteredOffers` means `GroupBy(_ => 1)` returns no group.
- `Total = 0` comes from the C# fallback, not from a returned SQL row.
- `DefaultIfEmpty()` is mainly for empty page rows when a count row exists.
- This is exactly why the nullable/fallback mapping exists.

## Question 16 - FluentValidation And Message Catalog

### Interviewer Question

چرا validation را داخل controller با `if`های دستی ننوشتی و از FluentValidation + message catalog استفاده کردی؟ مزیتش چیست؟

### Your Raw Answer

در واقع ما دیگه لازم نیست کد duplicate بنویسیم و یک دست باشه 

### Strong Answer

Using FluentValidation keeps validation rules separate from controller flow. The controller can focus on HTTP behavior, while the validator owns input rules such as page limits, supported status values, required name, and valid country code.

It also reduces duplication. If the same rule or message is needed in multiple tests or validators, a centralized message catalog prevents repeating literal strings everywhere. That makes tests less brittle and keeps API error messages consistent.

Another advantage is testability. Validators can be tested directly without starting the whole API, and integration tests can focus on whether invalid input becomes the expected `400 Bad Request` response.

So the main benefits are cleaner controllers, reusable validation rules, consistent error messages, and easier testing.

### Key Points To Mention

- Controllers stay focused on HTTP orchestration.
- Validators own request input rules.
- Message catalogs avoid duplicated literal strings.
- Consistent errors are easier for clients and tests.
- Validators are easier to unit test.
