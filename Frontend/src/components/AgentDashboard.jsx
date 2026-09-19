// // import { useState, useEffect } from 'react';
// // import axios from 'axios';
// // import toast from 'react-hot-toast'; // Toast import kiya
// // import Galaxy from './Galaxy';
// // import { useAuth } from '../context/AuthContext';

// // export default function AgentDashboard() {
// //   const [tickets, setTickets] = useState([]);
// //   const [search, setSearch] = useState('');
// //   const [statusFilter, setStatusFilter] = useState('');
// //   const [page, setPage] = useState(1);
// //   const [selectedTicket, setSelectedTicket] = useState(null);
// //   const [note, setNote] = useState('');
// //   const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  
// //   // IMPORTANT: Testing ke liye apna Agent Token
// //   //const agentToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJyYWh1bEB0ZXN0LmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkN1c3RvbWVyIiwiZXhwIjoxNzkxNjU2MDY0LCJpc3MiOiJTdXBwb3J0UHVsc2VBUEkiLCJhdWQiOiJTdXBwb3J0UHVsc2VDbGllbnQifQ.dhvvJAX2u5dOBp8rhr5panME9ROCaw1vOMM0v6a9VDg";
// //   const { user } = useAuth();

// //   useEffect(() => {
// //     fetchTickets();
// //   }, [search, statusFilter, page]);

// //   const fetchTickets = async () => {
// //     try {
// //       const response = await axios.get('http://localhost:5215/api/v1/Tickets', {
// //         headers: { 'Authorization': `Bearer ${user.token}` },
// //         params: { search, status: statusFilter, page, pageSize: 10 }
// //       });
// //       setTickets(response.data.tickets);
// //     } catch (error) {
// //       console.error("Error fetching tickets", error);
// //       toast.error("Failed to load tickets.");
// //     }
// //   };

// //   const updateStatus = async (id, newStatus) => {
// //     try {
// //       await axios.put(`http://localhost:5215/api/v1/Tickets/${id}/status`, 
// //         { status: newStatus },
// //         { headers: { 'Authorization': `Bearer ${user.token}` } }
// //       );
// //       toast.success(`Ticket #${id} status updated to ${newStatus}`);
// //       fetchTickets();
// //     } catch (error) {
// //       toast.error('Failed to update status');
// //     }
// //   };

// //   const addNote = async (id) => {
// //     if (!note.trim()) {
// //       toast.error("Note cannot be empty!");
// //       return;
// //     }

// //     try {
// //       await axios.post(`http://localhost:5215/api/v1/Tickets/${id}/notes`, 
// //         { note: note },
// //         { headers: { 'Authorization': `Bearer ${user.token}` } }
// //       );
// //       toast.success('Internal note added securely! 📝');
// //       setNote(''); 
// //       setSelectedTicket(null); 
// //       fetchTickets(); 
// //     } catch (error) {
// //       toast.error('Failed to add note');
// //     }
// //   };

// //   const handleDownloadPdf = async (ticketId) => {
// //     const loadingToast = toast.loading("Generating PDF report...");
// //     try {
// //         const response = await fetch(`http://localhost:5215/api/v1/Tickets/${ticketId}/pdf`, {
// //             method: 'GET',
// //             headers: {
// //                 'Authorization': `Bearer ${user.token}`
// //             }
// //         });

// //         if (!response.ok) throw new Error("PDF download failed");

// //         const blob = await response.blob();
// //         const url = window.URL.createObjectURL(blob);
// //         const link = document.createElement('a');
// //         link.href = url;
// //         link.setAttribute('download', `Ticket_${ticketId}_Summary.pdf`);
// //         document.body.appendChild(link);
// //         link.click();
// //         link.parentNode.removeChild(link);
        
// //         toast.success("PDF downloaded successfully! 🎉", { id: loadingToast });
// //     } catch (error) {
// //         console.error("Error downloading PDF:", error);
// //         toast.error("Failed to download PDF.", { id: loadingToast });
// //     }
// //   };

// //   return (
// //     <div className="relative h-[calc(100vh-4rem)] overflow-y-auto overflow-x-hidden bg-slate-100">
// //       <Galaxy
// //         className="opacity-70"
// //         density={1.15}
// //         glowIntensity={0.45}
// //         saturation={0.75}
// //         hueShift={205}
// //         rotationSpeed={0.06}
// //       />
// //       <div className="relative z-10 p-6 max-w-6xl mx-auto">
// //       <h2 className="text-3xl font-bold mb-6 text-gray-800">Agent Dashboard</h2>
      
// //       {/* Search & Filters */}
// //       <div className="flex gap-4 mb-6">
// //         <input 
// //           type="text" 
// //           placeholder="Search tickets..." 
// //           className="border border-gray-300 p-2.5 rounded-lg w-1/3 shadow-sm outline-none focus:ring-2 focus:ring-blue-500"
// //           value={search}
// //           onChange={(e) => setSearch(e.target.value)}
// //         />
// //         <select 
// //           className="border border-gray-300 p-2.5 rounded-lg shadow-sm outline-none focus:ring-2 focus:ring-blue-500 bg-white"
// //           value={statusFilter}
// //           onChange={(e) => setStatusFilter(e.target.value)}
// //         >
// //           <option value="">All Statuses</option>
// //           <option value="Open">Open</option>
// //           <option value="In Progress">In Progress</option>
// //           <option value="Resolved">Resolved</option>
// //         </select>
// //       </div>

