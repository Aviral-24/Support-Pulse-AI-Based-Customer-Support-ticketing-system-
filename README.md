


## Day 2: 

Authentication, RBAC & Security (Completed)Today's focus was on securing the API, implementing proper authentication, and setting up defense mechanisms against common web vulnerabilities.Key Implementations:Authentication & Identity:Implemented secure User Registration and Login APIs.  Configured JSON Web Token (JWT) Authentication for secure, stateless sessions.  Integrated BCrypt.Net-Next for strong, one-way Password Hashing (passwords are never stored in plain text).  Authorization & Access Control:Enforced Role-Based Access Control (RBAC) to differentiate between Customer, Agent, and Admin roles.  Applied Server-side authorization on required routes using the [Authorize] attribute.  Security & Vulnerability Protection:BOLA/IDOR Protection: Ensured user operations rely on the secure ClaimTypes.NameIdentifier extracted from the JWT, completely ignoring easily spoofed URL IDs.  Input Validation: Added strict server-side validation using Data Annotations (e.g., [Required], [EmailAddress]) on DTOs.  Rate Limiting: Configured a global Fixed Window rate limiter to prevent API abuse and brute-force attacks.  CORS: Configured Cross-Origin Resource Sharing (CORS) policies.  Middleware & Reliability:Built a Centralized Global Exception Middleware to catch unhandled errors gracefully, returning clean JSON responses without leaking stack traces.  Testing:Configured an xUnit test project and wrote automated Security Bypass Tests to cryptographically prove that secured endpoints reject unauthenticated (401) and unauthorized (403) requests.  




# 🚀 SupportPulse — Day 3 Implementation

## 📅 Day 3: Advanced Ticket Management & Multi-Modal Support

Day 3 of **SupportPulse** focused on implementing the core ticket-management features required for a production-ready customer support system.

The main focus was on:

* Multi-modal ticket submission
* Secure file upload and storage
* Agent-only internal notes
* Ticket lifecycle management
* Server-side pagination
* Search and filtering
* Interactive React dashboard
* Role-based authorization

---

# 🎯 Day 3 Objectives

The objective of Day 3 was to transform the basic ticket system into a more practical and scalable support platform.

### Implemented Features

| Feature                | Implementation                |
| ---------------------- | ----------------------------- |
| 📝 Text Ticket         | Customer issue description    |
| 🖼️ Image Upload       | Screenshot attachment         |
| 🎙️ Audio Upload       | Voice explanation             |
| 🔐 File Validation     | MIME type + 10 MB limit       |
| 💾 Storage Abstraction | `IStorageService`             |
| 👨‍💻 Internal Notes   | Agent/Admin only              |
| 🔄 Ticket Workflow     | Open → In Progress → Resolved |
| 🔎 Search              | Server-side keyword search    |
| 🏷️ Filtering          | Status-based filtering        |
| 📄 Pagination          | `Skip()` + `Take()`           |
| ⚛️ Interactive UI      | React + Tailwind CSS          |
| 🛡️ Authorization      | Role-based access             |

---

# 1. 🎫 Multi-Modal Ticket Submission

Customers can now submit a ticket using:

* Text description
* Screenshot/Image
* Audio/Voice explanation

Instead of only submitting text, customers can provide visual and audio context about their issue.

### Frontend

The React frontend uses `FormData` to send the ticket and attachments.

```javascript
const formData = new FormData();

formData.append("title", title);
formData.append("description", description);
formData.append("image", imageFile);
formData.append("audio", audioFile);

await axios.post("/api/tickets", formData);
```

### Backend

ASP.NET Core handles the multipart request using `IFormFile`.

```csharp
[HttpPost]
public async Task<IActionResult> CreateTicket(
    [FromForm] CreateTicketRequest request)
{
    // Ticket creation logic
}
```

### Business Benefit

Multi-modal tickets help agents understand customer problems faster.

```text
Text
 +
Screenshot
 +
Voice Explanation
       ↓
Better Context
       ↓
Faster Troubleshooting
       ↓
Reduced TAT
```

---

# 2. 🔐 Secure File Validation & Storage Abstraction

File uploads are validated on the server before being stored.

## Allowed MIME Types

### Images

```text
image/jpeg
image/png
```

### Audio

