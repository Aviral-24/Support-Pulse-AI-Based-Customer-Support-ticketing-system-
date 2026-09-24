import React, { useState, useEffect } from 'react';
import { PieChart, Pie, Cell, BarChart, Bar, XAxis, YAxis, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import { useAuth } from '../context/AuthContext';

const AnalyticsDashboard = () => {
    const [stats, setStats] = useState(null);
    const [loading, setLoading] = useState(true);
    
    const { user } = useAuth(); 

    // Naye fix kiye gaye colors AI categories ke hisaab se
    const SENTIMENT_COLORS = {
        "Negative / Frustrated": "#EF4444", // Red
        "Positive / Calm": "#10B981",       // Green
        "Neutral / Confused": "#F59E0B",    // Yellow
        "Other": "#8B5CF6"                  // Purple
    };

    const BAR_COLORS = ['#3B82F6', '#6366F1', '#8B5CF6', '#EC4899'];

    useEffect(() => {
        if (user && user.token) {
            fetchAnalytics();
        }
    }, [user]); 

    const fetchAnalytics = async () => {
        try {
                const response = await fetch('http://localhost:5215/api/v1/Analytics/dashboard', {
                // const response = await fetch('http://34.93.237.221:5215/api/v1/Analytics/dashboard', {
                headers: { 
                    'Authorization': `Bearer ${user.token}` 
                }
            });

            if (!response.ok) throw new Error("Failed to fetch analytics data");

            const data = await response.json();

            //AI Sentiments ko group karna taaki Pie chart clean dikhe
            const groupedSentiment = {
                "Negative / Frustrated": 0,
                "Positive / Calm": 0,
                "Neutral / Confused": 0,
                "Other": 0
            };

            Object.keys(data.ticketsBySentiment || {}).forEach(key => {
                const text = key.toLowerCase();
                const count = data.ticketsBySentiment[key];

                if (text.includes("angry") || text.includes("frustrated") || text.includes("urgent") || text.includes("demanding") || text.includes("furious")) {
                    groupedSentiment["Negative / Frustrated"] += count;
                } else if (text.includes("calm") || text.includes("happy") || text.includes("satisfied") || text.includes("appreciative")) {
                    groupedSentiment["Positive / Calm"] += count;
                } else if (text.includes("confused") || text.includes("neutral") || text.includes("vague") || text.includes("delay")) {
                    groupedSentiment["Neutral / Confused"] += count;
                } else {
                    groupedSentiment["Other"] += count;
                }
            });

            // Sirf wahi categories bhejein jinka count 0 se zyada ho
            const sentimentData = Object.keys(groupedSentiment)
                .filter(key => groupedSentiment[key] > 0)
                .map(key => ({ name: key, value: groupedSentiment[key] }));

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

    // Custom label function percentages dikhane ke liye
    const renderCustomizedLabel = ({ cx, cy, midAngle, innerRadius, outerRadius, percent }) => {
        const radius = innerRadius + (outerRadius - innerRadius) * 0.5;
        const x = cx + radius * Math.cos(-midAngle * Math.PI / 180);
        const y = cy + radius * Math.sin(-midAngle * Math.PI / 180);
      
        return percent > 0 ? (
            <text x={x} y={y} fill="white" textAnchor="middle" dominantBaseline="central" className="font-bold text-xs">
                {`${(percent * 100).toFixed(0)}%`}
            </text>
        ) : null;
    };

    if (loading) return <div className="text-center p-10 text-xl font-bold mt-10">Loading Analytics... 📊</div>;
    if (!stats) return <div className="text-center p-10 text-red-500 mt-10">Failed to load data.</div>;

    return (
        <div className="relative h-[calc(100vh-4rem)] overflow-hidden bg-slate-100">
            <div className="relative z-10 h-full overflow-y-auto overflow-x-hidden p-6">
                <div className="max-w-7xl mx-auto">
                    <h1 className="text-3xl font-bold text-gray-800 mb-6 flex items-center gap-2">
                        📊 Support Analytics Dashboard
                    </h1>

                    {/* Top Stat Cards */}
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-10">
                        <div className="bg-white p-6 rounded-2xl shadow-lg border-l-4 border-blue-500 transform transition-transform hover:-translate-y-1">
                            <h2 className="text-gray-500 text-sm uppercase font-bold tracking-wider">Total Tickets</h2>
                            <p className="text-5xl font-extrabold text-gray-800 mt-2">{stats.totalTickets}</p>
                        </div>
                        <div className="bg-white p-6 rounded-2xl shadow-lg border-l-4 border-yellow-500 transform transition-transform hover:-translate-y-1">
                            <h2 className="text-gray-500 text-sm uppercase font-bold tracking-wider">Open Tickets</h2>
                            <p className="text-5xl font-extrabold text-yellow-500 mt-2">{stats.openTickets}</p>
                        </div>
                        <div className="bg-white p-6 rounded-2xl shadow-lg border-l-4 border-green-500 transform transition-transform hover:-translate-y-1">
                            <h2 className="text-gray-500 text-sm uppercase font-bold tracking-wider">Resolved Tickets</h2>
                            <p className="text-5xl font-extrabold text-green-500 mt-2">{stats.resolvedTickets}</p>
                        </div>
                    </div>

                    {/* Charts Section */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                        
                        {/* Clean Pie/Donut Chart */}
                        <div className="bg-white p-6 rounded-2xl shadow-lg">
                            <h3 className="text-xl font-bold text-gray-700 mb-6 border-b pb-2">Customer Sentiment (AI)</h3>
                            <ResponsiveContainer width="100%" height={320}>
                                <PieChart>
                                    <Pie 
                                        data={stats.sentimentData} 
                                        dataKey="value" 
                                        nameKey="name" 
                                        cx="50%" 
                                        cy="50%" 
                                        outerRadius={110} 
                                        innerRadius={60} // Isko donut chart banata hai
                                        labelLine={false} // Lambi lines hata di
                                        label={renderCustomizedLabel} // Sirf clean percentage dikhayega
                                        paddingAngle={2}
                                    >
                                        {stats.sentimentData.map((entry, index) => (
                                            <Cell key={`cell-${index}`} fill={SENTIMENT_COLORS[entry.name] || '#CBD5E1'} />
                                        ))}
                                    </Pie>
                                    <Tooltip 
                                        contentStyle={{ borderRadius: '10px', border: 'none', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
                                    />
                                    <Legend verticalAlign="bottom" height={36} wrapperStyle={{ paddingTop: '20px' }}/>
                                </PieChart>
                            </ResponsiveContainer>
                        </div>

                        {/* Bar Chart */}
                        <div className="bg-white p-6 rounded-2xl shadow-lg">
                            <h3 className="text-xl font-bold text-gray-700 mb-6 border-b pb-2">Tickets by Priority/Category</h3>
                            <ResponsiveContainer width="100%" height={320}>
                                <BarChart data={stats.categoryData} margin={{ top: 20, right: 30, left: 0, bottom: 5 }}>
                                    <XAxis dataKey="name" axisLine={false} tickLine={false} />
                                    <YAxis axisLine={false} tickLine={false} />
                                    <Tooltip 
                                        cursor={{fill: 'transparent'}}
                                        contentStyle={{ borderRadius: '10px', border: 'none', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
                                    />
                                    <Bar dataKey="value" radius={[6, 6, 6, 6]} barSize={50}>
                                        {stats.categoryData.map((entry, index) => (
                                            <Cell key={`cell-${index}`} fill={BAR_COLORS[index % BAR_COLORS.length]} />
                                        ))}
                                    </Bar>
                                </BarChart>
                            </ResponsiveContainer>
                        </div>

                    </div>
                </div>
            </div>
        </div>
    );
};

export default AnalyticsDashboard;