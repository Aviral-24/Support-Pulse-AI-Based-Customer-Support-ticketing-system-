import { XIcon } from 'lucide-react';
import { useState } from 'react';
import toast from 'react-hot-toast';
import axios from 'axios';

export default function TicketDrawer({ ticket, isOpen, onClose, agentToken, onTicketUpdated }) {
  const [updating, setUpdating] = useState(false);
  const [newStatus, setNewStatus] = useState(ticket?.status || 'Open');
 
  if (!isOpen || !ticket) return null;

  const handleStatusUpdate = async () => {
    setUpdating(true);
    try {
      // API call to update status (PUT request)
           await axios.put(`https://aviral-supportpulse.duckdns.org/api/v1/Tickets/${ticket.id}/status`, 
          // await axios.put(`http://localhost:5215/api/v1/Tickets/${ticket.id}/status`,
         // await axios.put(`http://34.93.237.221:5215/api/v1/Tickets/${ticket.id}/status`, 
        { status: newStatus },
        { headers: { Authorization: `Bearer ${agentToken}` } }
      );
      toast.success("Status updated successfully!");
      onTicketUpdated(); // Dashboard ko refresh karne ke liye
      onClose(); // Drawer close karne ke liye
    } catch (error) {
      toast.error("Failed to update status.");
    } finally {
      setUpdating(false);
    }
  };
return (
    <div className="fixed inset-0 z-50 flex justify-end bg-gray-900 bg-opacity-50 transition-opacity">
      {/* Slide-over panel */}
      <div className="w-full max-w-md bg-white h-full shadow-2xl flex flex-col transform transition-transform duration-300 ease-in-out">
        
        {/* Header */}
        <div className="px-6 py-4 border-b border-gray-200 flex justify-between items-center bg-gray-50">
          <h2 className="text-lg font-bold text-gray-800">Ticket #{ticket.id}</h2>
          <button onClick={onClose} className="text-gray-500 hover:text-red-500">
            <XIcon className="w-6 h-6" />
          </button>
        </div>

        {/* Scrollable Content */}
        <div className="flex-1 overflow-y-auto p-6 space-y-6">
          
          {/* Info Section */}
          <div>
            <h3 className="text-xl font-semibold text-gray-900">{ticket.title}</h3>
            
            {/* AI Category & Priority Badges */}
            <div className="flex gap-2 mt-3 mb-2">
              <span className="bg-indigo-50 text-indigo-700 px-2.5 py-1 rounded-md text-xs font-bold border border-indigo-200">
                {ticket.aiCategory || ticket.AiCategory || ticket.category || 'General'}
              </span>
              <span className={`px-2.5 py-1 rounded-md text-xs font-bold border ${
                  (ticket.priority || ticket.Priority) === 'High' ? 'bg-red-50 text-red-700 border-red-200' : 
                  (ticket.priority || ticket.Priority) === 'Medium' ? 'bg-yellow-50 text-yellow-700 border-yellow-200' : 
                  'bg-green-50 text-green-700 border-green-200'
                }`}>
                {ticket.priority || ticket.Priority || 'Medium'} Priority
              </span>
            </div>

            <p className="text-gray-600 mt-3 text-sm whitespace-pre-wrap">{ticket.description}</p>
          </div>

          <div className="grid grid-cols-2 gap-4 text-sm border-t border-gray-100 pt-4">
            <div>
              <span className="text-gray-500 block mb-1">Customer ID</span>
              <span className="font-medium">{ticket.customerId || ticket.CustomerName || 'N/A'}</span>
            </div>
            <div>
              <span className="text-gray-500 block mb-1">Current Status</span>
              <span className="font-medium text-blue-600">{ticket.status}</span>
            </div>
          </div>

          {/* AI Insights Summary inside Drawer */}
          {(ticket.aiSummary || ticket.AiSummary) && (
            <div className="bg-purple-50 border border-purple-200 p-4 rounded-xl mt-4 shadow-sm">
              <h4 className="font-bold text-purple-900 mb-2 flex items-center gap-1.5 text-sm">
                <Sparkles className="w-4 h-4" /> AI Summary
              </h4>
              <p className={`text-sm ${(ticket.aiSummary || ticket.AiSummary).includes("SECURITY ALERT") ? "text-red-600 font-bold" : "text-gray-700"}`}>
                {ticket.aiSummary || ticket.AiSummary}
              </p>
            </div>
          )}

          {/* Media Files (S3 Links) */}
          <div className="border-t border-gray-100 pt-4 space-y-4">
            <h4 className="font-medium text-gray-800">Attached Files</h4>
            
            {ticket.imageUrl ? (
              <div className="rounded-lg border overflow-hidden">
                <img src={ticket.imageUrl} alt="Attachment" className="w-full h-auto object-cover" />
              </div>
            ) : (
              <p className="text-sm text-gray-400 italic">No image attached</p>
            )}

            {ticket.audioUrl && (
              <audio controls className="w-full mt-2" src={ticket.audioUrl}>
                Your browser does not support audio.
              </audio>
            )}
          </div>
        </div>

        {/* Footer with Action Buttons */}
        <div className="p-4 border-t border-gray-200 bg-gray-50">
          <label className="block text-sm font-medium text-gray-700 mb-2">Update Status</label>
          <div className="flex gap-2">
            <select 
              value={newStatus} 
              onChange={(e) => setNewStatus(e.target.value)}
              className="flex-1 border-gray-300 rounded-md shadow-sm p-2 border outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="Open">Open</option>
              <option value="In Progress">In Progress</option>
              <option value="Resolved">Resolved</option>
            </select>
            <button 
              onClick={handleStatusUpdate} 
              disabled={updating || newStatus === ticket.status}
              className="px-4 py-2 bg-blue-600 text-white rounded-md font-medium hover:bg-blue-700 disabled:bg-gray-400 transition-colors"
            >
              {updating ? 'Saving...' : 'Update'}
            </button>
          </div>
        </div>

      </div>
    </div>
  );
}