```text
audio/mpeg
audio/wav
audio/webm
```

## File Size Restriction

```text
Maximum File Size: 10 MB
```

Unsupported or oversized files are rejected by the backend.

---

## 🏗️ Storage Abstraction

Instead of directly writing files inside the controller, an abstraction was created:

```csharp
public interface IStorageService
{
    Task<string> SaveFileAsync(IFormFile file);
}
```

The local implementation:

```csharp
public class LocalStorageService : IStorageService
{
    // Local file storage implementation
}
```

Architecture:

```text
Controller
    │
    ▼
IStorageService
    │
    ▼
LocalStorageService
    │
    ▼
Local File System
```

This follows the **Dependency Inversion Principle**.

### Future Cloud Migration

The implementation can later be changed to:

```text
IStorageService
      │
      ├── LocalStorageService
      ├── S3StorageService
      └── AzureBlobStorageService
```

The controller does not need to know how the actual storage works.

---

# 3. 👨‍💻 Agent-Only Internal Notes

A separate `TicketNotes` entity was implemented for internal agent communication.

```text
Ticket
   │
   └── TicketNotes
          ├── Agent
          ├── Note
          └── CreatedAt
```

Agents can record:

* Troubleshooting steps
* Investigation results
* Escalation information
* Internal discussions

---

## 🛡️ Role-Based Protection

Internal notes are protected using:

```csharp
[Authorize(Roles = "Agent,Admin")]
```

Therefore:

```text
Customer
   ❌ Cannot access internal notes

Agent
   ✅ Can access internal notes

Admin
   ✅ Can access internal notes
```

### Business Benefit

Internal discussions remain confidential and are not exposed to customers.

---

# 4. 🔄 Ticket Lifecycle Workflow

A strict ticket lifecycle was implemented:

```text
┌─────────┐
│  Open   │
└────┬────┘
     │
     ▼
┌─────────────┐
│ In Progress │
└──────┬──────┘
       │
       ▼
┌──────────┐
│ Resolved │
└──────────┘
```

### Purpose

This provides a predictable workflow for support teams.

```text
Customer creates ticket
        ↓
       Open
        ↓
Agent starts working
        ↓
   In Progress
        ↓
Issue fixed
        ↓
     Resolved
```

---

# 5. 📊 Server-Side Pagination

Ticket listing uses **Entity Framework Core + IQueryable**.

Instead of loading every ticket into application memory, pagination is performed at the database-query level.

Example:

```csharp
var tickets = await query
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

For example:

```text
Database
100,000 Tickets
      │
      ▼
Search / Filter
      │
      ▼
Skip()
      │
      ▼
Take(10)
      │
      ▼
Frontend
10 Tickets
```

### Benefits

* Reduced memory usage
* Lower network bandwidth
* Faster response time
* Better database efficiency
* Better frontend performance
* Improved scalability

---

# 6. 🔎 Server-Side Search & Filtering

Users can search tickets using keywords.

Example:

```text
Search: payment
```

The backend performs the filtering instead of downloading all tickets to the browser.

Status filtering is also supported:

```text
All
Open
In Progress
Resolved
```

Flow:

```text
React Search / Filter
        ↓
API Request
        ↓
IQueryable
        ↓
Database
        ↓
Filtered Results
        ↓
React Dashboard
```

---

# 7. ⚛️ Interactive React Dashboard

The frontend was implemented using:

* React.js
* Tailwind CSS
* JavaScript
* HTML5

The dashboard dynamically updates when the user changes search or filter values.

### Implemented UI Features

* 🔎 Search
* 🏷️ Status filter
* 📄 Pagination
* 🪟 Ticket details modal
* 🖼️ Image preview
* 🎙️ Audio playback
* 📝 Internal notes
* 🔄 Status update

---

# 8. 🖼️ Ticket Detail Modal

Agents can open a ticket and view all relevant information without leaving the dashboard.

```text
┌──────────────────────────────────┐
│         Ticket Details           │
├──────────────────────────────────┤
│ Title                            │
│ Description                      │
│                                  │
│ Screenshot                       │
│ [ Image Preview ]                │
│                                  │
│ Audio                            │
│ ▶ ─────────────── 🔊             │
│                                  │
│ Internal Notes                   │
│ [ Add Note ]                     │
│                                  │
│ Status: [ In Progress ▼ ]        │
└──────────────────────────────────┘
```

The audio file is played using the native HTML5:

```html
<audio controls>
    <source src={audioUrl} />
