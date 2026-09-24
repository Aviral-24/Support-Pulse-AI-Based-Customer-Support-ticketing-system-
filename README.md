# Support-Pulse 

***An AI-Native Full-Stack Customer Support Ticketing System***

Support-Pulse is an intelligent, multi-modal support platform that goes beyond traditional ticketing. Customers can raise issues via text, voice notes, or screenshots. The system leverages an asynchronous AI pipeline (powered by Groq, Whisper, and Vision models) to automatically transcribe audio, extract error codes from images, classify ticket urgency, analyze sentiment, and draft RAG-grounded responses for support agents.

---

## 🚀 Key Features

* **Multi-Modal Ticket Submission:** Customers can submit support tickets via text, voice notes (auto-transcribed via Whisper), or image uploads (analyzed via Vision models).

* **AI-Powered Enrichment Pipeline:** Asynchronous background workers automatically categorize, prioritize, and generate sentiment analysis (e.g., "Frustrated", "Calm") for every incoming ticket.

* **Semantic Search & RAG:** Built-in vector database (`pgvector`) allows agents to search for conceptually similar past tickets and auto-draft AI-generated replies based on historical solutions.

* **MCP (Model Context Protocol) Server:** Exposes secure analytics and PDF generation tools directly to AI clients (like Claude Desktop) for conversational data retrieval.

* **Executive Analytics Dashboard:** Real-time metrics visualized using Recharts (Donut & Bar charts) for AI sentiment breakdown and category tracking.

* **Enterprise-Grade Security:** Strict server-side RBAC (Role-Based Access Control) and complete protection against BOLA/IDOR vulnerabilities. Passwords are securely hashed.

* **High Availability & Observability:** Integrated with Sentry for crash reporting, OpenTelemetry for metrics/tracing, and Redis for queue management.

## 🧠 System Architecture & AI Flow

## Ingestion: 
User submits a ticket (Text/Image/Audio). Uploads are securely stored in MinIO (S3).

## Queueing: 
Ticket ID is pushed to a Redis Queue for non-blocking execution.

## AI Processing:
 A .NET Background Worker dequeues the ticket and routes it:

Audio -> Transcribed via Whisper.

Image -> Analyzed via Qwen/Llama Vision API.

Text -> Classified & Sentiment analyzed via Groq LLM.

## Knowledge Retrieval: 
System converts context into embeddings via HuggingFace and queries pgvector to draft an initial agent response.

## Resolution: 
Agent reviews AI insights on the React Dashboard and resolves the ticket

---

## 🛠️ Technology Stack

### **Frontend:**
* React.js (Vite)
* Tailwind CSS (Styling)
* Recharts (Data Visualization)
* Axios (API Client)

**Backend:**
* C# .NET 8 (Web API)
* Entity Framework Core
* Background Services (Hosted Workers)

**Database & Infrastructure:**
* PostgreSQL (Relational Data)
* pgvector (Vector Embeddings - 384 dimensions)
* Redis (Message Queue & Caching)
* MinIO / Amazon S3 (Object Storage for audio/images)
* Docker & Docker Compose (Containerization)

**AI & Third-Party APIs:**
* Groq / LLaMA (Fast inference for summarization & drafting)
* HuggingFace Sentence-Transformers (Embeddings)
* Whisper (Speech-to-Text)


**Deployment And Tesing:**
* GCP: project is configured for cloud deployment (e.g., Google Cloud Platform - Compute Engine VM) using Docker Compose
* CI/CD : Github Action
* Load Testing:  K6 tool
* Unit test 

---
## ALL Backend API
#### Auth & Health (Public/No Token Required)

## POST /api/v1/Auth/login:
 User login karke JWT token generate karne ke liye. (Token: None)

## POST /api/v1/Auth/register: 
Naya account (Customer/Agent) banane ke liye. (Token: None)

## GET /api/v1/Health: 
Server, Database aur AI pipeline ka live status check karne ke liye. (Token: None)

#### 2. Profile (User Info)

## GET /api/v1/Profile/me: 
Logged-in user ki apni details (Role, Name) fetch karne ke liye. (Token: Any - Customer, Admin)

## GET /api/v1/Profile/agent-only: 
Specific endpoint jo sirf agents ka access verify karta hai. (Token: Admin)

#### 3. Tickets (Core Ticketing System)

## POST /api/v1/Tickets:
 Naya support ticket (text, audio, image) submit karne ke liye. (Token: Customer / Public)

## GET /api/v1/Tickets:
 Agent Dashboard par saari tickets ki list (with pagination & filters) load karne ke liye. (Token: Admin)

## GET /api/v1/Tickets/my: 
Customer dwara sirf khud ki submit ki hui tickets dekhne ke liye. (Token: Customer)

## GET /api/v1/Tickets/{id}:
 Kisi ek specific ticket ka pura data (images, AI remarks) dekhne ke liye. (Token: Customer [Owner], Admin)