// //       {/* Ticket Table */}
// //       <div className="overflow-x-auto shadow-md rounded-xl border border-gray-100">
// //         <table className="min-w-full bg-white">
// //           <thead className="bg-gray-800 text-white">
// //             <tr>
// //               <th className="py-3 px-4 text-left">ID</th>
// //               <th className="py-3 px-4 text-left">Title</th>
// //               <th className="py-3 px-4 text-left">Customer</th>
// //               <th className="py-3 px-4 text-left">Status</th>
// //               <th className="py-3 px-4 text-center">Actions</th>
// //             </tr>
// //           </thead>
// //           <tbody>
// //             {tickets.map(t => (
// //               <tr key={t.id} className="border-b hover:bg-gray-50 transition-colors">
// //                 <td className="py-3 px-4 font-semibold text-gray-600">#{t.id}</td>
// //                 <td className="py-3 px-4 font-medium text-gray-800">{t.title}</td>
// //                 <td className="py-3 px-4 text-gray-600">{t.customerName || 'Customer'}</td>
// //                 <td className="py-3 px-4">
// //                   <select 
// //                     className="border border-gray-300 p-1.5 rounded-lg font-medium text-sm text-gray-700 bg-white shadow-sm outline-none focus:ring-2 focus:ring-blue-500" 
// //                     value={t.status}
// //                     onChange={(e) => updateStatus(t.id, e.target.value)}
// //                   >
// //                     <option value="Open">Open</option>
// //                     <option value="In Progress">In Progress</option>
// //                     <option value="Resolved">Resolved</option>
// //                   </select>
// //                 </td>
// //                 <td className="py-3 px-4 text-center">
// //                   <button 
// //                     onClick={() => setSelectedTicket(t)}
// //                     className="bg-blue-600 text-white px-3.5 py-1.5 rounded-lg text-sm font-medium hover:bg-blue-700 transition-colors shadow-sm"
// //                   >
// //                     View Details
// //                   </button>
// //                 </td>
// //               </tr>
// //             ))}
// //             {tickets.length === 0 && (
// //               <tr><td colSpan="5" className="text-center py-8 text-gray-500 italic">No tickets found.</td></tr>
// //             )}
// //           </tbody>
// //         </table>
// //       </div>

// //       {/* Pagination Controls */}
// //       <div className="flex gap-4 mt-6 justify-end items-center">
// //         <button 
// //           disabled={page === 1} 
// //           onClick={() => setPage(page - 1)}
// //           className="px-4 py-2 bg-gray-200 text-gray-700 font-medium rounded-lg disabled:opacity-50 hover:bg-gray-300 transition-colors"
// //         >
// //           Previous
// //         </button>
// //         <span className="font-semibold text-gray-700">Page {page}</span>
// //         <button 
// //           onClick={() => setPage(page + 1)}
// //           className="px-4 py-2 bg-gray-200 text-gray-700 font-medium rounded-lg hover:bg-gray-300 transition-colors"
// //         >
// //           Next
// //         </button>
// //       </div>

// //       {/* Ticket Details Modal */}
// //       {selectedTicket && (
// //         <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50 backdrop-blur-sm">
// //           <div className="bg-white rounded-2xl p-6 max-w-2xl w-full shadow-2xl relative overflow-y-auto max-h-[90vh]">
// //             <button 
// //               onClick={() => setSelectedTicket(null)}
// //               className="absolute top-4 right-4 text-gray-400 hover:text-gray-700 font-bold text-xl bg-gray-100 hover:bg-gray-200 w-8 h-8 rounded-full flex items-center justify-center transition-colors"
// //             >
// //               ✕
// //             </button>
            
// //             <h3 className="text-2xl font-bold mb-2 text-gray-800">Ticket #{selectedTicket.id}: {selectedTicket.title}</h3>
// //             <p className="text-gray-600 mb-4 bg-gray-50 p-4 rounded-xl border border-gray-100 text-sm">{selectedTicket.description}</p>
            
// //            {/* AI Insights Card */}
// //            <div className="bg-purple-50 border border-purple-200 p-4 rounded-xl mb-4 shadow-sm">
// //               <h4 className="font-bold text-purple-900 mb-1 flex items-center gap-1.5">🤖 AI Insights</h4>
// //               <p className="text-sm text-gray-700 mb-1.5">
// //                 <span className="font-semibold">Summary:</span> {selectedTicket.aiSummary || selectedTicket.AiSummary || 'Analyzing summary...'}
// //               </p>
// //               <p className="text-sm flex items-center">
// //                 <span className="font-semibold">Sentiment:</span> 
// //                 <span className={`ml-2 px-2.5 py-0.5 rounded-full text-xs font-bold ${
// //                   (selectedTicket.aiSentiment || selectedTicket.AiSentiment) === 'Angry' ? 'bg-red-100 text-red-700' :
// //                   (selectedTicket.aiSentiment || selectedTicket.AiSentiment) === 'Happy' ? 'bg-green-100 text-green-700' :
// //                   'bg-gray-200 text-gray-700'
// //                 }`}>
// //                   {selectedTicket.aiSentiment || selectedTicket.AiSentiment || 'Pending'}
// //                 </span>
// //               </p>
// //             </div>

// //             {/* PDF Download Button */}
// //             <button 
// //               onClick={() => handleDownloadPdf(selectedTicket.id)}
// //               className="mb-6 w-full bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2.5 px-4 rounded-xl shadow-sm transition-colors flex items-center justify-center gap-2"
// //             >
// //               📄 Download PDF Report Summary
// //             </button>

// //             <div className="mb-4 text-sm text-gray-600">
// //               <span className="font-semibold text-gray-800">Category:</span> {selectedTicket.category || 'General'} | 
// //               <span className="font-semibold text-gray-800 ml-2">Customer:</span> {selectedTicket.customerName}
// //             </div>

// //             {/* Display Attached Image */}
// //             {selectedTicket.imageUrl && (
// //               <div className="mb-4">
// //                 <h4 className="font-semibold text-gray-700 mb-2 text-sm">Attached Screenshot:</h4>
// //                 <div className="rounded-xl overflow-hidden border border-gray-200 bg-gray-50 p-2">
// //                   <img src={selectedTicket.imageUrl} alt="Ticket Attachment" className="max-h-60 mx-auto rounded-lg object-contain" />
// //                 </div>
// //               </div>
// //             )}