</audio>
```

Images are displayed using:

```html
<img src={imageUrl} alt="Ticket attachment" />
```

No heavy external media library is required.

---

# 🛡️ Security Implementation

Day 3 introduced several server-side security controls.

### File Security

```text
✅ JPEG
✅ PNG
✅ MP3
✅ WAV
✅ WebM

❌ EXE
❌ SH
❌ Scripts
❌ Unsupported file types
```

### Size Security

```text
Maximum file size = 10 MB
```

### Authorization

```csharp
[Authorize(Roles = "Agent,Admin")]
```

### Storage Security

Uploaded files receive unique GUID-based names.

```text
Original:
screenshot.png

Stored:
8f3d2b9e-5a72-4a4e-a8f9-123456789abc.png
```

---

# 🏗️ Day 3 Architecture

```text
                     SUPPORTPULSE
                          │
             ┌────────────┴────────────┐
             │                         │
       React Frontend             ASP.NET Core
             │                         │
             │                    Controllers
             │                         │
             │                    Services
             │                         │
             │                 ┌───────┴───────┐
             │                 │               │
             │            Ticket Service   Storage Service
             │                 │               │
             │                 ▼               ▼
             │              EF Core       Local Storage
             │                 │
             │                 ▼
             │            SQL Server
             │
             └──── API / FormData ──────┘
```

---

# 🧰 Technology Stack

## Frontend

* React.js
* Tailwind CSS
* JavaScript
* HTML5
* FormData
* HTML5 Audio

## Backend

* C#
* ASP.NET Core Web API
* Entity Framework Core
* LINQ
* `IQueryable`
* `IFormFile`
* Dependency Injection
* Role-Based Authorization

## Database

* Microsoft SQL Server

## Storage

* Local File Storage
* `IStorageService`

---

# 📂 Day 3 Implementation Structure

```text
SupportPulse/
│
├── Backend/
│   ├── Controllers/
│   │   ├── TicketsController.cs
│   │   └── TicketNotesController.cs
│   │
│   ├── Models/
│   │   ├── Ticket.cs
│   │   └── TicketNote.cs
│   │
│   ├── DTOs/
│   │   └── CreateTicketRequest.cs
│   │
│   ├── Services/
│   │   ├── IStorageService.cs
│   │   ├── LocalStorageService.cs
│   │   └── TicketService.cs
│   │
│   └── Data/
│       └── ApplicationDbContext.cs
│
└── Frontend/
    └── src/
        ├── components/
        ├── pages/
        ├── services/
        └── App.jsx
```

---

# 📈 Day 3 Improvements

Before Day 3:

```text
Basic Ticket
     ↓
Text Only
     ↓
Simple Ticket List
```

After Day 3:

```text
                  SupportPulse
                       │
        ┌──────────────┼──────────────┐
        │              │              │
       Text          Image          Audio
        │              │              │
        └──────────────┼──────────────┘
                       ↓
                Secure Storage
                       ↓
                Ticket Management
                       ↓
          Search + Filter + Pagination
                       ↓
              Agent Dashboard
                       ↓
             Internal Collaboration
