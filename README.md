# Support-Pulse 🎟️✨
**An AI-Native Full-Stack Customer Support Ticketing System**

SupportPulse is an advanced, AI-powered support ticketing system and internal knowledge base designed to streamline customer service operations. Built as the final deliverable for the **EnProSys Al-Native Full-Stack Ownership Sprint**, this system integrates modern multi-modal AI models, secure role-based access, and asynchronous processing to handle high-volume support requests autonomously.

---

## 🚀 Key Features

* **Multi-Modal Ticket Submission:** Customers can submit support tickets via text, voice notes (auto-transcribed via Whisper), or image uploads (analyzed via Vision models).

* **AI-Powered Enrichment Pipeline:** Asynchronous background workers automatically categorize, prioritize, and generate sentiment analysis (e.g., "Frustrated", "Calm") for every incoming ticket.

* **Semantic Search & RAG:** Built-in vector database (`pgvector`) allows agents to search for conceptually similar past tickets and auto-draft AI-generated replies based on historical solutions.

* **MCP (Model Context Protocol) Server:** Exposes secure analytics and PDF generation tools directly to AI clients (like Claude Desktop) for conversational data retrieval.

* **Executive Analytics Dashboard:** Real-time metrics visualized using Recharts (Donut & Bar charts) for AI sentiment breakdown and category tracking.

* **Enterprise-Grade Security:** Strict server-side RBAC (Role-Based Access Control) and complete protection against BOLA/IDOR vulnerabilities. Passwords are securely hashed.

* **High Availability & Observability:** Integrated with Sentry for crash reporting, OpenTelemetry for metrics/tracing, and Redis for queue management.

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

---

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