// //             {/* Display Attached Audio */}
// //             {selectedTicket.audioUrl && (
// //               <div className="mb-4">
// //                 <h4 className="font-semibold text-gray-700 mb-2 text-sm">Voice Note:</h4>
// //                 <audio controls src={selectedTicket.audioUrl} className="w-full">
// //                   Your browser does not support the audio element.
// //                 </audio>
// //               </div>
// //             )}

// //             {/* Add Internal Note */}
// //             <div className="mt-6 border-t border-gray-100 pt-4">
// //               <h4 className="font-bold text-gray-800 mb-2 text-sm">Add Internal Note (Agent Only):</h4>
// //               <textarea 
// //                 className="w-full border border-gray-300 p-3 rounded-xl mb-3 outline-none focus:ring-2 focus:ring-blue-500 text-sm" 
// //                 rows="3" 
// //                 placeholder="Type your secret internal note here..."
// //                 value={note}
// //                 onChange={(e) => setNote(e.target.value)}
// //               ></textarea>
// //               <button 
// //                 onClick={() => addNote(selectedTicket.id)}
// //                 className="bg-emerald-600 text-white font-semibold px-4 py-2.5 rounded-xl hover:bg-emerald-700 transition-colors w-full shadow-sm"
// //               >
// //                 Save Internal Note
// //               </button>
// //             </div>
// //           </div>
// //         </div>
// //       )}
// //       </div>
// //     </div>
// //   );
// // }

// import { useState, useEffect } from 'react';
// import axios from 'axios';
// import toast from 'react-hot-toast'; 
// import Galaxy from './Galaxy';
// import { useAuth } from '../context/AuthContext';
// import { Sparkles, Search, X } from 'lucide-react';

// export default function AgentDashboard() {
//   const [tickets, setTickets] = useState([]);
//   const [search, setSearch] = useState('');
//   const [statusFilter, setStatusFilter] = useState('');
//   const [page, setPage] = useState(1);
//   const [selectedTicket, setSelectedTicket] = useState(null);
//   const [note, setNote] = useState('');
  
//   // 🔥 AI Search States
//   const [aiQuery, setAiQuery] = useState('');
//   const [isAiMode, setIsAiMode] = useState(false);
//   const [isAiSearching, setIsAiSearching] = useState(false);

//   const { user } = useAuth();

//   useEffect(() => {
//     // Agar AI mode ON hai, toh normal pagination aur filters ko pause kar denge
//     if (!isAiMode) {
//       fetchTickets();
//     }
//   }, [search, statusFilter, page, isAiMode]);

//   const fetchTickets = async () => {
//     try {
//       const response = await axios.get('http://localhost:5215/api/v1/Tickets', {
//       // const response = await axios.get('http://34.93.237.221:5215/api/v1/Tickets', {
//         headers: { 'Authorization': `Bearer ${user.token}` },
//         params: { search, status: statusFilter, page, pageSize: 10 }
//       });
//       setTickets(response.data.tickets);
//     } catch (error) {
//       toast.error("Failed to load tickets.");
//     }
//   };

//   // 🧠 THE MAGIC: Semantic Search Function
//   const handleSemanticSearch = async (e) => {
//     e.preventDefault();
//     if (!aiQuery.trim()) {
//       clearAiSearch();
//       return;
//     }
    
//     setIsAiSearching(true);
//     try {
//       const response = await axios.get('http://localhost:5215/api/v1/Tickets/semantic-search', {
        
//               //const response = await axios.get('http://34.93.237.221:5215/api/v1/Tickets/semantic-search', {

//         headers: { 'Authorization': `Bearer ${user.token}` },
//         params: { query: aiQuery }
//       });
      
//       setTickets(response.data); // Normal tickets ko AI vectors se replace kar diya
//       setIsAiMode(true);
//       toast.success("AI found the most relevant tickets! 🎯");
//     } catch (error) {
//       toast.error('Semantic search failed');
//     } finally {
//       setIsAiSearching(false);
//     }
//   };

//   const clearAiSearch = () => {
//     setAiQuery('');
//     setIsAiMode(false);
//     setPage(1);
//   };

//   const updateStatus = async (id, newStatus) => {
//     try {
//       await axios.put(`http://localhost:5215/api/v1/Tickets/${id}/status`, 
//      //     await axios.put(`http://34.93.237.221:5215/api/v1/Tickets/${id}/status`,
//         { status: newStatus },
//         { headers: { 'Authorization': `Bearer ${user.token}` } }
//       );
//       toast.success(`Ticket #${id} status updated to ${newStatus}`);
//       if (!isAiMode) fetchTickets(); // AI mode me background refresh skip karenge
//     } catch (error) {
//       toast.error('Failed to update status');
//     }
//   };

//   const addNote = async (id) => {
//     if (!note.trim()) return toast.error("Note cannot be empty!");
//     try {
//       await axios.post(`http://localhost:5215/api/v1/Tickets/${id}/notes`, 

//            // await axios.post(`http://34.93.237.221:5215/api/v1/Tickets/${id}/notes`, 
//         { note: note },
//         { headers: { 'Authorization': `Bearer ${user.token}` } }
//       );
//       toast.success('Internal note added securely! 📝');
//       setNote(''); 
//       setSelectedTicket(null); 
//     } catch (error) {
//       toast.error('Failed to add note');
//     }
//   };

//   const handleDownloadPdf = async (ticketId) => {
//     const loadingToast = toast.loading("Generating PDF report...");
//     try {
//         const response = await fetch(`http://localhost:5215/api/v1/Tickets/${ticketId}/pdf`, {
//            //const response = await fetch(`http://34.93.237.221:5215/api/v1/Tickets/${ticketId}/pdf`, {
//             headers: { 'Authorization': `Bearer ${user.token}` }
//         });
//         if (!response.ok) throw new Error("PDF download failed");