## PUT /api/v1/Tickets/{id}/status: 
Ticket ka status (Open -> In Progress -> Resolved) update karne ke liye. (Token: Admin)

## POST /api/v1/Tickets/{id}/notes: 
Ticket par secret internal notes add karne ke liye (jo customer ko na dikhe). (Token: Admin)

## GET /api/v1/Tickets/{id}/pdf: 
Ek single ticket ka detailed PDF report generate karke download karne ke liye. (Token: Admin)

## GET /api/v1/Tickets/semantic-search:
 Tickets me AI ke through natural language me search karne ke liye (e.g., "angry billing users"). (Token: Admin)

## GET /api/v1/Tickets/{id}/force-ai-test:
 Kisi ticket par AI pipeline (Vision, Sentiment) forcefully wapas run karne ke liye. (Token:  Admin)

 ## POST /api/v1/Tickets/{id}/draft-reply: 
 Knowledge Base data ka use karke AI dwara auto-reply draft generate karne ke liye. (Token:Admin)



#### 4. KnowledgeBase (AI RAG Setup)

## POST /api/v1/KnowledgeBase: 
AI pipeline (RAG) ke liye naya FAQ (Question-Answer) add aur vectorize (pgvector) karne ke liye. (Token: Admin Only)

## DELETE /api/v1/KnowledgeBase/{id}:
 Purana ya galat FAQ knowledge base se remove karne ke liye. (Token: Admin Only)




#### 5. MCP (Model Context Protocol)

## POST /api/v1/mcp/tools/get_ticket_analytics:
 AI chat client (LLM) ko conversational analytics query ka answer dene ke liye. (Token: Admin)

## POST /api/v1/mcp/tools/generate_pdf_summary:
 AI client ke request par on-demand PDF report generate karne ke liye. (Token:Admin)



#### 6. Analytics (Dashboard Stats)

## GET /api/v1/Analytics/dashboard: 
Agent UI ke liye top-level stats (Total, Resolved, Pending tickets) return karne ke liye. (Token: Admin)





## 🔒 Security Highlights
* **Zero Trust Architecture:** Every API route and MCP tool independently verifies user roles (Customer vs. Agent/Admin) via JWT.

* **No BOLA/IDOR:** Strict ownership checks prevent users from accessing or modifying tickets they do not own.

* **Data Protection:** System fields (Status, Priority, AI Sentiment) cannot be manipulated by client-side payloads.

---

## ⚙️ Local Setup & Installation

### Prerequisites
* Docker Desktop installed and running
* Git installed
* API Keys (Groq/OpenAI, HuggingFace)

### Steps to Run

1. **Clone the repository:**
   ```bash
   git clone [https://github.com/Aviral-24/Support-Pulse-AI-Based-Customer-Support-ticketing-system-](https://github.com/Aviral-24/Support-Pulse-AI-Based-Customer-Support-ticketing-system-)
   cd SupportPulse
Environment Variables:
Update the appsettings.json in the Backend folder or set up a .env file with your respective AI API keys and JWT Secrets.

## Build and Spin up containers:

Bash
docker compose up -d --build
This command will spin up the Frontend, Backend, PostgreSQL (with pgvector), Redis, and MinIO containers.

Access the Application:

## Frontend UI: http://localhost:5173

## Backend Swagger API: http://localhost:5215/swagger

## MinIO Console: http://localhost:9001 (Default login: minioadmin / minioadmin)

## 🧪 Testing
Load Testing: Verified to securely handle up to 1000 concurrent users with 99%+ success rate using k6.

Automated Testing: Core logic and security rules (RBAC/BOLA) are covered using xUnit and Moq.

## To run tests locally:

Bash
cd Unit_Tests
dotnet test
🚢 Deployment
The project is configured for cloud deployment (e.g., Google Cloud Platform - Compute Engine VM) using Docker Compose.

Set up a GCP VM (Ubuntu).

## Ensure Firewall rules permit inbound traffic on ports 5173 (Web) and 5215 (API).

Pull the repository and run docker compose up -d --build.


## 🧪 Testing & Reliability

## Unit & Integration Testing: 
Core logic, state transitions, and critical security rules (RBAC/BOLA bypass attempts) are covered using xUnit and Moq.

## Load Testing: 
Verified to securely handle up to 1000 concurrent users with a 99%+ success rate using k6.


## 🚢 Deployment

The project is fully containerized and production-ready for deployment on cloud virtual machines (e.g., Google Cloud Platform - Compute Engine VM).

Set up a Linux VM (Ubuntu) on GCP.

Ensure Cloud Firewall rules permit inbound traffic on ports 80/443 (Web) and 5215 (API).

Pull the repository, configure the production .env file, and run:


## sudo docker compose -f docker-compose.prod.yml up -d --build