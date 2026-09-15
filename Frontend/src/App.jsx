// import { BrowserRouter as Router, Routes, Route, Link, Navigate } from 'react-router-dom';
// import { AuthProvider, useAuth } from './context/AuthContext';
// import TicketForm from './components/TicketForm';
// import AgentDashboard from './components/AgentDashboard';
// import AnalyticsDashboard from './components/AnalyticsDashboard';
// import Login from './components/Login';
// import Register from './components/Register';
// import MyTickets from './components/MyTickets'; // 🔥 Naya import
// import { Toaster } from 'react-hot-toast';

// const ProtectedRoute = ({ children, allowedRoles }) => {
//   const { user } = useAuth();
//   if (!user) return <Navigate to="/login" />;
//   if (allowedRoles && !allowedRoles.includes(user.role)) return <Navigate to="/" />;
//   return children;
// };

// const Navbar = () => {
//   const { user, logout } = useAuth();
//   return (
//     <nav className="bg-gray-900 text-white p-4 px-8 flex justify-between items-center shadow-lg relative z-50">
//       <div className="font-bold text-xl tracking-wide flex items-center gap-2">
//         <span className="text-blue-400">⚡</span> SupportPulse
//       </div>
//       <div className="flex gap-6 items-center font-medium">
//         {user ? (
//           <>
//             {user.role === 'Customer' && (
//               <>
//                 <Link to="/" className="hover:text-blue-400 transition">Submit Ticket</Link>
//                 {/* 🔥 NAYA LINK ADD KIYA */}
//                 <Link to="/my-tickets" className="hover:text-blue-400 transition">My Tickets</Link>
//               </>
//             )}
            
//             {(user.role === 'Agent' || user.role === 'Admin') && (
//               <>
//                 <Link to="/dashboard" className="hover:text-blue-400 transition">Agent Dashboard</Link>
//                 <Link to="/analytics" className="hover:text-blue-400 transition">Analytics</Link>
//               </>
//             )}
            
//             <button onClick={logout} className="ml-4 bg-red-500 hover:bg-red-600 text-white px-4 py-1.5 rounded-lg transition shadow-sm">
//               Logout
//             </button>
//           </>
//         ) : (
//           <>
//             <Link to="/login" className="hover:text-blue-400 transition">Login</Link>
//             <Link to="/register" className="bg-blue-600 hover:bg-blue-700 px-4 py-1.5 rounded-lg transition shadow-sm">Register</Link>
//           </>
//         )}
//       </div>
//     </nav>
//   );
// };

// function App() {
//   return (
//     <Router>
//       <AuthProvider>
//         <div className="min-h-screen bg-gray-50 flex flex-col">
//           <Navbar />
//           <div className="flex-grow">
//             <Routes>
//               <Route path="/login" element={<Login />} />
//               <Route path="/register" element={<Register />} />
              
//               <Route path="/" element={<ProtectedRoute allowedRoles={['Customer']}><TicketForm /></ProtectedRoute>} />
//               {/* 🔥 NAYA ROUTE ADD KIYA */}
//               <Route path="/my-tickets" element={<ProtectedRoute allowedRoles={['Customer']}><MyTickets /></ProtectedRoute>} />
              
//               <Route path="/dashboard" element={<ProtectedRoute allowedRoles={['Agent', 'Admin']}><AgentDashboard /></ProtectedRoute>} />
//               <Route path="/analytics" element={<ProtectedRoute allowedRoles={['Agent', 'Admin']}><AnalyticsDashboard /></ProtectedRoute>} />
//             </Routes>
//           </div>
//           <Toaster position="top-right" />
//         </div>
//       </AuthProvider>
//     </Router>
//   );
// }

// export default App;


import React from 'react';
import { Routes, Route, Link, Navigate } from 'react-router-dom'; // 🟢 Yahan se BrowserRouter hata diya
import { Toaster } from 'react-hot-toast';
import { useAuth } from './context/AuthContext'; 

// Components
import Login from './components/Login';
import Register from './components/Register';
import TicketForm from './components/TicketForm';
import MyTickets from './components/MyTickets';
import AgentDashboard from './components/AgentDashboard';
import AnalyticsDashboard from './components/AnalyticsDashboard';

function App() {
  const { user, logout } = useAuth(); 

  const isAgent = user?.role === 'Agent' || user?.role === 'Admin';
  const isCustomer = user?.role === 'Customer';

  return (
    <> {/* 🟢 BrowserRouter ki jagah khali fragment <> laga diya */}
      <Toaster position="top-right" /> 

      {user && (
        <nav className="bg-slate-900 text-white p-4 flex justify-between items-center shadow-lg">
          <div className="font-bold text-xl tracking-wider">SupportPulse</div>
          
          <div className="space-x-6 flex items-center">
            {isCustomer && (
              <>
                <Link to="/submit-ticket" className="hover:text-blue-400 font-medium transition">Submit Ticket</Link>
                <Link to="/my-tickets" className="hover:text-blue-400 font-medium transition">My Tickets</Link>
              </>
            )}

            {isAgent && (
              <>
                <Link to="/agent-dashboard" className="hover:text-blue-400 font-medium transition">Agent Dashboard</Link>
                <Link to="/analytics" className="hover:text-blue-400 font-medium transition">Analytics</Link>
              </>
            )}
            
            <button onClick={logout} className="ml-4 bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded-lg font-semibold transition">
              Logout
            </button>
          </div>
        </nav>
      )}

      <div>
        <Routes>
          <Route path="/login" element={!user ? <Login /> : <Navigate to="/" />} />
          <Route path="/register" element={!user ? <Register /> : <Navigate to="/" />} />

          <Route path="/submit-ticket" element={isCustomer ? <TicketForm /> : <Navigate to="/login" />} />
          <Route path="/my-tickets" element={isCustomer ? <MyTickets /> : <Navigate to="/login" />} />

          <Route path="/agent-dashboard" element={isAgent ? <AgentDashboard /> : <Navigate to="/login" />} />
          <Route path="/analytics" element={isAgent ? <AnalyticsDashboard /> : <Navigate to="/login" />} />

          <Route path="/" element={
            !user ? <Navigate to="/login" /> : 
            isAgent ? <Navigate to="/agent-dashboard" /> : 
            <Navigate to="/submit-ticket" />
          } />
        </Routes>
      </div>
    </>
  );
}

export default App;