```

---

# 🎤 Day 3 — 30 Second Interview Pitch

> **"On Day 3, I implemented the core business features of SupportPulse. I built a multi-modal ticket system where customers can submit text along with image and audio attachments. On the ASP.NET Core backend, I implemented multipart form-data handling, server-side file validation with MIME type and 10 MB size restrictions, and storage abstraction using IStorageService. I also implemented a controlled ticket lifecycle, confidential agent-only internal notes with role-based authorization, and a server-side paginated dashboard using Entity Framework Core IQueryable, Skip and Take. On the frontend, I built an interactive React and Tailwind dashboard with search, filtering, media preview, audio playback, status updates, and modal-based ticket details."**

---

# ✅ Day 3 Completion Checklist

* [x] Multi-modal ticket submission
* [x] Text attachment support
* [x] Image upload
* [x] Audio upload
* [x] Multipart/form-data
* [x] `IFormFile`
* [x] MIME type validation
* [x] 10 MB file size validation
* [x] `IStorageService`
* [x] `LocalStorageService`
* [x] GUID-based file naming
* [x] TicketNotes entity
* [x] Agent/Admin authorization
* [x] Ticket lifecycle
* [x] Server-side search
* [x] Server-side filtering
* [x] Server-side pagination
* [x] `IQueryable`
* [x] `Skip()` / `Take()`
* [x] React dashboard
* [x] Tailwind CSS
* [x] Ticket detail modal
* [x] Image preview
* [x] Audio playback

---

# 🏁 Day 3 Summary

**Day 3 transformed SupportPulse from a basic ticketing system into a more scalable and production-oriented customer support platform.**

The implementation focused on:

> **Better Customer Experience + Secure Backend + Scalable Queries + Agent Collaboration + Clean Architecture**



## Day - 4

## AI-Powered Ticket Enrichment Pipeline

## Asynchronous Queuing (Channel<T>): 
Customer ticket submissions are immediately saved to PostgreSQL and pushed to a thread-safe, non-blocking in-memory queue, ensuring instant API responses.

## Background Worker (BackgroundService):
 A dedicated worker thread (AiEnrichmentWorker) picks up queued tickets asynchronously to process them without impacting server performance.

##  Groq LLM Integration:
 The worker securely calls the Groq AI API (openai/gpt-oss-120b) via IHttpClientFactory to analyze the ticket description, generating a crisp 1-sentence summary and a sentiment classification (Happy, Angry, Neutral).

## Database Persistence: 
The processed AI summary and sentiment are automatically updated in the PostgreSQL database using Entity Framework Core.

## Agent Dashboard UI:
 The React frontend fetches and displays these insights in a dedicated 🤖 AI Insights card with dynamic color-coded sentiment badges inside the ticket details modal.

## Fault Tolerance & Resilience: 
Built-in exception handling ensures that external API failures or network timeouts log gracefully without crashing the core backend server.




## Day-5

Support-Pulse: AI-Powered Enterprise Helpdesk System

Core Tech Stack

Backend: ASP.NET Core, Entity Framework Core, PostgreSQL, JWT Authentication.

Frontend: React, Tailwind CSS, Recharts.

Reporting & AI: QuestPDF, LLM Integration (Groq/Gemini), MCP (Model Context Protocol).

Key Features Implemented & How They Work

Public Ticket Submission: Customers can submit support tickets containing titles, descriptions, and optional file attachments (images/audio). Data is processed via REST APIs and queued asynchronously.

##  AI Enrichment Engine:
 A background worker automatically analyzes incoming tickets using an LLM to generate concise summaries and classify customer sentiment (Happy, Neutral, Angry).

##  Automated PDF Generation: 
Utilizes the QuestPDF library to instantly generate professional, downloadable summary reports containing ticket details and embedded AI insights.

##  Analytics Engine:
 Provides a real-time data aggregation API endpoint that computes key metrics, including total/open/resolved counts, category breakdowns, and sentiment distributions.

##  Audit Trail System: 
Tracks and securely records critical user actions (such as status updates, PDF downloads, and tool executions) in a dedicated PostgreSQL database table with timestamps and user IDs.

##  MCP (Model Context Protocol) Server:
 Implements a secure JSON-RPC endpoint that exposes internal backend capabilities (get_ticket_analytics, generate_pdf_summary) as callable tools for external AI models.

##  React Frontend & Visual Dashboard:
 A responsive agent interface built with Tailwind CSS featuring real-time ticket management, pagination, search/filtering, direct PDF download triggers, and Recharts-powered data visualization.

## Support-Pulse ek modern aur AI-powered enterprise helpdesk system hai jise ek robust tech stack ke sath banaya gaya hai. Backend par ASP.NET Core, Entity Framework Core, PostgreSQL, aur JWT Authentication ka use kiya gaya hai, jabki frontend ke liye Tailwind CSS aur Recharts ke sath React ka istemal hua hai. Is platform ki shuruaat ek public ticket submission system se hoti hai jahan customers apne issues, titles, descriptions, aur optional attachments (jaise images ya audio files) upload kar sakte hain. In tickets ke submit hote hi ek background AI enrichment engine active ho jata hai jo LLM ki madad se ticket ke content ko analyze karta hai, ek concise summary banata hai, aur customer ka sentiment (Happy, Neutral, Angry) automatically classify kar deta hai.  Platform ke andar reporting aur enterprise management ko strong banane ke liye QuestPDF library ko integrate kiya gaya hai, jisse agents kisi bhi ticket ki professional PDF summary report ek click par download kar sakte hain. Iske alawa, ek real-time Analytics Engine banaya gaya hai jo database se data aggregate karke total, open, aur resolved tickets ke counts ke sath-sath category aur sentiment ka breakdown provide karta hai, jise React frontend par Recharts ki madad se interactive pie charts aur bar graphs ke roop me visualize kiya jata hai. Security aur accountability ko maintain karne ke liye ek dedicated Audit Trail system implement kiya gaya hai jo har critical action—jaise status update karna ya PDF download karna—ko timestamp aur user ID ke sath database me securely log karta hai. Aakhir me, is system me ek advanced Model Context Protocol (MCP) server bhi add kiya gaya hai, jo secure JSON-RPC endpoints ke zariye internal backend capabilities (get_ticket_analytics aur generate_pdf_summary) ko external AI models ke liye callable tools ke taur par expose karta hai.




## Day - 6

Key Implementation Highlights

Docker & Docker Compose Orchestration: Built multi-stage Dockerfiles for the .NET 8 backend and React/Vite frontend (served via Nginx), containerizing the full application alongside PostgreSQL with persistent data volumes.

Role-Based Access Control (RBAC): Enforced strict authorization policies across controllers to secure endpoints, separating permissions for Customer ticket creation from Agent/Admin management workflows.

Resilient Startup & Auto-Migration: Programmatically integrated Entity Framework Core database migrations (context.Database.Migrate()) inside Program.cs equipped with a retry backoff loop to handle container startup race conditions.

Asynchronous AI Integration: Validated background queue processing (AiEnrichmentWorker) that successfully coordinates containerized ticket ingestion with external AI enrichment.

Detailed Overview for README

Today's development focused on full-stack containerization, security enforcement, and database reliability for the Support-Pulse platform. The application has been fully containerized using Docker and Docker Compose, featuring optimized multi-stage builds for the .NET 8 backend and a high-performance Nginx web server hosting the React frontend alongside a managed PostgreSQL database container. To ensure enterprise-grade security, Role-Based Access Control (RBAC) was strictly implemented across all backend API controllers, ensuring that users can only access endpoints matching their assigned permissions (such as customers submitting tickets and agents managing dashboards or downloading reports). Furthermore, to solve common container startup synchronization issues between the application layer and the database, a robust auto-migration mechanism with built-in connection retries was embedded directly into the application startup pipeline. This ensures that database schemas are provisioned cleanly and reliably upon deployment without manual intervention, finalizing a scalable, observable, and production-ready architecture.

What is Day 6?
Day 6 transforms the Support-Pulse application into a production-ready, highly observable, and fully tested enterprise system with robust container infrastructure.

Core Implementations & How They Work

Advanced Observability (OpenTelemetry & Sentry):

OpenTelemetry captures distributed traces and performance metrics for all incoming HTTP requests and external API calls (like Groq AI).

Sentry is integrated to automatically catch unhandled exceptions, application crashes, and background worker errors in real-time.

Production-Grade Redis Queue:

Upgraded the background job processing from a volatile in-memory channel to a persistent Redis Queue using StackExchange.Redis (ListLeftPushAsync / ListRightPopAsync).

Ensures pending support tickets for AI enrichment survive service restarts while tracking live custom metrics like queue depth (ticket_queue_depth) and processing latency.

Automated Testing Suite (xUnit & Security):

Built a comprehensive test project using xUnit and Moq to validate isolated business logic, queue behaviors, and background workflows.

Implemented security integration tests to verify Role-Based Access Control (RBAC), blocking unauthorized users and role-tampering exploits (REST and MCP bypass tests).

Full-Stack Containerization & Resilient Startup:

Orchestrated the entire stack—PostgreSQL, Redis, .NET 8 Backend, Background Worker, and Nginx-served React Frontend—using Docker Compose.

Embedded a programmatic auto-migration retry loop inside Program.cs to handle container startup race conditions gracefully when connecting to PostgreSQL.



##### Day - 7

