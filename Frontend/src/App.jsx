import { BrowserRouter as Router, Routes, Route, Link, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import TicketForm from './components/TicketForm';
import AgentDashboard from './components/AgentDashboard';
import AnalyticsDashboard from './components/AnalyticsDashboard';
import Login from './components/Login';
import Register from './components/Register';
import MyTickets from './components/MyTickets'; // 🔥 Naya import
import { Toaster } from 'react-hot-toast';

const ProtectedRoute = ({ children, allowedRoles }) => {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" />;
  if (allowedRoles && !allowedRoles.includes(user.role)) return <Navigate to="/" />;
  return children;
};

const Navbar = () => {
  const { user, logout } = useAuth();
  return (
    <nav className="bg-gray-900 text-white p-4 px-8 flex justify-between items-center shadow-lg relative z-50">
      <div className="font-bold text-xl tracking-wide flex items-center gap-2">
        <span className="text-blue-400">⚡</span> SupportPulse
      </div>
      <div className="flex gap-6 items-center font-medium">
        {user ? (
          <>
            {user.role === 'Customer' && (
              <>
                <Link to="/" className="hover:text-blue-400 transition">Submit Ticket</Link>
                {/* 🔥 NAYA LINK ADD KIYA */}
                <Link to="/my-tickets" className="hover:text-blue-400 transition">My Tickets</Link>
              </>
            )}
            
            {(user.role === 'Agent' || user.role === 'Admin') && (
              <>
                <Link to="/dashboard" className="hover:text-blue-400 transition">Agent Dashboard</Link>
                <Link to="/analytics" className="hover:text-blue-400 transition">Analytics</Link>
              </>
            )}
            
            <button onClick={logout} className="ml-4 bg-red-500 hover:bg-red-600 text-white px-4 py-1.5 rounded-lg transition shadow-sm">
              Logout
            </button>
          </>
        ) : (
          <>
            <Link to="/login" className="hover:text-blue-400 transition">Login</Link>
            <Link to="/register" className="bg-blue-600 hover:bg-blue-700 px-4 py-1.5 rounded-lg transition shadow-sm">Register</Link>
          </>
        )}
      </div>
    </nav>
  );
};

function App() {
  return (
    <Router>
      <AuthProvider>
        <div className="min-h-screen bg-gray-50 flex flex-col">
          <Navbar />
          <div className="flex-grow">
            <Routes>
              <Route path="/login" element={<Login />} />
              <Route path="/register" element={<Register />} />
              
              <Route path="/" element={<ProtectedRoute allowedRoles={['Customer']}><TicketForm /></ProtectedRoute>} />
              {/* 🔥 NAYA ROUTE ADD KIYA */}
              <Route path="/my-tickets" element={<ProtectedRoute allowedRoles={['Customer']}><MyTickets /></ProtectedRoute>} />
              
              <Route path="/dashboard" element={<ProtectedRoute allowedRoles={['Agent', 'Admin']}><AgentDashboard /></ProtectedRoute>} />
              <Route path="/analytics" element={<ProtectedRoute allowedRoles={['Agent', 'Admin']}><AnalyticsDashboard /></ProtectedRoute>} />
            </Routes>
          </div>
          <Toaster position="top-right" />
        </div>
      </AuthProvider>
    </Router>
  );
}

export default App;