//         const blob = await response.blob();
//         const url = window.URL.createObjectURL(blob);
//         const link = document.createElement('a');
//         link.href = url;
//         link.setAttribute('download', `Ticket_${ticketId}_Summary.pdf`);
//         document.body.appendChild(link);
//         link.click();
//         link.parentNode.removeChild(link);
        
//         toast.success("PDF downloaded successfully! 🎉", { id: loadingToast });
//     } catch (error) {
//         toast.error("Failed to download PDF.", { id: loadingToast });
//     }
//   };

//   // AI distance ko Match Percentage me convert karna
//   const getMatchPercentage = (distance) => {
//     return Math.max(0, (1 - distance) * 100).toFixed(1);
//   };

//   return (
//     <div className="relative h-[calc(100vh-4rem)] overflow-y-auto overflow-x-hidden bg-slate-100">
//       <Galaxy className="opacity-70" density={1.15} glowIntensity={0.45} saturation={0.75} hueShift={205} rotationSpeed={0.06} />
      
//       <div className="relative z-10 p-6 max-w-6xl mx-auto">
//         <h2 className="text-3xl font-bold mb-6 text-gray-800">Agent Dashboard</h2>
        
//         {/* 🌟 NEW: AI Semantic Search Bar */}
//         <form onSubmit={handleSemanticSearch} className="mb-6">
//           <div className={`p-[2px] rounded-xl shadow-sm transition-all duration-300 ${isAiMode ? 'bg-gradient-to-r from-fuchsia-500 via-purple-500 to-indigo-500' : 'bg-gradient-to-r from-blue-400 to-indigo-400'}`}>
//             <div className="bg-white rounded-[10px] p-2 pl-4 flex items-center gap-3">
//               <Sparkles className={`w-6 h-6 ${isAiMode ? 'text-fuchsia-500' : 'text-indigo-400'}`} />
//               <input 
//                 type="text" 
//                 placeholder="Ask AI... (e.g. 'Find angry customers who had payment issues')" 
//                 className="flex-1 outline-none text-gray-700 bg-transparent py-2 font-medium"
//                 value={aiQuery}
//                 onChange={(e) => setAiQuery(e.target.value)}
//               />
//               {isAiMode && (
//                 <button type="button" onClick={clearAiSearch} className="p-2 text-gray-400 hover:text-red-500 transition-colors">
//                   <X className="w-5 h-5" />
//                 </button>
//               )}
//               <button 
//                 type="submit" 
//                 disabled={isAiSearching}
//                 className={`text-white px-6 py-2.5 rounded-lg font-bold transition-all shadow-md flex items-center gap-2 ${isAiMode ? 'bg-purple-600 hover:bg-purple-700' : 'bg-indigo-600 hover:bg-indigo-700'}`}
//               >
//                 {isAiSearching ? '🧠 Thinking...' : 'Semantic Search'}
//               </button>
//             </div>
//           </div>
//         </form>

//         {/* Normal Filters (Hide in AI Mode) */}
//         {!isAiMode && (
//           <div className="flex gap-4 mb-6">
//             <div className="relative w-1/3">
//               <Search className="absolute left-3 top-3 w-5 h-5 text-gray-400" />
//               <input 
//                 type="text" 
//                 placeholder="Standard Search..." 
//                 className="border border-gray-300 pl-10 p-2.5 rounded-lg w-full shadow-sm outline-none focus:ring-2 focus:ring-blue-500"
//                 value={search}
//                 onChange={(e) => setSearch(e.target.value)}
//               />
//             </div>
//             <select 
//               className="border border-gray-300 p-2.5 rounded-lg shadow-sm outline-none focus:ring-2 focus:ring-blue-500 bg-white"
//               value={statusFilter}
//               onChange={(e) => setStatusFilter(e.target.value)}
//             >
//               <option value="">All Statuses</option>
//               <option value="Open">Open</option>
//               <option value="In Progress">In Progress</option>
//               <option value="Resolved">Resolved</option>
//             </select>
//           </div>
//         )}

//         {/* AI Results Indicator */}
//         {isAiMode && (
//           <div className="mb-4 text-purple-800 bg-purple-100 px-4 py-2 rounded-lg font-medium border border-purple-200 inline-block">
//             Showing top semantic matches from Vector Database based on AI Embeddings.
//           </div>
//         )}

//         {/* Ticket Table */}
//         <div className="overflow-x-auto shadow-md rounded-xl border border-gray-100">
//           <table className="min-w-full bg-white">
//             <thead className={`${isAiMode ? 'bg-purple-900' : 'bg-gray-800'} text-white transition-colors`}>
//               <tr>
//                 <th className="py-3 px-4 text-left">ID</th>
//                 <th className="py-3 px-4 text-left">Title & Summary</th>
//                 {isAiMode && <th className="py-3 px-4 text-left">AI Match Score</th>}
//                 <th className="py-3 px-4 text-left">Status</th>
//                 <th className="py-3 px-4 text-center">Actions</th>
//               </tr>
//             </thead>
//             <tbody>
//               {tickets.map(t => (
//                 <tr key={t.id} className="border-b hover:bg-gray-50 transition-colors">
//                   <td className="py-3 px-4 font-semibold text-gray-600">#{t.id}</td>
                  
//                   <td className="py-3 px-4">
//                     <p className="font-bold text-gray-800 mb-1">{t.title}</p>
//                     {isAiMode && (
//                       <p className="text-sm text-gray-500 line-clamp-2 italic">"{t.aiSummary || 'No summary generated yet'}"</p>
//                     )}
//                   </td>

