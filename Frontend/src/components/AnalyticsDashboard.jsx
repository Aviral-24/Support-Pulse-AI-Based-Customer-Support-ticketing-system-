// import React, { useState, useEffect } from 'react';
// import { PieChart, Pie, Cell, BarChart, Bar, XAxis, YAxis, Tooltip, Legend, ResponsiveContainer } from 'recharts';
// import Galaxy from './Galaxy';
// import { useAuth } from '../context/AuthContext';

// const AnalyticsDashboard = () => {
//     const [stats, setStats] = useState(null);
//     const [loading, setLoading] = useState(true);

//     // Chart Colors
//     const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884d8'];

//     useEffect(() => {
//         fetchAnalytics();
//     }, []);
// const fetchAnalytics = async () => {
//         try {

//             const { user } = useAuth();
//   //const agentToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJyYWh1bEB0ZXN0LmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkN1c3RvbWVyIiwiZXhwIjoxNzkxNjU2MDY0LCJpc3MiOiJTdXBwb3J0UHVsc2VBUEkiLCJhdWQiOiJTdXBwb3J0UHVsc2VDbGllbnQifQ.dhvvJAX2u5dOBp8rhr5panME9ROCaw1vOMM0v6a9VDg";

//             const response = await fetch('http://localhost:5215/api/v1/Analytics/dashboard', {
//                 headers: { 
//                     'Authorization': `Bearer ${user.token}` 
//                 }
//             });

//             if (!response.ok) throw new Error("Failed to fetch analytics data");

//             const data = await response.json();

//             // Recharts ko Array of Objects chahiye hota hai
//             const sentimentData = Object.keys(data.ticketsBySentiment || {}).map(key => ({
//                 name: key, value: data.ticketsBySentiment[key]
//             }));

//             const categoryData = Object.keys(data.ticketsByCategory || {}).map(key => ({
//                 name: key, value: data.ticketsByCategory[key]
//             }));

//             setStats({ ...data, sentimentData, categoryData });
//             setLoading(false);
//         } catch (error) {
//             console.error("Error fetching analytics:", error);
//             setLoading(false);
//         }
//     };

//     if (loading) return <div className="text-center p-10 text-xl font-bold">Loading Analytics... 📊</div>;
//     if (!stats) return <div className="text-center p-10 text-red-500">Failed to load data.</div>;

//     return (
//         <div className="relative h-[calc(100vh-4rem)] overflow-y-auto overflow-x-hidden bg-slate-100">
//             <Galaxy
//                 className="opacity-65"
//                 density={1.05}
//                 glowIntensity={0.4}
//                 saturation={0.65}
//                 hueShift={220}
//                 rotationSpeed={0.05}
//             />
//             <div className="relative z-10 p-6 max-w-7xl mx-auto">
//             <h1 className="text-3xl font-bold text-gray-800 mb-6">📊 Support Analytics Dashboard</h1>

//             {/* Top Stat Cards */}
//             <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-10">
//                 <div className="bg-white p-6 rounded-lg shadow-md border-l-4 border-blue-500">
//                     <h2 className="text-gray-500 text-sm uppercase font-bold">Total Tickets</h2>
//                     <p className="text-4xl font-extrabold text-gray-800 mt-2">{stats.totalTickets}</p>
//                 </div>
//                 <div className="bg-white p-6 rounded-lg shadow-md border-l-4 border-yellow-500">
//                     <h2 className="text-gray-500 text-sm uppercase font-bold">Open Tickets</h2>
//                     <p className="text-4xl font-extrabold text-yellow-600 mt-2">{stats.openTickets}</p>
//                 </div>
//                 <div className="bg-white p-6 rounded-lg shadow-md border-l-4 border-green-500">
//                     <h2 className="text-gray-500 text-sm uppercase font-bold">Resolved Tickets</h2>
//                     <p className="text-4xl font-extrabold text-green-600 mt-2">{stats.resolvedTickets}</p>
//                 </div>
//             </div>

//             {/* Charts Section */}
//             <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                
//                 {/* Sentiment Pie Chart */}
//                 <div className="bg-white p-6 rounded-lg shadow-md">
//                     <h3 className="text-xl font-bold text-gray-700 mb-4">Customer Sentiment (AI)</h3>
//                     <ResponsiveContainer width="100%" height={300}>
//                         <PieChart>
//                             <Pie data={stats.sentimentData} dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={100} label>
//                                 {stats.sentimentData.map((entry, index) => (
//                                     <Cell key={`cell-${index}`} fill={
//                                         entry.name === 'Happy' ? '#10B981' : 
//                                         entry.name === 'Angry' ? '#EF4444' : '#6B7280'
//                                     } />
//                                 ))}
//                             </Pie>
//                             <Tooltip />
//                             <Legend />
//                         </PieChart>
//                     </ResponsiveContainer>
//                 </div>

//                 {/* Category Bar Chart */}
//                 <div className="bg-white p-6 rounded-lg shadow-md">
//                     <h3 className="text-xl font-bold text-gray-700 mb-4">Tickets by Category</h3>
//                     <ResponsiveContainer width="100%" height={300}>
//                         <BarChart data={stats.categoryData}>
//                             <XAxis dataKey="name" />
//                             <YAxis />
//                             <Tooltip />
//                             <Bar dataKey="value" fill="#3B82F6" radius={[4, 4, 0, 0]} />
//                         </BarChart>
//                     </ResponsiveContainer>
//                 </div>

