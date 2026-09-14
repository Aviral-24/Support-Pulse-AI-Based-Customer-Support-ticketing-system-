import { useState } from 'react';
import axios from 'axios';
import { useNavigate, Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useAuth } from '../context/AuthContext';
import Galaxy from './Galaxy';

export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { login } = useAuth();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      // Backend ko login request (Dhyan rakhein port 5215 hai)
      const response = await axios.post('http://localhost:5215/api/v1/Auth/login', { email, password });
      
      const { token, role } = response.data;
      login(token, role); // Context me save kiya
      
      toast.success('Logged in successfully!');
      
      // Role ke hisaab se redirect
      if (role === 'Agent' || role === 'Admin') {
        navigate('/dashboard');
      } else {
        navigate('/'); // Customer ko ticket form par bhejenge
      }
    } catch (error) {
      toast.error(error.response?.data || 'Login failed. Please check credentials.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="relative h-[calc(100vh-4rem)] flex items-center justify-center bg-slate-100 overflow-hidden">
      <Galaxy className="opacity-70" density={1.1} glowIntensity={0.4} saturation={0.6} />
      <div className="relative z-10 w-full max-w-md p-8 bg-white/90 backdrop-blur-md rounded-2xl shadow-xl border border-gray-200">
        <h2 className="text-3xl font-bold text-center text-gray-800 mb-6">Welcome Back</h2>
        <form onSubmit={handleSubmit} className="space-y-5">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)}
              className="w-full border border-gray-300 p-3 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" placeholder="agent@support.com" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Password</label>
            <input type="password" required value={password} onChange={(e) => setPassword(e.target.value)}
              className="w-full border border-gray-300 p-3 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" placeholder="••••••••" />
          </div>
          <button type="submit" disabled={loading} className="w-full bg-blue-600 text-white font-bold py-3 rounded-lg hover:bg-blue-700 transition">
            {loading ? 'Authenticating...' : 'Login'}
          </button>
        </form>
        <p className="mt-4 text-center text-sm text-gray-600">
          Don't have an account? <Link to="/register" className="text-blue-600 font-semibold hover:underline">Register here</Link>
        </p>
      </div>
    </div>
  );
}