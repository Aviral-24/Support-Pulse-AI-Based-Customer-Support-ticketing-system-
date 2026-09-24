import { useState } from 'react';
import axios from 'axios';
import { useNavigate, Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { Eye, EyeOff, LockKeyhole, UserRound, ArrowRight, Mail } from 'lucide-react';

export default function Register() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [role, setRole] = useState('Customer');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await axios.post('http://localhost:5215/api/v1/Auth/register', { name, email, password, role });
      // await axios.post('http://34.93.237.221:5215/api/v1/Auth/register', { name, email, password, role });
      toast.success('Registration successful! Please login.');
      navigate('/login');
    } catch (error) {
      toast.error(error.response?.data || 'Registration failed.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="relative min-h-screen overflow-hidden bg-[#021b2b]">
      <div
        className="absolute inset-0 opacity-90"
        style={{
          backgroundImage:
            "linear-gradient(rgba(7,31,43,0.16), rgba(7,31,43,0.16)), repeating-linear-gradient(0deg, rgba(8,27,38,0.78) 0px, rgba(8,27,38,0.78) 14px, rgba(20,57,72,0.78) 14px, rgba(20,57,72,0.78) 28px)",
        }}
      />

      <div className="absolute left-1/2 top-0 h-16 w-40 -translate-x-1/2 rounded-b-[10px] border border-[#89d9f0]/20 bg-[#0b1e2c]/80 shadow-[0_0_30px_rgba(255,211,136,0.5)]" />
      <div className="absolute left-1/2 top-0 h-12 w-24 -translate-x-1/2 rounded-full bg-[#f8d99d] blur-[10px] opacity-80" />
      <div className="absolute left-1/2 top-14 h-36 w-36 -translate-x-1/2 rounded-full bg-[#f4c77a]/20 blur-[30px]" />

      <div className="relative z-10 flex min-h-screen items-center justify-center p-6">
        <div className="w-full max-w-[560px] rounded-[28px] border border-[#68dbf5]/20 bg-[rgba(14,33,42,0.72)] p-6 shadow-[0_25px_60px_rgba(0,0,0,0.4)] backdrop-blur-[2px] sm:p-8">
          <h1 className="mb-8 text-center text-[3rem] font-black leading-none tracking-[-0.06em] text-[#4ce3ef]">Sign up</h1>

          <form onSubmit={handleSubmit} className="space-y-5">
            <label className="relative block">
              <span className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-[#4ce3ef]">
                <UserRound size={22} />
              </span>
              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
                placeholder="Full Name"
                className="h-[58px] w-full rounded-[14px] border border-[#2a4d60] bg-[#1a2d3a]/80 pl-12 pr-4 text-base text-white placeholder:text-[#b8c9d4] outline-none transition focus:border-[#5feaf5] focus:ring-2 focus:ring-[#5feaf5]/20"
              />
            </label>

            <label className="relative block">
              <span className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-[#4ce3ef]">
                <Mail size={20} />
              </span>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                placeholder="Email"
                className="h-[58px] w-full rounded-[14px] border border-[#2a4d60] bg-[#1a2d3a]/80 pl-12 pr-4 text-base text-white placeholder:text-[#b8c9d4] outline-none transition focus:border-[#5feaf5] focus:ring-2 focus:ring-[#5feaf5]/20"
              />
            </label>

            <label className="relative block">
              <span className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-[#4ce3ef]">
                <LockKeyhole size={22} />
              </span>
              <input
                type={showPassword ? 'text' : 'password'}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                placeholder="Password"
                className="h-[58px] w-full rounded-[14px] border border-[#2a4d60] bg-[#1a2d3a]/80 pl-12 pr-12 text-base text-white placeholder:text-[#b8c9d4] outline-none transition focus:border-[#5feaf5] focus:ring-2 focus:ring-[#5feaf5]/20"
              />
              <button
                type="button"
                onClick={() => setShowPassword((prev) => !prev)}
                className="absolute right-4 top-1/2 -translate-y-1/2 text-[#d8ecef] transition hover:text-white"
                aria-label="Toggle password visibility"
              >
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </label>

            <select
              value={role}
              onChange={(e) => setRole(e.target.value)}
              className="h-[58px] w-full rounded-[14px] border border-[#2a4d60] bg-[#1a2d3a]/80 px-4 text-base text-white outline-none transition focus:border-[#5feaf5] focus:ring-2 focus:ring-[#5feaf5]/20"
            >
              <option value="Customer" className="text-slate-800">Customer</option>
            </select>

            <button
              type="submit"
              disabled={loading}
              className="mt-2 h-[58px] w-full rounded-[16px] bg-gradient-to-r from-[#4ce3ef] to-[#65d7f7] text-[1.05rem] font-bold text-[#0a2432] shadow-[0_12px_30px_rgba(76,227,239,0.35)] transition hover:brightness-105 disabled:opacity-80"
            >
              {loading ? 'Loading...' : 'Sign up'}
            </button>
          </form>

          <p className="mt-6 text-center text-[#dfeef3]">
            Already have an account?{' '}
            <Link to="/login" className="font-semibold text-[#4ce3ef] underline decoration-[#4ce3ef]/80 underline-offset-4">
              Login
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
