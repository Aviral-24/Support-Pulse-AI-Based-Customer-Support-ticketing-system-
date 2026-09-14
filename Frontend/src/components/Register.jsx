import { useState } from 'react';
import axios from 'axios';
import { useNavigate, Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import Galaxy from './Galaxy';

export default function Register() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await axios.post('http://localhost:5215/api/v1/Auth/register', { name, email, password });
      toast.success('Registration successful! Please login.');
      navigate('/login');
    } catch (error) {
      toast.error(error.response?.data || 'Registration failed.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="relative h-[calc(100vh-4rem)] flex items-center justify-center bg-slate-100 overflow-hidden">
      <Galaxy className="opacity-70" density={1.1} glowIntensity={0.4} saturation={0.6} />
      <div className="relative z-10 w-full max-w-md p-8 bg-white/90 backdrop-blur-md rounded-2xl shadow-xl border border-gray-200">
        <h2 className="text-3xl font-bold text-center text-gray-800 mb-6">Create Account</h2>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Full Name</label>
            <input type="text" required value={name} onChange={(e) => setName(e.target.value)}
              className="w-full border border-gray-300 p-3 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" placeholder="John Doe" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)}
              className="w-full border border-gray-300 p-3 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" placeholder="you@example.com" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Password</label>
            <input type="password" required value={password} onChange={(e) => setPassword(e.target.value)}
              className="w-full border border-gray-300 p-3 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" placeholder="••••••••" />
          </div>
          <button type="submit" disabled={loading} className="w-full bg-blue-600 text-white font-bold py-3 rounded-lg hover:bg-blue-700 transition">
            {loading ? 'Creating Account...' : 'Register'}
          </button>
        </form>
        <p className="mt-4 text-center text-sm text-gray-600">
          Already have an account? <Link to="/login" className="text-blue-600 font-semibold hover:underline">Login here</Link>
        </p>
      </div>
    </div>
  );
}