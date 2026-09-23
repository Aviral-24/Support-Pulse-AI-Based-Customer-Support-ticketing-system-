import { useState, useEffect } from 'react';
import axios from 'axios';
import { useAuth } from '../context/AuthContext';
import Galaxy from './Galaxy';
import { InboxIcon, Clock, CheckCircle, AlertCircle } from 'lucide-react';

export default function MyTickets() {
  const [tickets, setTickets] = useState([]);
  const [loading, setLoading] = useState(true);
  const { user } = useAuth();

  useEffect(() => {
    const fetchMyTickets = async () => {
      try {
                  const response = await axios.get('http://localhost:5215/api/v1/Tickets/my', {
                  //const response = await axios.get('http://34.93.237.221:5215/api/v1/Tickets/my', {
          headers: { 'Authorization': `Bearer ${user.token}` }
        });
        setTickets(response.data);
      } catch (error) {
        console.error("Failed to load tickets", error);
      } finally {
        setLoading(false);
      }
    };
    fetchMyTickets();
  }, [user.token]);

  const getStatusIcon = (status) => {
    switch(status) {
      case 'Resolved': return <CheckCircle className="w-5 h-5 text-green-500" />;
      case 'In Progress': return <Clock className="w-5 h-5 text-blue-500" />;
      default: return <AlertCircle className="w-5 h-5 text-yellow-500" />;
    }
  };

  const getStatusColor = (status) => {
    switch(status) {
      case 'Resolved': return 'bg-green-100 text-green-800 border-green-200';
      case 'In Progress': return 'bg-blue-100 text-blue-800 border-blue-200';
      default: return 'bg-yellow-100 text-yellow-800 border-yellow-200';
    }
  };

  return (
    <div className="relative h-[calc(100vh-4rem)] overflow-y-auto overflow-x-hidden bg-slate-100">
      <Galaxy className="opacity-60" density={1.0} glowIntensity={0.3} saturation={0.5} hueShift={180} />
      
      <div className="relative z-10 p-6 max-w-4xl mx-auto mt-6">
        <h2 className="text-3xl font-bold mb-6 text-gray-800">My Support Tickets</h2>

        {loading ? (
          <div className="text-center text-gray-600 font-medium animate-pulse mt-10">Loading your tickets...</div>
        ) : tickets.length === 0 ? (
          <div className="bg-white rounded-2xl p-10 text-center shadow-lg border border-gray-100 flex flex-col items-center justify-center">
            <div className="bg-blue-50 p-4 rounded-full mb-4">
              <InboxIcon className="w-12 h-12 text-blue-400" />
            </div>
            <h3 className="text-xl font-bold text-gray-800">No Tickets Yet</h3>
            <p className="text-gray-500 mt-2">You haven't submitted any support requests.</p>
          </div>
        ) : (
          <div className="space-y-4">
            {tickets.map(ticket => (
              <div key={ticket.id} className="bg-white/95 backdrop-blur-sm rounded-xl p-6 shadow-md border border-gray-100 hover:shadow-lg transition-shadow">
                <div className="flex justify-between items-start mb-4">
                  <div>
                    <h3 className="text-xl font-bold text-gray-800">#{ticket.id} - {ticket.title}</h3>
                    <p className="text-sm text-gray-500 font-medium mt-1">Category: {ticket.category || 'General'}</p>
                  </div>
                  <div className={`flex items-center gap-2 px-3 py-1.5 rounded-full border font-bold text-sm ${getStatusColor(ticket.status)}`}>
                    {getStatusIcon(ticket.status)}
                    {ticket.status}
                  </div>
                </div>
                
                <p className="text-gray-600 mb-4 bg-gray-50 p-3 rounded-lg text-sm border border-gray-100">
                  {ticket.description}
                </p>

                {ticket.aiSummary && (
                  <div className="mt-4 border-t border-gray-100 pt-4">
                    <p className="text-xs font-bold text-purple-600 uppercase tracking-wider mb-1">Agent Response / Summary</p>
                    <p className="text-sm text-gray-700 italic">"{ticket.aiSummary}"</p>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}