//             </div>
//             </div>
//         </div>
//     );
// };

// export default AnalyticsDashboard;


import React, { useState, useEffect } from 'react';
import { PieChart, Pie, Cell, BarChart, Bar, XAxis, YAxis, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import Galaxy from './Galaxy';
import { useAuth } from '../context/AuthContext';

const AnalyticsDashboard = () => {
    const [stats, setStats] = useState(null);
    const [loading, setLoading] = useState(true);
    
    // 🔥 RULE OF HOOKS: useAuth() hamesha top level par call hona chahiye!
    const { user } = useAuth(); 

    const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884d8'];

    useEffect(() => {
        // Jab user aur token dono load ho jayein, tabhi API hit karein
        if (user && user.token) {
            fetchAnalytics();
        }
    }, [user]); // Dependency array me user add kiya

    const fetchAnalytics = async () => {
        try {
            // Yahan par direct "user.token" use karenge jo upar top se mila hai
const response = await fetch('http://localhost:5215/api/v1/Analytics/dashboard', {
                // const response = await fetch('http://34.93.237.221:5215/api/v1/Analytics/dashboard', {
                headers: { 
                    'Authorization': `Bearer ${user.token}` 
                }
            });

            if (!response.ok) throw new Error("Failed to fetch analytics data");

            const data = await response.json();

            const sentimentData = Object.keys(data.ticketsBySentiment || {}).map(key => ({
                name: key, value: data.ticketsBySentiment[key]
            }));

            const categoryData = Object.keys(data.ticketsByCategory || {}).map(key => ({
                name: key, value: data.ticketsByCategory[key]
            }));

            setStats({ ...data, sentimentData, categoryData });
            setLoading(false);
        } catch (error) {
            console.error("Error fetching analytics:", error);
            setLoading(false);
        }
    };

    if (loading) return <div className="text-center p-10 text-xl font-bold mt-10">Loading Analytics... 📊</div>;
    if (!stats) return <div className="text-center p-10 text-red-500 mt-10">Failed to load data.</div>;

    return (
        <div className="relative h-[calc(100vh-4rem)] overflow-y-auto overflow-x-hidden bg-slate-100">
            <Galaxy className="opacity-65" density={1.05} glowIntensity={0.4} saturation={0.65} hueShift={220} rotationSpeed={0.05} />
            <div className="relative z-10 p-6 max-w-7xl mx-auto">
            <h1 className="text-3xl font-bold text-gray-800 mb-6">📊 Support Analytics Dashboard</h1>

            {/* Top Stat Cards */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-10">
                <div className="bg-white p-6 rounded-lg shadow-md border-l-4 border-blue-500">
                    <h2 className="text-gray-500 text-sm uppercase font-bold">Total Tickets</h2>
                    <p className="text-4xl font-extrabold text-gray-800 mt-2">{stats.totalTickets}</p>
                </div>
                <div className="bg-white p-6 rounded-lg shadow-md border-l-4 border-yellow-500">
                    <h2 className="text-gray-500 text-sm uppercase font-bold">Open Tickets</h2>
                    <p className="text-4xl font-extrabold text-yellow-600 mt-2">{stats.openTickets}</p>
                </div>
                <div className="bg-white p-6 rounded-lg shadow-md border-l-4 border-green-500">
                    <h2 className="text-gray-500 text-sm uppercase font-bold">Resolved Tickets</h2>
                    <p className="text-4xl font-extrabold text-green-600 mt-2">{stats.resolvedTickets}</p>
                </div>
            </div>

            {/* Charts Section */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                <div className="bg-white p-6 rounded-lg shadow-md">
                    <h3 className="text-xl font-bold text-gray-700 mb-4">Customer Sentiment (AI)</h3>
                    <ResponsiveContainer width="100%" height={300}>
                        <PieChart>
                            <Pie data={stats.sentimentData} dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={100} label>
                                {stats.sentimentData.map((entry, index) => (
                                    <Cell key={`cell-${index}`} fill={entry.name === 'Happy' ? '#10B981' : entry.name === 'Angry' ? '#EF4444' : '#6B7280'} />
                                ))}
                            </Pie>
                            <Tooltip />
                            <Legend />
                        </PieChart>
                    </ResponsiveContainer>
                </div>

                <div className="bg-white p-6 rounded-lg shadow-md">
                    <h3 className="text-xl font-bold text-gray-700 mb-4">Tickets by Category</h3>
                    <ResponsiveContainer width="100%" height={300}>
                        <BarChart data={stats.categoryData}>
                            <XAxis dataKey="name" />
                            <YAxis />
                            <Tooltip />
                            <Bar dataKey="value" fill="#3B82F6" radius={[4, 4, 0, 0]} />
                        </BarChart>
                    </ResponsiveContainer>
                </div>
            </div>
            </div>
        </div>
    );
};

export default AnalyticsDashboard;