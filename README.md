<img width="500" height="550" alt="image" src="https://github.com/user-attachments/assets/46455943-7da7-432f-b42b-b912e5f5fd48" />




Summary:

This is a well-structured three-tier application using Repository, Service Layer, and Dependency Injection patterns as the core architectural patterns. It demonstrates a hybrid approach with modern .NET 8 for APIs and .NET Framework for the web application, communicating via HTTP/REST services. The architecture promotes SOLID principles, particularly Dependency Inversion and Single Responsibility.

-------------------------------------------------------


Design Patterns:

a. Repository

b. Generic Repository

c. Dependency Injection

d. DTO

e. Service Layer

f. Facade

g. Singleton



Architecture:
Layered/N-Tier Architecture (3-Tier) with Client-Server and Service-Oriented characteristics:



•  Presentation Layer: ASP.NET MVC (.NET Framework 4.8.1) web application



•  Business Logic Layer: ASP.NET Core Web API (.NET 8) with RESTful endpoints



•  Data Access Layer: Shared DataContext library (.NET Framework 4.8) with in-memory storage



Separation of Concerns:
Clear boundaries between Presentation, Business Logic, and Data Access layers



Technology Stack:



•  Web: ASP.NET MVC (.NET Framework 4.8.1) with Unity DI



•  API: ASP.NET Core Web API (.NET 8) with built-in DI



•  Data: Shared library (.NET Framework 4.8) with in-memory storage