//                   {isAiMode && (
//                     <td className="py-3 px-4">
//                       <span className="bg-fuchsia-100 text-fuchsia-700 px-3 py-1 rounded-full font-bold text-sm border border-fuchsia-200">
//                         {getMatchPercentage(t.distance)}% Match
//                       </span>
//                     </td>
//                   )}

//                   <td className="py-3 px-4">
//                     <select 
//                       className="border border-gray-300 p-1.5 rounded-lg font-medium text-sm text-gray-700 bg-white shadow-sm outline-none focus:ring-2 focus:ring-blue-500" 
//                       value={t.status}
//                       onChange={(e) => updateStatus(t.id, e.target.value)}
//                     >
//                       <option value="Open">Open</option>
//                       <option value="In Progress">In Progress</option>
//                       <option value="Resolved">Resolved</option>
//                     </select>
//                   </td>
//                   <td className="py-3 px-4 text-center">
//                     <button 
//                       onClick={() => setSelectedTicket(t)}
//                       className="bg-blue-600 text-white px-3.5 py-1.5 rounded-lg text-sm font-medium hover:bg-blue-700 transition-colors shadow-sm"
//                     >
//                       View Details
//                     </button>
//                   </td>
//                 </tr>
//               ))}
//               {tickets.length === 0 && (
//                 <tr><td colSpan={isAiMode ? "5" : "4"} className="text-center py-8 text-gray-500 italic">No tickets found.</td></tr>
//               )}
//             </tbody>
//           </table>
//         </div>

//         {/* Pagination Controls (Hide in AI Mode because RAG returns top 5 directly) */}
//         {!isAiMode && (
//           <div className="flex gap-4 mt-6 justify-end items-center">
//             <button 
//               disabled={page === 1} 
//               onClick={() => setPage(page - 1)}
//               className="px-4 py-2 bg-gray-200 text-gray-700 font-medium rounded-lg disabled:opacity-50 hover:bg-gray-300 transition-colors"
//             >
//               Previous
//             </button>
//             <span className="font-semibold text-gray-700">Page {page}</span>
//             <button 
//               onClick={() => setPage(page + 1)}
//               className="px-4 py-2 bg-gray-200 text-gray-700 font-medium rounded-lg hover:bg-gray-300 transition-colors"
//             >
//               Next
//             </button>
//           </div>
//         )}

//         {/* Ticket Details Modal */}
//         {selectedTicket && (
//           <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50 backdrop-blur-sm">
//             <div className="bg-white rounded-2xl p-6 max-w-2xl w-full shadow-2xl relative overflow-y-auto max-h-[90vh]">
//               <button 
//                 onClick={() => setSelectedTicket(null)}
//                 className="absolute top-4 right-4 text-gray-400 hover:text-gray-700 font-bold text-xl bg-gray-100 hover:bg-gray-200 w-8 h-8 rounded-full flex items-center justify-center transition-colors"
//               >
//                 <X className="w-5 h-5" />
//               </button>
              
//               <h3 className="text-2xl font-bold mb-2 text-gray-800">Ticket #{selectedTicket.id}: {selectedTicket.title}</h3>
//               <p className="text-gray-600 mb-4 bg-gray-50 p-4 rounded-xl border border-gray-100 text-sm">{selectedTicket.description || 'View PDF for full description.'}</p>
              
//              {/* AI Insights Card */}
//              <div className="bg-purple-50 border border-purple-200 p-4 rounded-xl mb-4 shadow-sm">
//                 <h4 className="font-bold text-purple-900 mb-1 flex items-center gap-1.5">
//                   <Sparkles className="w-4 h-4" /> AI Insights
//                 </h4>
//                 <p className="text-sm text-gray-700 mb-1.5">
//                   <span className="font-semibold">Summary:</span> {selectedTicket.aiSummary || selectedTicket.AiSummary || 'Analyzing summary...'}
//                 </p>
//                 <p className="text-sm flex items-center">
//                   <span className="font-semibold">Sentiment:</span> 
//                   <span className={`ml-2 px-2.5 py-0.5 rounded-full text-xs font-bold ${
//                     (selectedTicket.aiSentiment || selectedTicket.AiSentiment) === 'Angry' ? 'bg-red-100 text-red-700' :
//                     (selectedTicket.aiSentiment || selectedTicket.AiSentiment) === 'Happy' ? 'bg-green-100 text-green-700' :
//                     'bg-gray-200 text-gray-700'
//                   }`}>
//                     {selectedTicket.aiSentiment || selectedTicket.AiSentiment || 'Neutral'}
//                   </span>
//                 </p>
//               </div>

//               {/* PDF Download Button */}
//               <button 
//                 onClick={() => handleDownloadPdf(selectedTicket.id)}
//                 className="mb-6 w-full bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2.5 px-4 rounded-xl shadow-sm transition-colors flex items-center justify-center gap-2"
//               >
//                 📄 Download Detailed PDF Report
//               </button>

//               {/* Add Internal Note */}
//               <div className="mt-6 border-t border-gray-100 pt-4">
//                 <h4 className="font-bold text-gray-800 mb-2 text-sm">Add Internal Note (Agent Only):</h4>
//                 <textarea 
//                   className="w-full border border-gray-300 p-3 rounded-xl mb-3 outline-none focus:ring-2 focus:ring-blue-500 text-sm" 
//                   rows="3" 
//                   placeholder="Type your secret internal note here..."
//                   value={note}
//                   onChange={(e) => setNote(e.target.value)}
//                 ></textarea>
//                 <button 
//                   onClick={() => addNote(selectedTicket.id)}
//                   className="bg-emerald-600 text-white font-semibold px-4 py-2.5 rounded-xl hover:bg-emerald-700 transition-colors w-full shadow-sm"
//                 >
//                   Save Internal Note
//                 </button>
//               </div>
//             </div>
//           </div>
//         )}
//       </div>
//     </div>
//   );
// }

import { useState, useEffect } from 'react';
import axios from 'axios';
import toast from 'react-hot-toast'; 
import Galaxy from './Galaxy';
import { useAuth } from '../context/AuthContext';
import { Sparkles, Search, X } from 'lucide-react';

export default function AgentDashboard() {
  const [tickets, setTickets] = useState([]);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [page, setPage] = useState(1);
  const [selectedTicket, setSelectedTicket] = useState(null);
  const [note, setNote] = useState('');
  
  // 🔥 AI Search States
  const [aiQuery, setAiQuery] = useState('');
  const [isAiMode, setIsAiMode] = useState(false);
  const [isAiSearching, setIsAiSearching] = useState(false);

  const [draftReply, setDraftReply] = useState('');
  const [isGeneratingReply, setIsGeneratingReply] = useState(false);

  const { user } = useAuth();

  useEffect(() => {
    // Agar AI mode ON hai, toh normal pagination aur filters ko pause kar denge
    if (!isAiMode) {
      fetchTickets();
    }
  }, [search, statusFilter, page, isAiMode]);

  const fetchTickets = async () => {
    try {
      const response = await axios.get('http://localhost:5215/api/v1/Tickets', {
        headers: { 'Authorization': `Bearer ${user.token}` },
        params: { search, status: statusFilter, page, pageSize: 10 }
      });
      setTickets(response.data.tickets);
    } catch (error) {
      toast.error("Failed to load tickets.");
    }
  };


  const generateAiReply = async (ticketId) => {
    setIsGeneratingReply(true);
    const loadingToast = toast.loading("🧠 Searching past solutions & drafting reply...");
    try {
      const response = await axios.post(`http://localhost:5215/api/v1/Tickets/${ticketId}/draft-reply`, {}, {
        headers: { 'Authorization': `Bearer ${user.token}` }
      });
      setDraftReply(response.data.draftReply);
      toast.success("AI Draft Generated Successfully!", { id: loadingToast });
    } catch (error) {
      toast.error(error.response?.data?.message || 'Failed to generate AI reply', { id: loadingToast });
    } finally {
      setIsGeneratingReply(false);
    }
  };

  // 🧠 THE MAGIC: Semantic Search Function
  const handleSemanticSearch = async (e) => {
    e.preventDefault();
    if (!aiQuery.trim()) {
      clearAiSearch();
      return;
    }
    
    setIsAiSearching(true);
    try {
      const response = await axios.get('http://localhost:5215/api/v1/Tickets/semantic-search', {
        headers: { 'Authorization': `Bearer ${user.token}` },
        params: { query: aiQuery }
      });
      
      setTickets(response.data); // Normal tickets ko AI vectors se replace kar diya
      setIsAiMode(true);
      toast.success("AI found the most relevant tickets! 🎯");
    } catch (error) {
      toast.error('Semantic search failed');
    } finally {
      setIsAiSearching(false);
    }
  };

  const clearAiSearch = () => {
    setAiQuery('');
    setIsAiMode(false);
    setPage(1);
  };

  const updateStatus = async (id, newStatus) => {
    try {
      await axios.put(`http://localhost:5215/api/v1/Tickets/${id}/status`, 
        { status: newStatus },
        { headers: { 'Authorization': `Bearer ${user.token}` } }
      );
      toast.success(`Ticket #${id} status updated to ${newStatus}`);
      if (!isAiMode) fetchTickets(); // AI mode me background refresh skip karenge
    } catch (error) {
      toast.error('Failed to update status');
    }
  };

  const addNote = async (id) => {
    if (!note.trim()) return toast.error("Note cannot be empty!");
    try {
      await axios.post(`http://localhost:5215/api/v1/Tickets/${id}/notes`, 
        { note: note },
        { headers: { 'Authorization': `Bearer ${user.token}` } }
      );
      toast.success('Internal note added securely! 📝');
      setNote(''); 
      setSelectedTicket(null); 
    } catch (error) {
      toast.error('Failed to add note');
    }
  };

  const handleDownloadPdf = async (ticketId) => {
    const loadingToast = toast.loading("Generating PDF report...");
    try {
        const response = await fetch(`http://localhost:5215/api/v1/Tickets/${ticketId}/pdf`, {
            headers: { 'Authorization': `Bearer ${user.token}` }
        });
        if (!response.ok) throw new Error("PDF download failed");

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `Ticket_${ticketId}_Summary.pdf`);
        document.body.appendChild(link);
        link.click();
        link.parentNode.removeChild(link);
        
        toast.success("PDF downloaded successfully! 🎉", { id: loadingToast });
    } catch (error) {
        toast.error("Failed to download PDF.", { id: loadingToast });
    }
  };

  // AI distance ko Match Percentage me convert karna
  const getMatchPercentage = (distance) => {
    return Math.max(0, (1 - distance) * 100).toFixed(1);
  };

  // 🔥 NEW: Dynamic Sentiment Color Function
  const getSentimentBadgeStyle = (sentiment) => {
    if (!sentiment) return "bg-gray-100 text-gray-500 border-gray-200"; 
    const text = sentiment.toLowerCase();
    
    if (text.includes("angry") || text.includes("frustrated") || text.includes("furious") || text.includes("urgent") || text.includes("panicked") || text.includes("sarcastic") || text.includes("demanding")) {
      return "bg-red-50 text-red-600 border-red-200";
    }
    if (text.includes("happy") || text.includes("appreciative") || text.includes("satisfied") || text.includes("grateful") || text.includes("awesome")) {
      return "bg-green-50 text-green-600 border-green-200";
    }
    if (text.includes("confused") || text.includes("delay") || text.includes("waiting") || text.includes("polite")) {
      return "bg-yellow-50 text-yellow-600 border-yellow-200";
    }
    return "bg-blue-50 text-blue-600 border-blue-200"; 
  };

  return (
    <div className="relative h-[calc(100vh-4rem)] overflow-y-auto overflow-x-hidden bg-slate-100">
      <Galaxy className="opacity-70" density={1.15} glowIntensity={0.45} saturation={0.75} hueShift={205} rotationSpeed={0.06} />
      
      <div className="relative z-10 p-6 max-w-6xl mx-auto">
        <h2 className="text-3xl font-bold mb-6 text-gray-800">Agent Dashboard</h2>
        
        {/* 🌟 NEW: AI Semantic Search Bar */}
        <form onSubmit={handleSemanticSearch} className="mb-6">
          <div className={`p-[2px] rounded-xl shadow-sm transition-all duration-300 ${isAiMode ? 'bg-gradient-to-r from-fuchsia-500 via-purple-500 to-indigo-500' : 'bg-gradient-to-r from-blue-400 to-indigo-400'}`}>
            <div className="bg-white rounded-[10px] p-2 pl-4 flex items-center gap-3">
              <Sparkles className={`w-6 h-6 ${isAiMode ? 'text-fuchsia-500' : 'text-indigo-400'}`} />
              <input 
                type="text" 
                placeholder="Ask AI... (e.g. 'Find angry customers who had payment issues')" 
                className="flex-1 outline-none text-gray-700 bg-transparent py-2 font-medium"
                value={aiQuery}
                onChange={(e) => setAiQuery(e.target.value)}
              />
              {isAiMode && (
                <button type="button" onClick={clearAiSearch} className="p-2 text-gray-400 hover:text-red-500 transition-colors">
                  <X className="w-5 h-5" />
                </button>
              )}
              <button 
                type="submit" 
                disabled={isAiSearching}
                className={`text-white px-6 py-2.5 rounded-lg font-bold transition-all shadow-md flex items-center gap-2 ${isAiMode ? 'bg-purple-600 hover:bg-purple-700' : 'bg-indigo-600 hover:bg-indigo-700'}`}
              >
                {isAiSearching ? '🧠 Thinking...' : 'Semantic Search'}
              </button>
            </div>
          </div>
        </form>

        {/* Normal Filters (Hide in AI Mode) */}
        {!isAiMode && (
          <div className="flex gap-4 mb-6">
            <div className="relative w-1/3">
              <Search className="absolute left-3 top-3 w-5 h-5 text-gray-400" />
              <input 
                type="text" 
                placeholder="Standard Search..." 
                className="border border-gray-300 pl-10 p-2.5 rounded-lg w-full shadow-sm outline-none focus:ring-2 focus:ring-blue-500"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
            </div>
            <select 
              className="border border-gray-300 p-2.5 rounded-lg shadow-sm outline-none focus:ring-2 focus:ring-blue-500 bg-white"
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
            >
              <option value="">All Statuses</option>
              <option value="Open">Open</option>
              <option value="In Progress">In Progress</option>
              <option value="Resolved">Resolved</option>
            </select>
          </div>
        )}

        {/* AI Results Indicator */}
        {isAiMode && (
          <div className="mb-4 text-purple-800 bg-purple-100 px-4 py-2 rounded-lg font-medium border border-purple-200 inline-block">
            Showing top semantic matches from Vector Database based on AI Embeddings.
          </div>
        )}

        {/* Ticket Table */}
        <div className="overflow-x-auto shadow-md rounded-xl border border-gray-100">
          <table className="min-w-full bg-white">
            <thead className={`${isAiMode ? 'bg-purple-900' : 'bg-gray-800'} text-white transition-colors`}>
              <tr>
                <th className="py-3 px-4 text-left">ID</th>
                <th className="py-3 px-4 text-left">Title & Summary</th>
                {isAiMode && <th className="py-3 px-4 text-left">AI Match Score</th>}
                <th className="py-3 px-4 text-left">Status</th>
                {/* 🔥 NEW COLUMN HEADER */}
                <th className="py-3 px-4 text-center">AI Sentiment</th>
                <th className="py-3 px-4 text-center">Actions</th>
              </tr>
            </thead>
            <tbody>
              {tickets.map(t => (
                <tr key={t.id} className="border-b hover:bg-gray-50 transition-colors">
                  <td className="py-3 px-4 font-semibold text-gray-600">#{t.id}</td>
                  
                  <td className="py-3 px-4">
                    <p className="font-bold text-gray-800 mb-1">{t.title}</p>
                    {isAiMode && (
                      <p className="text-sm text-gray-500 line-clamp-2 italic">"{t.aiSummary || 'No summary generated yet'}"</p>
                    )}
                  </td>

                  {isAiMode && (
                    <td className="py-3 px-4">
                      <span className="bg-fuchsia-100 text-fuchsia-700 px-3 py-1 rounded-full font-bold text-sm border border-fuchsia-200">
                        {getMatchPercentage(t.distance)}% Match
                      </span>
                    </td>
                  )}

                  <td className="py-3 px-4">
                    <select 
                      className="border border-gray-300 p-1.5 rounded-lg font-medium text-sm text-gray-700 bg-white shadow-sm outline-none focus:ring-2 focus:ring-blue-500" 
                      value={t.status}
                      onChange={(e) => updateStatus(t.id, e.target.value)}
                    >
                      <option value="Open">Open</option>
                      <option value="In Progress">In Progress</option>
                      <option value="Resolved">Resolved</option>
                    </select>
                  </td>
                  
                  {/* 🔥 NEW COLUMN CONTENT */}
                  <td className="py-3 px-4 text-center">
                    <div className="flex justify-center items-center">
                      <span 
                        className={`inline-block max-w-[180px] text-wrap text-xs px-3 py-1.5 rounded-lg border font-semibold leading-tight shadow-sm ${getSentimentBadgeStyle(t.aiSentiment)}`}
                      >
                        {t.aiSentiment || 'Analyzing...'}
                      </span>
                    </div>
                  </td>

                  <td className="py-3 px-4 text-center">
                    <button 
                      onClick={() => setSelectedTicket(t)}
                      className="bg-blue-600 text-white px-3.5 py-1.5 rounded-lg text-sm font-medium hover:bg-blue-700 transition-colors shadow-sm"
                    >
                      View Details
                    </button>
                  </td>
                </tr>
              ))}
              {tickets.length === 0 && (
                <tr><td colSpan={isAiMode ? "6" : "5"} className="text-center py-8 text-gray-500 italic">No tickets found.</td></tr>
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination Controls */}
        {!isAiMode && (
          <div className="flex gap-4 mt-6 justify-end items-center">
            <button 
              disabled={page === 1} 
              onClick={() => setPage(page - 1)}
              className="px-4 py-2 bg-gray-200 text-gray-700 font-medium rounded-lg disabled:opacity-50 hover:bg-gray-300 transition-colors"
            >
              Previous
            </button>
            <span className="font-semibold text-gray-700">Page {page}</span>
            <button 
              onClick={() => setPage(page + 1)}
              className="px-4 py-2 bg-gray-200 text-gray-700 font-medium rounded-lg hover:bg-gray-300 transition-colors"
            >
              Next
            </button>
          </div>
        )}

        {/* Ticket Details Modal */}
        {selectedTicket && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50 backdrop-blur-sm">
            <div className="bg-white rounded-2xl p-6 max-w-2xl w-full shadow-2xl relative overflow-y-auto max-h-[90vh]">
              <button 
                onClick={() => setSelectedTicket(null)}
                className="absolute top-4 right-4 text-gray-400 hover:text-gray-700 font-bold text-xl bg-gray-100 hover:bg-gray-200 w-8 h-8 rounded-full flex items-center justify-center transition-colors"
              >
                <X className="w-5 h-5" />
              </button>
              
              <h3 className="text-2xl font-bold mb-2 text-gray-800">Ticket #{selectedTicket.id}: {selectedTicket.title}</h3>
              <p className="text-gray-600 mb-4 bg-gray-50 p-4 rounded-xl border border-gray-100 text-sm">{selectedTicket.description || 'View PDF for full description.'}</p>
              
             {/* AI Insights Card */}
             <div className="bg-purple-50 border border-purple-200 p-4 rounded-xl mb-4 shadow-sm">
                <h4 className="font-bold text-purple-900 mb-1 flex items-center gap-1.5">
                  <Sparkles className="w-4 h-4" /> AI Insights
                </h4>
                <p className="text-sm text-gray-700 mb-1.5">
                  <span className="font-semibold">Summary:</span> {selectedTicket.aiSummary || selectedTicket.AiSummary || 'Analyzing summary...'}
                </p>
                <p className="text-sm flex items-center">
                  <span className="font-semibold">Sentiment:</span> 
                  {/* 🔥 UPDATED MODAL SENTIMENT */}
                  <span className={`ml-2 px-2.5 py-0.5 rounded-full text-xs font-bold border ${getSentimentBadgeStyle(selectedTicket.aiSentiment || selectedTicket.AiSentiment)}`}>
                    {selectedTicket.aiSentiment || selectedTicket.AiSentiment || 'Neutral'}
                  </span>
                </p>
              </div>

              {/* PDF Download Button */}
              <button 
                onClick={() => handleDownloadPdf(selectedTicket.id)}
                className="mb-6 w-full bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2.5 px-4 rounded-xl shadow-sm transition-colors flex items-center justify-center gap-2"
              >
                📄 Download Detailed PDF Report
              </button>

            {/* 🔥 NEW: RAG Auto-Reply Section */}
              <div className="mt-6 border-t border-gray-100 pt-5 mb-4">
                <div className="flex justify-between items-center mb-3">
                  <h4 className="font-bold text-indigo-900 flex items-center gap-2 text-sm">
                    <Sparkles className="w-4 h-4 text-indigo-600" /> RAG Auto-Reply (Draft)
                  </h4>
                  <button 
                    onClick={() => generateAiReply(selectedTicket.id)}
                    disabled={isGeneratingReply}
                    className="bg-indigo-100 hover:bg-indigo-200 text-indigo-700 font-bold px-4 py-2 rounded-lg text-xs transition-colors shadow-sm disabled:opacity-50"
                  >
                    {isGeneratingReply ? 'Generating...' : '✨ Generate AI Reply'}
                  </button>
                </div>
                
                {draftReply && (
                  <div className="bg-indigo-50 border border-indigo-200 p-4 rounded-xl relative shadow-inner">
                    <p className="text-sm text-gray-800 whitespace-pre-wrap leading-relaxed">
                      {draftReply}
                    </p>
                    <button 
                      onClick={() => { navigator.clipboard.writeText(draftReply); toast.success("Draft copied to clipboard!"); }}
                      className="absolute top-2 right-2 bg-white border border-gray-300 text-gray-600 hover:text-indigo-600 p-1.5 rounded-md text-xs shadow-sm transition-colors"
                      title="Copy to clipboard"
                    >
                      📋 Copy
                    </button>
                  </div>
                )}
              </div>
              
              {/* Add Internal Note */}
              <div className="mt-6 border-t border-gray-100 pt-4">
                <h4 className="font-bold text-gray-800 mb-2 text-sm">Add Internal Note (Agent Only):</h4>
                <textarea 
                  className="w-full border border-gray-300 p-3 rounded-xl mb-3 outline-none focus:ring-2 focus:ring-blue-500 text-sm" 
                  rows="3" 
                  placeholder="Type your secret internal note here..."
                  value={note}
                  onChange={(e) => setNote(e.target.value)}
                ></textarea>
                <button 
                  onClick={() => addNote(selectedTicket.id)}
                  className="bg-emerald-600 text-white font-semibold px-4 py-2.5 rounded-xl hover:bg-emerald-700 transition-colors w-full shadow-sm"
                >
                  Save Internal Note
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}